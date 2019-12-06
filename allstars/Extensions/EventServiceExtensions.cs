using allstars.Models;
using allstars.Repositories;
using allstars.Services;
using allstars.Utils;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Extensions
{
    public static class EventServiceExtensions
    {
        public static async Task<bool> IsAccAgeOkAsync(this EventService eventService, SocketGuildUser socketGuildUser, IRepositoryWrapper repositoryWrapper, int accAge, Logger Log, IConfigurationRoot config)
        {
            if (accAge > 0)
            {
                DateTimeOffset startTime = DateTimeOffset.Now;
                DateTimeOffset endTime = socketGuildUser.CreatedAt;

                TimeSpan differenceInHours = startTime.Subtract(endTime);

                if (Math.Abs(differenceInHours.TotalHours) <= accAge)
                {
                    var dmChannel = await socketGuildUser.GetOrCreateDMChannelAsync();
                    if (dmChannel != null)
                    {
                        try
                        {
                            await dmChannel.SendMessageAsync($"You have been kicked from the server for having an account younger than what the server allows.\nYour account has to be at least **{accAge}** hours old.").ConfigureAwait(false);
                        }
                        catch
                        {
                        }
                    }

                    var embKicked = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"User **{socketGuildUser.Username}** | **{socketGuildUser.DiscriminatorValue}** | **{socketGuildUser.Id}** | has tried to join the discord but has been kicked due to account age being below the threshol of {accAge} hours.\n\nCreation Date: {socketGuildUser.CreatedAt.ToString()}, Joined at: {DateTime.Now.ToString()}",
                        Footer = new EmbedFooterBuilder().WithIconUrl(config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await socketGuildUser.KickAsync($"Account too young. Account {Math.Round(differenceInHours.TotalHours, 3)} hours old but required are {accAge} hours.").ConfigureAwait(false);

                    var logSpecialChannel = await repositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
                    if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
                        Log.Error($"Log channel has not been set.");
                    else
                    {
                        var embNotify = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName(socketGuildUser.Username).WithIconUrl(socketGuildUser.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                            Color = Constants.InfoColor,
                            Description = $"Has tried to join the discord but was kicked due to an account age below the threshold of **{accAge}** hours.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        if (socketGuildUser.Guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                            await guildLogChannel.SendMessageAsync("", false, embNotify.Build()).ConfigureAwait(false);
                    }

                    return await Task.FromResult(false);
                }
            }

            return true;
        }

        public static async Task ApplyMuteIfNeededAsync(this EventService eventService, SocketGuildUser socketGuildUser, IRepositoryWrapper repositoryWrapper)
        {
            var mute = await repositoryWrapper.MuteRepository.GetActiveMuteByUserIdAsync(socketGuildUser.Id);
            if (!mute.IsObjectNull() && !mute.IsEmpty())
            {
                var role = socketGuildUser.Guild.Roles.Where(x => x.Name.ToLower().CompareTo("muted") == 0).FirstOrDefault();

                if (role != null)
                    await socketGuildUser.AddRoleAsync(role).ConfigureAwait(false);
            }
        }

        public static async Task UpdateOrCreateUser(this EventService eventService, SocketGuildUser socketGuildUser, IRepositoryWrapper repositoryWrapper, bool joined)
        {
            var user = await repositoryWrapper.UserRepository.GetUserByIdAsync(socketGuildUser.Id);
            if (user.IsObjectNull() || user.IsEmpty())
            {
                user = new User()
                {
                    Id = socketGuildUser.Id,
                    Guild = socketGuildUser.Guild.Id,
                    Discriminator = socketGuildUser.DiscriminatorValue,
                    Name = socketGuildUser.Username,
                    AvatarUrl = socketGuildUser.GetAvatarUrl(),
                    Joined = DateTime.UtcNow,
                    Left = new DateTime(),
                    MuteExpires = new DateTime()
                };

                repositoryWrapper.UserRepository.AddUser(user);
                repositoryWrapper.UserRepository.SaveChanges();
            }
            else
            {
                if (joined)
                    user.Joined = DateTime.UtcNow;
                else
                    user.Left = DateTime.UtcNow;

                repositoryWrapper.UserRepository.UpdateUser(user);
                repositoryWrapper.UserRepository.SaveChanges();
            }
        }

        public static async Task SendWelcomeMessage(this EventService eventService, SocketGuildUser socketGuildUser)
        {
            var dmChannel = await socketGuildUser.GetOrCreateDMChannelAsync();
            if (dmChannel != null)
            {
                try
                {
                    await dmChannel.SendMessageAsync(Messages.Welcome).ConfigureAwait(false);
                }
                catch
                {
                }
            }
        }

        public static async Task LogJoinLeftEvent(this EventService eventService, SocketGuildUser socketGuildUser, IRepositoryWrapper repositoryWrapper, Logger Log, string joinedOrLeft, IConfigurationRoot config)
        {
            var logSpecialChannel = await repositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
            if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
                Log.Error($"Log channel has not been set.");
            else
            {
                var embJoined = new EmbedBuilder()
                {
                    Color = joinedOrLeft.CompareTo("joined") == 0 ? Constants.SuccessColor : Constants.FailureColor,
                    Description = string.Format("User **{0} | {1} | {2}** {3}. Bot: {4}", socketGuildUser.Username, socketGuildUser.DiscriminatorValue, socketGuildUser.Id, joinedOrLeft, socketGuildUser.IsBot),
                    Footer = new EmbedFooterBuilder().WithIconUrl(config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (socketGuildUser.Guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync("", false, embJoined.Build()).ConfigureAwait(false);
                else
                {
                    Log.Error($"Log channel could not be found.");
                }
            }
        }

        public static async Task ArchiveMessage(this EventService eventService, Cacheable<IMessage, ulong> message, ISocketMessageChannel channel, IRepositoryWrapper repositoryWrapper, Logger Log, IConfigurationRoot config)
        {
            var archiveSpecialChannel = await repositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("archive");
            if (archiveSpecialChannel.IsObjectNull() || archiveSpecialChannel.IsEmpty())
                return;
            else
            {
                var guildChannel = (channel as IGuildChannel);
                var deletedMessage = await message.GetOrDownloadAsync();

                if (archiveSpecialChannel.Id == channel.Id)
                    return;

                if (deletedMessage.Author.Id == ulong.Parse(config[Constants.ConfigBotId]))
                    return;

                if (deletedMessage.Content.StartsWith("!"))
                    return;

                var embDeleted = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName($"{deletedMessage.Author.Username}#{deletedMessage.Author.DiscriminatorValue}'s | {deletedMessage.Author.Id} | message has been deleted in channel " + channel.Name).WithIconUrl(config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.InfoColor,
                    Description = deletedMessage.Content,
                    Footer = new EmbedFooterBuilder().WithIconUrl(config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (await guildChannel.Guild.GetChannelAsync(archiveSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync("", false, embDeleted.Build()).ConfigureAwait(false);
                else
                {
                    Log.Error($"Archive channel could not be found.");
                }
            }
        }

        public static async Task LogOnMessageUpdated(this EventService eventService, Cacheable<IMessage, ulong> oldMessage, SocketMessage newMessage, ISocketMessageChannel channel, IRepositoryWrapper repositoryWrapper, Logger Log, IConfigurationRoot config)
        {
            var archiveSpecialChannel = await repositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("archive");
            if (archiveSpecialChannel.IsObjectNull() || archiveSpecialChannel.IsEmpty())
                return;
            else
            {
                var guildChannel = (channel as IGuildChannel);

                if (!oldMessage.HasValue || newMessage == null || channel == null)
                    return;

                var author = oldMessage.Value.Author;
                if (author == null)
                    author = newMessage.Author;

                if (author == null)
                    return;

                var embDeleted = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName($"{author.Username}#{author.DiscriminatorValue}'s | {author.Id} | has updated his message in " + channel.Name).WithIconUrl(config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.InfoColor,
                    Description = $"**Old Message:**\n{oldMessage.Value.Content}\n\n**Updated message:**\n{newMessage.Content}",
                    Footer = new EmbedFooterBuilder().WithIconUrl(config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (await guildChannel.Guild.GetChannelAsync(archiveSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync("", false, embDeleted.Build()).ConfigureAwait(false);
                else
                {
                    Log.Error($"Archive channel could not be found.");
                }
            }
        }
    }
}