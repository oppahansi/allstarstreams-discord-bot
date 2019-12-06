using allstars.Extensions;
using allstars.Repositories;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Threading.Tasks;

namespace allstars.Services
{
    public class EventService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly DiscordSocketClient DiscordClient;
        private readonly CommandService CommandService;
        private readonly IServiceProvider ServiceProvider;
        private readonly AutoMessageService AutoMessageService;
        private readonly IConfigurationRoot Config;
        private IRepositoryWrapper RepositoryWrapper;

        public EventService(DiscordSocketClient discordSocketClient, CommandService commandService, IServiceProvider serviceProvider, IConfigurationRoot config, IRepositoryWrapper repositoryWrapper, AutoMessageService autoMessageService)
        {
            DiscordClient = discordSocketClient;
            CommandService = commandService;
            ServiceProvider = serviceProvider;
            AutoMessageService = autoMessageService;
            Config = config;
            RepositoryWrapper = repositoryWrapper;

            DiscordClient.UserJoined += OnUserJoinedAsync;
            DiscordClient.UserLeft += OnUserLeftAsync;
            DiscordClient.MessageReceived += OnMessageReceivedAsync;
            DiscordClient.MessageDeleted += OnUserDeletedMessage;
            DiscordClient.MessageUpdated += OnMessageUpdatedAsync;
            DiscordClient.Ready += OnReadyAsync;
        }

        private async Task OnReadyAsync()
        {
            await AutoMessageService.StartTimersAsync();
        }

        private async Task OnUserJoinedAsync(SocketGuildUser socketGuildUser)
        {
            if (socketGuildUser == null)
                return;

            if (!(await this.IsAccAgeOkAsync(socketGuildUser, RepositoryWrapper, Config.GetValue<int>(Constants.ConfigMinAccAge), Log, Config)))
                return;

            await this.UpdateOrCreateUser(socketGuildUser, RepositoryWrapper, true).ConfigureAwait(false);
            await this.ApplyMuteIfNeededAsync(socketGuildUser, RepositoryWrapper).ConfigureAwait(false);
            await this.SendWelcomeMessage(socketGuildUser).ConfigureAwait(false);
            await this.LogJoinLeftEvent(socketGuildUser, RepositoryWrapper, Log, "joined", Config).ConfigureAwait(false);
        }

        private async Task OnUserLeftAsync(SocketGuildUser socketGuildUser)
        {
            await this.UpdateOrCreateUser(socketGuildUser, RepositoryWrapper, false).ConfigureAwait(false);
            await this.LogJoinLeftEvent(socketGuildUser, RepositoryWrapper, Log, "left", Config).ConfigureAwait(false);
        }

        private async Task OnMessageReceivedAsync(SocketMessage socketMessage)
        {
            if (!(socketMessage is SocketUserMessage message)) return;
            if (message.Author == DiscordClient.CurrentUser) return;

            var argPos = 0;
            if (message.HasStringPrefix(Config[Constants.ConfigCommandPrefix], ref argPos))
                await ProcessCommand(message, argPos).ConfigureAwait(false);
            else
                await ProcessMessage(message).ConfigureAwait(false);
        }

        private async Task ProcessCommand(SocketUserMessage message, int argPos)
        {
            var context = new SocketCommandContext(DiscordClient, message);

            Log.Info(Messages.UserExecutingCommand, message.Channel?.Name, context.Guild?.Name, message.Author.Username, message.Content);
            var result = await CommandService.ExecuteAsync(context, argPos, ServiceProvider);

            if (!result.IsSuccess)
            {
                Log.Error(result.ToString());
                await message.DeleteAsync().ConfigureAwait(false);
                await message.Channel.SendMessageAsync("Command could not be executed. Reason:\n" + result.ErrorReason).ConfigureAwait(false);
            }
        }

        private async Task ProcessMessage(SocketUserMessage message)
        {
            if (AutoMessageService.ContainsChannel(message.Channel.Id) && message.Author.Id != ulong.Parse(Config[Constants.ConfigBotId]))
                await AutoMessageService.MessageReceived(message.Channel as ITextChannel).ConfigureAwait(false);
        }

        private async Task OnUserDeletedMessage(Cacheable<IMessage, ulong> message, ISocketMessageChannel channel)
        {
            if (Config.GetValue<int>(Constants.ConfigArchiving) == 0)
                return;

            await this.ArchiveMessage(message, channel, RepositoryWrapper, Log, Config).ConfigureAwait(false);
        }

        private async Task OnMessageUpdatedAsync(Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel)
        {
            if (newMessage.Author.Id == ulong.Parse(Config[Constants.ConfigBotId]))
                return;

            if (newMessage == null || string.IsNullOrEmpty(newMessage.Content) || !oldMessage.HasValue || newMessage.Content.CompareTo(oldMessage.Value.Content) == 0)
                return;

            await this.LogOnMessageUpdated(oldMessage, newMessage, channel, RepositoryWrapper, Log, Config).ConfigureAwait(false);
        }
    }
}