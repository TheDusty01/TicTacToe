using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe.Discord
{
    public class GameCommands : ApplicationCommandModule
    {
        private readonly ILogger<GameCommands> logger;
        private readonly GameService gameService;

        public GameCommands(ILogger<GameCommands> logger, GameService gameService)
        {
            this.logger = logger;
            this.gameService = gameService;
        }


        [SlashCommand("tictactoe", "Start a game of Tic Tac Toe!")]
        public async Task TicTacToeCommand(InteractionContext ctx, [Option("opponent", "Your opponent")] DiscordUser opponent = null)
        {
            await ctx.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource);
            try
            {
                await gameService.StartGame(ctx, ctx.Member, opponent);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Failed to start a game: {Member}", ctx.Member);
                await ctx.EditResponseAsync(new DiscordWebhookBuilder().WithContent("Huh.. something went wrong")).ConfigureAwait(false);
            }
        }
    }
}
