using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.Discord
{
    public class GameService
    {
        private readonly Dictionary<ulong, Game> games = new();
        private readonly ILogger<GameService> logger;

        public GameService(ILogger<GameService> logger, DiscordClient discordClient)
        {
            discordClient.ComponentInteractionCreated += DiscordClient_ComponentInteractionCreated;
            this.logger = logger;
        }

        private Task DiscordClient_ComponentInteractionCreated(DiscordClient sender, ComponentInteractionCreateEventArgs e)
        {
            logger.LogTrace("ComponentInteractionCreated: {ComponentType}", e.Interaction.Data.ComponentType);

            if (e.Interaction.Data.ComponentType != ComponentType.Button)
                return Task.CompletedTask;

            _ = Task.Run(async () =>
            {
                // Check if custom id is valid
                string customId = e.Interaction.Data.CustomId;
                string[] guidAndField = customId.Split('_');
                if (guidAndField.Length == 2 && byte.TryParse(guidAndField[1], out byte field) && (field >= 0 && field < 9))
                {
                    // Find game
                    Game game = null;
                    lock (games)
                        games.TryGetValue(e.Message.Id, out game);

                    // Should only happen if win message coudln't bet updated (will most likely never happen)
                    if (game == null || game.Board.IsGameOver())
                    {
                        // Disable all buttons
                        foreach (DiscordActionRowComponent row in e.Message.Components)
                        {
                            foreach (DiscordButtonComponent button in row.Components)
                            {
                                if (button != null)
                                {
                                    button.Disable();
                                }
                            }
                        }

                        DiscordEmbed embed;
                        if (game == null || e.Message.Embeds == null || e.Message.Embeds.Count == 0)
                        {
                            embed = new DiscordEmbedBuilder().WithTitle("Tic Tac Toe").WithDescription("Game is invlaid, please start a new one.");
                        }
                        else
                        {
                            embed = e.Message.Embeds[0];
                        }

                        // Update game embed
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder()
                                .AddEmbed(embed)
                                .AddComponents(e.Message.Components))
                            .ConfigureAwait(false);

                        return;
                    }

                    // Check if it's the callers turn
                    if (e.User.Id != game.CurrentTurn.Id)
                    {
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                            new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("It's not your turn!")).ConfigureAwait(false);
                        return;
                    }

                    try
                    {
                        if (game.Board.MakeMove(field) && game.IsSolo && !game.IsPlayer1Turn)
                        {
                            logger.LogInformation("{GameId}: Set player {piece} at {field}", game.Message.Id, game.Board.GetLastTurn(), field);

                            // Make AI move
                            if (game.MakeAiMove())
                            {
                                logger.LogInformation("{GameId}: Set AI {piece}", game.Message.Id, game.Board.GetLastTurn());
                            }
                        }

                        // Remove game from active games
                        if (game.Board.IsGameOver())
                        {
                            lock (games)
                                games.Remove(game.Message.Id);

                            logger.LogInformation("{GameId}: Game is over: {Winner}", game.Message.Id, game.Board.Winner);
                        }

                        var webhookBuilder = GenerateBoard(game);
                        await e.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage,
                            new DiscordInteractionResponseBuilder().AddComponents(webhookBuilder.Components).AddEmbeds(webhookBuilder.Embeds)).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        // Revoke player move
                        if (game.Board.RevokeLastMove() && game.IsSolo && !game.IsPlayer1Turn)
                        {
                            // Revoke AI move
                            game.Board.RevokeLastMove();
                        }

                        logger.LogWarning(ex, "{GameId}: Failed to update game message, revoked move(s)", game.Message.Id);
                    }
                }
                else
                {
                    logger.LogWarning("Recieved custom id of button couldn't be processed: {customId}", customId);
                    await e.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource,
                        new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("Something went wrong, but it's not your fault :(")).ConfigureAwait(false);
                }


            });

            return Task.CompletedTask;
        }

        public async Task StartGame(InteractionContext ctx, DiscordUser p1, DiscordUser p2)
        {
            Game game = new(ctx.Client.CurrentUser, p1, p2);

            var msg = await ctx.EditResponseAsync(GenerateBoard(game)).ConfigureAwait(false);
            game.Message = msg;
            lock (games)
                games.Add(msg.Id, game);
        }

        private static DiscordWebhookBuilder GenerateBoard(Game game)
        {
            bool isGameOver = game.Board.IsGameOver();
            // Calculate buttons
            IList<DiscordActionRowComponent> rows = new List<DiscordActionRowComponent>(3);
            IReadOnlyList<byte> possibleMoves = game.Board.GetPossibleMoves();
            for (int i = 0; i < 3; i++)
            {
                var buttons = new List<DiscordButtonComponent>();

                for (int j = 0; j < 3; j++)
                {
                    int field = j + 3 * i;

                    bool disabled = isGameOver || !possibleMoves.Contains((byte)field);

                    Piece pieceAtField = game.Board.GetPiece((byte)field);
                    DiscordComponentEmoji emojiAtField = new DiscordComponentEmoji(Game.PieceToEmoji(pieceAtField));
                    if (pieceAtField == Piece.Empty)
                        buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"{Guid.NewGuid():N}_{field}", $" ", disabled));
                    else
                        buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"{Guid.NewGuid():N}_{field}", $"", disabled, emojiAtField));

                }

                rows.Add(new DiscordActionRowComponent(buttons));
            }

            var builder = new DiscordEmbedBuilder()
                .WithTitle("Tic Tac Toe");


            var webhookBuilder = new DiscordWebhookBuilder()
                .AddComponents(rows);

            DiscordUser currentTurnUser = game.GetCurrentTurn();
            string gameStatus;
            if (isGameOver)
            {
                DiscordUser winner = game.GetWinner();
                if (winner == null)
                {
                    gameStatus = "It's a draw!";
                }
                else
                {
                    gameStatus = $"{winner.Mention} won the game!";

                    //webhookBuilder.AddMention(new UserMention(winner));
                }
            }
            else
            {
                gameStatus = $"It's {currentTurnUser.Mention} ({Game.PieceToEmoji(game.Board.CurrentTurn)}) turn!";

                //webhookBuilder.AddMention(new UserMention(currentTurnUser));
            }

            builder.WithDescription(gameStatus + "\n\n" + $"{game.Player1.Mention} vs. {game.Player2.Mention}");

            // Add embed to message
            webhookBuilder.AddEmbed(builder.Build());

            return webhookBuilder;
        }

        class Game
        {
            public const Piece Player1Piece = Piece.Cross;
            public const Piece Player2Piece = Piece.Circle;

            private static readonly Dictionary<Piece, DiscordEmoji> pieceEmojis = new()
            {
                { Piece.Empty, DiscordEmoji.FromUnicode(DiscordBoard.EmptyEmoji) },
                { Piece.Cross, DiscordEmoji.FromUnicode(DiscordBoard.CrossEmoji) },
                { Piece.Circle, DiscordEmoji.FromUnicode(DiscordBoard.CircleEmoji) }
            };

            private readonly Random rng = new();

            public Game(DiscordUser botUser, DiscordUser p1, DiscordUser p2)
            {
                Board = new DiscordBoard();
                Player1 = p1;

                if (p2 == null || p2.Id == botUser.Id)
                {
                    Board.CurrentTurn = Player1Piece;
                    Player2 = botUser;
                    IsSolo = true;
                }
                else
                {
                    Player2 = p2;
                    IsSolo = false;
                }
            }

            public static DiscordEmoji PieceToEmoji(Piece piece)
            {
                return pieceEmojis[piece];
            }

            public DiscordUser GetCurrentTurn()
            {
                return IsPlayer1Turn ? Player1 : Player2;
            }

            public DiscordUser GetWinner()
            {
                if (Board.Winner == Player1Piece)
                {
                    return Player1;
                }
                else if (Board.Winner == Player2Piece)
                {
                    return Player2;
                }

                return null;
            }

            public bool MakeAiMove()
            {
                lock (rng)
                {
                    IReadOnlyList<byte> moves = Board.GetPossibleMoves();
                    if (moves.Count == 0)
                        return false;

                    return Board.MakeMove(moves[rng.Next(moves.Count)]);
                }
            }

            public DiscordMessage Message { get; set; }
            public BaseBoard Board { get; private set; }

            public DiscordUser Player1 { get; private set; }
            public DiscordUser Player2 { get; private set; }

            public bool IsSolo { get; private set; }
            public bool IsPlayer1Turn { get => Board.CurrentTurn == Player1Piece; }

            public DiscordUser CurrentTurn { get => IsPlayer1Turn ? Player1 : Player2; }

            public class Player
            {
                public DiscordUser User { get; set; }
            }
        }
    }
}
