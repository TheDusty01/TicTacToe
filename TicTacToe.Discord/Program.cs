using DSharpPlus;
using DSharpPlus.Menus;
using DSharpPlus.SlashCommands;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using TicTacToe.Game;

namespace TicTacToe.Discord
{
    class Program
    {
        static Task Main(string[] args) =>
            CreateHostBuilder(args).Build().RunAsync();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .UseConsoleLifetime()
                .ConfigureLogging((hostContext, options) =>
                {
                    LogLevel appLogLevel;
                    if (hostContext.HostingEnvironment.IsDevelopment())
                    {
                        appLogLevel = LogLevel.Trace;
                    }
                    else if (hostContext.HostingEnvironment.IsStaging())
                    {
                        appLogLevel = LogLevel.Debug;
                    }
                    else
                    {
                        appLogLevel = LogLevel.Information;
                    }

                    options.SetMinimumLevel(appLogLevel);
                })

                // Configure services
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton(s =>
                    {
                        if (!Enum.TryParse(hostContext.Configuration["Logging:LogLevel:Discord"], out LogLevel discordLogLevel))
                        {
                            discordLogLevel = LogLevel.Information;
                        }
                        return new DiscordClient(new DiscordConfiguration
                        {
                            LoggerFactory = s.GetRequiredService<ILoggerFactory>(),
                            Token = hostContext.Configuration["Discord:Token"],
                            MinimumLogLevel = discordLogLevel
                        });
                    })
                    .AddSingleton(s => s.GetRequiredService<DiscordClient>().UseSlashCommands(new SlashCommandsConfiguration
                    {
                        Services = s
                    }))
                    .AddSingleton(s => s.GetRequiredService<DiscordClient>().UseMenus())
                    
                    // Add services that depend on discord client
                    .AddSingleton<GameService>()

                    // Add hosted service
                    .AddHostedService<BotService>();

                });
        }
    }
}
