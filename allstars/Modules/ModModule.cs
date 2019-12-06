using allstars.Extensions;
using allstars.Models;
using allstars.Repositories;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace allstars.Modules
{
    [RequireContext(ContextType.Guild)]
    public class ModModule : ModuleBase<SocketCommandContext>
    {
        public enum DeleteType
        {
            self = 0,
            bot = 1,
            all = 2
        }

        public enum DeleteStrategy
        {
            bulk = 0,
            manual = 1,
        }

        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IConfigurationRoot Config;
        private IRepositoryWrapper RepositoryWrapper;

        public ModModule(IRepositoryWrapper repositoryWrapper, IConfigurationRoot config)
        {
            RepositoryWrapper = repositoryWrapper;
            Config = config;
        }

        [Command("announce")]
        [Alias("ann")]
        [Summary("Announces the specified message.")]
        [PermissionCheck]
        public async Task AnnounceAsync([Remainder] string announcement = null)
        {
            var message = Context.Message;
            var mod = message.Author as IGuildUser;
            var guild = Context.Guild;

            if (string.IsNullOrEmpty(announcement))
            {
                var embInvalid = new EmbedBuilder()
                {
                    Description = $"Announement message invalid.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embInvalid.Build()).ConfigureAwait(false);
            }
            else
            {
                var announcSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("announce");
                if (announcSpecialChannel.IsObjectNull() || announcSpecialChannel.IsEmpty())
                {
                    Log.Error($"Announcement channel has not been set.");

                    var embNoAnnouncement = new EmbedBuilder()
                    {
                        Description = $"Announement channel has not been set. Aborting.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNoAnnouncement.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embAnnouncement = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName("Announcement").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                        Description = announcement,
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    if (guild.GetChannel(announcSpecialChannel.Id) is ITextChannel guildAnnouncementChannel)
                        await guildAnnouncementChannel.SendMessageAsync("@everyone", false, embAnnouncement.Build()).ConfigureAwait(false);
                    else
                    {
                        var embGuildChannel = new EmbedBuilder()
                        {
                            Description = $"Announement channel with the id : **{announcSpecialChannel.Id}** could not be found.",
                            Color = Constants.FailureColor,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embGuildChannel.Build()).ConfigureAwait(false);
                        return;
                    }

                    var logSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
                    if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
                        Log.Error($"Log channel has not been set.");
                    else
                    {
                        var embNotify = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                            Color = Constants.InfoColor,
                            Description = $"Has used the **announce** command in **{message.Channel.Name}** channel.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                            await guildLogChannel.SendMessageAsync("", false, embNotify.Build()).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("mute")]
        [Alias("m")]
        [Summary("Mute the specified user for x amount of minutes.")]
        [PermissionCheck]
        public async Task MuteAsync(IGuildUser user = null, int minutes = 0, [Remainder] string reason = null)
        {
            var mod = Context.Message.Author;
            var guild = Context.Guild;
            var role = guild.Roles.Where(x => x.Name.ToLower().CompareTo("muted") == 0).FirstOrDefault();

            await Context.Message.DeleteAsync().ConfigureAwait(false);

            var logSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
            if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
            {
                Log.Error($"Log channel has not been set. Aborting mute.");
                return;
            }

            if (user == null || minutes == 0)
            {
                var embFailed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.FailureColor,
                    Description = $"User or amount of minutes for the mute not provided. Please try again.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync(mod.Mention, false, embFailed.Build()).ConfigureAwait(false);
            }
            else if (string.IsNullOrEmpty(reason))
            {
                var embFailed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.FailureColor,
                    Description = $"Missing reason for the mute. Please try again with proper reason.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync(mod.Mention, false, embFailed.Build()).ConfigureAwait(false);
            }
            else
            {
                var mute = await RepositoryWrapper.MuteRepository.GetActiveMuteByUserIdAsync(user.Id);
                if (mute.IsObjectNull() || mute.IsEmpty())
                {
                    var newMute = new Mute()
                    {
                        UserId = user.Id,
                        GuildId = Context.Guild.Id,
                        Active = true,
                        Expires = DateTime.UtcNow.AddMinutes(minutes),
                        Reason = reason,
                        MutedBy = mod.Id
                    };

                    await RepositoryWrapper.MuteRepository.AddMuteAsync(newMute);
                    RepositoryWrapper.MuteRepository.SaveChanges();
                }
                else
                {
                    mute.Expires.AddMinutes(minutes);

                    RepositoryWrapper.MuteRepository.UpdateMute(mute);
                    RepositoryWrapper.MuteRepository.SaveChanges();
                }

                var userInDb = await RepositoryWrapper.UserRepository.GetUserByIdAsync(user.Id);
                if (userInDb.IsObjectNull() || userInDb.IsEmpty())
                {
                    var newUser = new User()
                    {
                        Id = user.Id,
                        Guild = user.Guild.Id,
                        Discriminator = user.DiscriminatorValue,
                        Name = user.Username,
                        AvatarUrl = user.GetAvatarUrl(),
                        Joined = DateTime.UtcNow,
                        Left = new DateTime(),
                        MuteExpires = mute.Expires
                    };

                    await RepositoryWrapper.UserRepository.AddUserAsync(newUser);
                    RepositoryWrapper.UserRepository.SaveChanges();
                }
                else
                {
                    userInDb.MuteExpires = mute.Expires;

                    RepositoryWrapper.UserRepository.UpdateUser(userInDb);
                    RepositoryWrapper.UserRepository.SaveChanges();
                }

                await user.AddRoleAsync(role).ConfigureAwait(false);

                var embNotify = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.SuccessColor,
                    Description = $"Has muted **{user.Username}** for **{minutes}** minutes.\n\n **Reason**:\n{reason}.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync("", false, embNotify.Build()).ConfigureAwait(false);
            }
        }

        [Command("unmute")]
        [Alias("um")]
        [Summary("Unmute the specified user.")]
        [PermissionCheck]
        public async Task UnmuteAsync(IGuildUser user = null, [Remainder] string reason = null)
        {
            var mod = Context.Message.Author;
            var guild = Context.Guild;
            var role = guild.Roles.Where(x => x.Name.ToLower().CompareTo("muted") == 0).FirstOrDefault();

            await Context.Message.DeleteAsync().ConfigureAwait(false);

            var logSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
            if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
            {
                Log.Error($"Log channel has not been set. Aborting mute.");
                return;
            }

            if (user == null)
            {
                var embFailed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.FailureColor,
                    Description = $"User not provided. Please try again.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync(mod.Mention, false, embFailed.Build()).ConfigureAwait(false);
            }
            else if (string.IsNullOrEmpty(reason))
            {
                var embFailed = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.FailureColor,
                    Description = $"Missing reason for the unmute. Please try again with proper reason.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync(mod.Mention, false, embFailed.Build()).ConfigureAwait(false);
            }
            else
            {
                var mute = await RepositoryWrapper.MuteRepository.GetActiveMuteByUserIdAsync(user.Id);
                if (mute.IsObjectNull() || mute.IsEmpty())
                {
                    var embFailed = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                        Color = Constants.InfoColor,
                        Description = $"There is no active mute for user: **{user.Username}** | **{user.Id}**.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel2)
                        await guildLogChannel2.SendMessageAsync(mod.Mention, false, embFailed.Build()).ConfigureAwait(false);

                    return;
                }
                else
                {
                    mute.Active = false;

                    RepositoryWrapper.MuteRepository.UpdateMute(mute);
                    RepositoryWrapper.MuteRepository.SaveChanges();
                }

                var userInDb = await RepositoryWrapper.UserRepository.GetUserByIdAsync(user.Id);
                if (userInDb.IsObjectNull() || userInDb.IsEmpty())
                {
                    var newUser = new User()
                    {
                        Id = user.Id,
                        Guild = user.Guild.Id,
                        Discriminator = user.DiscriminatorValue,
                        Name = user.Username,
                        AvatarUrl = user.GetAvatarUrl(),
                        Joined = DateTime.UtcNow,
                        Left = new DateTime(),
                        MuteExpires = DateTime.UtcNow
                    };

                    await RepositoryWrapper.UserRepository.AddUserAsync(newUser);
                    RepositoryWrapper.UserRepository.SaveChanges();
                }
                else
                {
                    userInDb.MuteExpires = DateTime.UtcNow;

                    RepositoryWrapper.UserRepository.UpdateUser(userInDb);
                    RepositoryWrapper.UserRepository.SaveChanges();
                }

                await user.RemoveRoleAsync(role).ConfigureAwait(false);

                var embNotify = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.SuccessColor,
                    Description = $"Has unmuted **{user.Username}**.\n\n **Reason**:\n{reason}.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync("", false, embNotify.Build()).ConfigureAwait(false);
            }
        }

        [Command("mutes")]
        [Alias("ms")]
        [Summary("Shows a list of currently active mutes.")]
        [PermissionCheck]
        public async Task MuteListAsync()
        {
            var muteList = await RepositoryWrapper.MuteRepository.GetAllActiveMutesAsync();
            if (muteList.Count() == 0)
            {
                var embEmpty = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"There are currently no active mutes.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embEmpty.Build()).ConfigureAwait(false);
            }
            else
            {
                var muteListReply = new StringBuilder();
                var embs = new List<Embed>();
                var embList = new EmbedBuilder();
                var guild = Context.Guild;

                foreach (var mute in muteList)
                {
                    var guildUser = guild.GetUser(mute.UserId);
                    var mod = guild.GetUser(mute.MutedBy);
                    var newMutetLine = $"**{(guildUser != null ? guildUser.Username : "user not found")}** -- Muted by: {(mod != null ? mod.Username : "mod not found")} -- Reason: {mute.Reason} -- Expires on: {mute.Expires.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)}\n";

                    if (muteListReply.Length + newMutetLine.Length <= 2000)
                        muteListReply.Append(value: newMutetLine);
                    else
                    {
                        embList = new EmbedBuilder()
                        {
                            Description = muteListReply.ToString(),
                            Color = Constants.InfoColor,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embs.Add(embList.Build());
                        muteListReply.Clear();
                    }
                }

                embList = new EmbedBuilder()
                {
                    Description = muteListReply.ToString(),
                    Color = Constants.InfoColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embs.Add(embList.Build());

                foreach (var emb in embs)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
        }

        [Command("cmdcd")]
        [Alias("ccd")]
        [Summary("Sets a cd for the given command.")]
        [PermissionCheck]
        public async Task SetCmdCdAsync(string cmd = null, [Remainder] int seconds = -1)
        {
            if (string.IsNullOrEmpty(cmd))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Command is invalid.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else if (seconds < 0)
            {
                var configValue = Config.GetValue<int>($"cmdCds:{cmd.ToLower()}");

                var embNull = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"Command **{cmd.ToLower()}** has currently a cooldown of **{configValue}** seconds.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else
            {
                JObject configObj = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile)));
                JObject configCds = (JObject)configObj["cmdCds"];
                configCds[cmd.ToLower()] = seconds;
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile), configObj.ToString());

                if (seconds == 0)
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Command: **{cmd.ToLower()}** has now no cooldown.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Command: **{cmd.ToLower()}** has now a cooldown of **{seconds}** seconds.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("purge")]
        [Alias("clean")]
        [Summary("Cleans the bot's messages")]
        [PermissionCheck]
        public async Task Clean(
        [Summary("The optional number of messages to delete; defaults to 10")] int count = 10,
        [Summary("The type of messages to delete - Self, Bot, or All")] DeleteType deleteType = DeleteType.all,
        [Summary("The strategy to delete messages - BulkDelete or Manual")] DeleteStrategy deleteStrategy = DeleteStrategy.bulk)
        {
            int index = 0;
            var deleteMessages = new List<IMessage>(count);
            var userId = Context.Message.Author.Id;
            var messages = Context.Channel.GetMessagesAsync();

            var mod = Context.Message.Author as IGuildUser;
            var channel = Context.Channel;
            var guild = Context.Guild;
            var message = Context.Message;

            await messages.ForEachAsync(async m =>
            {
                IEnumerable<IMessage> delete = null;
                if (deleteType == DeleteType.self)
                    delete = m.Where(msg => msg.Author.Id == userId);
                else if (deleteType == DeleteType.bot)
                    delete = m.Where(msg => msg.Author.IsBot);
                else if (deleteType == DeleteType.all)
                    delete = m;

                foreach (var msg in delete.OrderByDescending(msg => msg.Timestamp))
                {
                    if (msg.Timestamp.DateTime < DateTime.Now.AddDays(-14)) continue;
                    if (index >= count) { await EndClean(deleteMessages, deleteStrategy); return; }
                    deleteMessages.Add(msg);
                    index++;
                }
            });

            var logSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("log");
            if (logSpecialChannel.IsObjectNull() || logSpecialChannel.IsEmpty())
                Log.Error($"Log channel has not been set.");
            else
            {
                var embNotify = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mod.Username).WithIconUrl(mod.GetAvatarUrl()).WithUrl(Constants.AllStarsUrl),
                    Color = Constants.InfoColor,
                    Description = $"Has purged the chat in **{channel.Name}** channel.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (guild.GetChannel(logSpecialChannel.Id) is ITextChannel guildLogChannel)
                    await guildLogChannel.SendMessageAsync("", false, embNotify.Build()).ConfigureAwait(false);
            }
        }

        internal async Task EndClean(IEnumerable<IMessage> messages, DeleteStrategy strategy)
        {
            if (strategy == DeleteStrategy.bulk)
                await (Context.Channel as ITextChannel).DeleteMessagesAsync(messages);
            else if (strategy == DeleteStrategy.manual)
            {
                foreach (var msg in messages.Cast<IUserMessage>())
                {
                    await msg.DeleteAsync();
                }
            }
        }
    }
}