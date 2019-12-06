using allstars.Utils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.Reflection;
using System.Threading.Tasks;

namespace allstars.Services
{
    public class StartupService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient DiscordClient;
        private readonly CommandService CommandService;
        private readonly IConfigurationRoot Config;
        private readonly IServiceProvider ServiceProvider;

        public StartupService(DiscordSocketClient discordClient, CommandService commandService, IConfigurationRoot config, IServiceProvider serviceProvider)
        {
            Config = config;
            DiscordClient = discordClient;
            CommandService = commandService;
            ServiceProvider = serviceProvider;
        }

        public async Task StartAsync()
        {
#if DEBUG
            string discordToken = Config[Constants.ConfigDevBotToken];
#endif
#if RELEASE
            string discordToken = Config[Constants.ConfigLiveBotToken];
#endif
            if (string.IsNullOrWhiteSpace(discordToken))
            {
                Log.Fatal(Messages.MissingBotToken);
                throw new Exception();
            }

            Log.Info(Messages.StartingBot);

            await DiscordClient.LoginAsync(TokenType.Bot, discordToken);
            await DiscordClient.StartAsync();

            await CommandService.AddModulesAsync(Assembly.GetEntryAssembly(), ServiceProvider);
        }
    }
}