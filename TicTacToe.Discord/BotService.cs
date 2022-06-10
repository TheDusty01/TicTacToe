using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TicTacToe.Discord
{
    public class BotService : IHostedService
    {
        private readonly ILogger<BotService> logger;
        private readonly DiscordClient discordClient;
        private readonly SlashCommandsExtension slash;

        public static bool IsReady { get; private set; } = false;

        #region Init
        public BotService(ILogger<BotService> logger, DiscordClient discordClient, SlashCommandsExtension slash)
        {
            this.logger = logger;
            this.discordClient = discordClient;
            this.slash = slash;
        }

        public async Task StartAsync(CancellationToken stoppingToken)
        {
            ConfigureSlashCommands();

            discordClient.GuildDownloadCompleted += DiscordClient_GuildDownloadCompleted;
            await discordClient.ConnectAsync(new DiscordActivity("Tic Tac Toe!", ActivityType.Playing), UserStatus.Online).ConfigureAwait(false);
            await discordClient.InitializeAsync().ConfigureAwait(false);
        }

        private Task DiscordClient_GuildDownloadCompleted(DiscordClient sender, GuildDownloadCompletedEventArgs e)
        {
            IsReady = true;
            logger.LogInformation("Bot started.");
            return Task.CompletedTask;
        }

        private void ConfigureSlashCommands()
        {
            slash.RegisterCommands<GameCommands>();
        }
        #endregion

        public async Task StopAsync(CancellationToken stoppingToken)
        {
            IsReady = false;
            await discordClient.DisconnectAsync().ConfigureAwait(false);
        }

        public virtual void Dispose()
        {
            discordClient.Dispose();
        }
    }
}
