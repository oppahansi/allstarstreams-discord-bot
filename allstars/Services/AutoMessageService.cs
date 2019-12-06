using allstars.Models;
using allstars.Repositories;
using allstars.Utils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace allstars.Services
{
    public class AutoMessageService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient DiscordClient;
        private readonly IConfigurationRoot Config;
        private IRepositoryWrapper RepositoryWrapper;
        private Dictionary<ulong, Timer> Timers;
        private Dictionary<ulong, int> MessagesCounts;
        private Dictionary<ulong, DateTime> LastPostedTimes;

        public AutoMessageService(DiscordSocketClient client, IConfigurationRoot config, IRepositoryWrapper repositoryWrapper)
        {
            DiscordClient = client;
            Config = config;
            RepositoryWrapper = repositoryWrapper;
        }

        public async Task StartTimersAsync()
        {
            Log.Info("Starting auto messages..");

            Timers = new Dictionary<ulong, Timer>();
            MessagesCounts = new Dictionary<ulong, int>();
            LastPostedTimes = new Dictionary<ulong, DateTime>();

            var autoMessages = await RepositoryWrapper.AutoMessageRepository.GetAllAutoMessagesAsync();

            foreach (var autoMessage in autoMessages)
            {
                var timer = new Timer(_ =>
                {
                    var tasks = new List<Task>();
                    tasks.Add(Task.Factory.StartNew(async () => { await PostMessage(autoMessage).ConfigureAwait(false); }));
                },
                null,
                TimeSpan.FromMinutes(autoMessage.TimeInterval),
                TimeSpan.FromMinutes(autoMessage.TimeInterval));

                Timers.Add(autoMessage.Channel, timer);
                MessagesCounts.Add(autoMessage.Channel, 0);
                LastPostedTimes.Add(autoMessage.Channel, DateTime.Now);
            }

            Log.Info("Starting auto messages.. done.");
        }

        private async Task PostMessage(AutoMessage autoMessage)
        {
            if (autoMessage != null)
            {
                var channel = DiscordClient.GetChannel(autoMessage.Channel) as ITextChannel;
                if (channel != null)
                {
                    Log.Info($"Posting auto message in {channel.Name} channel.");

                    var emb = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"{autoMessage.Message}",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    if (autoMessage.MessagesInterval > 0)
                        MessagesCounts[autoMessage.Channel] = 0;
                    
                    LastPostedTimes[channel.Id] = DateTime.Now;

                    await channel.SendMessageAsync("", false, emb.Build()).ConfigureAwait(false);
                }
                else
                    Log.Error($"Posting auto message failed, channel null.\nChannel id: {autoMessage.Channel}");
            }
            else
                Log.Error($"Posting auto message failed, autoMessage is null.");            
        }

        public async Task MessageReceived(ITextChannel channel)
        {
            if (channel != null)
            {
                var autoMessage = await RepositoryWrapper.AutoMessageRepository.GetAutoMessageAsync(channel.Id);
                if (autoMessage == null || autoMessage.Channel == 0 || autoMessage.MessagesInterval == 0)
                    return;

                MessagesCounts[channel.Id] += 1;

                if (MessagesCounts[channel.Id] >= autoMessage.MessagesInterval)
                {
                    MessagesCounts[channel.Id] = 0;
                    Timers[channel.Id].Change(autoMessage.TimeInterval * 60 * 1000, autoMessage.TimeInterval * 60 * 1000);
                    LastPostedTimes[autoMessage.Channel] = DateTime.Now;

                    var emb = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"{autoMessage.Message}",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await channel.SendMessageAsync("", false, emb.Build()).ConfigureAwait(false);
                }
            }
        }

        public async Task AddAutoMessageAsync(AutoMessage autoMessage)
        {
            if (autoMessage != null)
            {
                await RepositoryWrapper.AutoMessageRepository.AddAutoMessageAsync(autoMessage);
                RepositoryWrapper.AutoMessageRepository.SaveChanges();

                var timer = new Timer(_ =>
                {
                    var tasks = new List<Task>();
                    tasks.Add(Task.Factory.StartNew(async () => { await PostMessage(autoMessage).ConfigureAwait(false); }));
                },
                null,
                TimeSpan.FromMinutes(autoMessage.TimeInterval),
                TimeSpan.FromMinutes(autoMessage.TimeInterval));

                Timers.Add(autoMessage.Channel, timer);
                MessagesCounts.Add(autoMessage.Channel, 0);
                LastPostedTimes.Add(autoMessage.Channel, DateTime.Now);

                var channel = DiscordClient.GetChannel(autoMessage.Channel) as ITextChannel;
                
                var emb = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"{autoMessage.Message}",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await channel.SendMessageAsync("", false, emb.Build()).ConfigureAwait(false);
            }
        }

        public async Task UpdateAutoMessageAsync(AutoMessage autoMessage)
        {
            if (autoMessage != null)
            {
                RepositoryWrapper.AutoMessageRepository.UpdateAutoMessage(autoMessage);
                RepositoryWrapper.AutoMessageRepository.SaveChanges();
                
                Timers[autoMessage.Channel].Change(autoMessage.TimeInterval * 60 * 1000, autoMessage.TimeInterval * 60 * 1000);
                MessagesCounts[autoMessage.Channel] =  0;
                LastPostedTimes[autoMessage.Channel] = DateTime.Now;

                var channel = DiscordClient.GetChannel(autoMessage.Channel) as ITextChannel;
                
                var emb = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"{autoMessage.Message}",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await channel.SendMessageAsync("", false, emb.Build()).ConfigureAwait(false);
            }
        }

        public void RemoveAutoMessage(AutoMessage autoMessage)
        {
            if (autoMessage != null && ContainsChannel(autoMessage.Channel))
            {
                RepositoryWrapper.AutoMessageRepository.DeleteAutoMessage(autoMessage);
                RepositoryWrapper.AutoMessageRepository.SaveChanges();

                Timers[autoMessage.Channel].Dispose();
                Timers.Remove(autoMessage.Channel);
                MessagesCounts.Remove(autoMessage.Channel);
                LastPostedTimes.Remove(autoMessage.Channel);
            }
        }

        public Boolean ContainsChannel(ulong channelId)
        {
            return Timers.ContainsKey(channelId) || MessagesCounts.ContainsKey(channelId);
        }

        public double GetRemainingTime(ulong channelId, int timeInterval)
        {
            var startingTime = LastPostedTimes[channelId];
            var nextPostingTime = startingTime.AddMinutes(timeInterval);

            return (nextPostingTime - DateTime.Now).TotalMinutes;
        }
    }
}