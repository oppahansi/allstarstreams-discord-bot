using allstars.Extensions;
using allstars.Models;
using allstars.Repositories;
using allstars.Services;
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
    public class AdminModule : ModuleBase<SocketCommandContext>
    {
        private IConfigurationRoot Config;
        private IRepositoryWrapper RepositoryWrapper;
        private AutoMessageService AutoMessageService;
        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        public AdminModule(IConfigurationRoot config, IRepositoryWrapper repositoryWrapper, AutoMessageService autoMessageService)
        {
            Config = config;
            RepositoryWrapper = repositoryWrapper;
            AutoMessageService= autoMessageService;
        }

        [Command("specialchan")]
        [Alias("sc")]
        [Summary("Flags a channel with a channel type.")]
        [PermissionCheck]
        public async Task SetSpecialChannelAsync(IGuildChannel channel = null, string channelType = null)
        {
            if (channel == null || string.IsNullOrEmpty(channelType))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Channel type or channel is null. Both required.\n\nExample: !specialchan #channel channelType",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else if (!Constants.ChannelTypes.Contains(channelType.ToLower()))
            {
                var embNotFound = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Channel type **{channelType.ToLower()}** does not exist.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
            }
            else
            {
                var specialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync(channelType.ToLower());
                if (!specialChannel.IsObjectNull() && !specialChannel.IsEmpty())
                {
                    var oldChannel = Context.Guild.GetChannel(specialChannel.Id);
                    var oldChannelName = oldChannel != null ? oldChannel.Name : "";

                    specialChannel.Id = channel.Id;
                    RepositoryWrapper.SpecialChannelRepository.UpdateSpecialChannel(specialChannel);
                    RepositoryWrapper.SpecialChannelRepository.SaveChanges();

                    var embSet = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Channel **{channel.Name}** has been flagged as **{channelType.ToLower()}** channel." + (!string.IsNullOrEmpty(oldChannelName) ? $"\nPrevious channel **{oldChannelName}** has been unflagged." : ""),
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embSet.Build()).ConfigureAwait(false);
                }
                else
                {
                    specialChannel = new SpecialChannel()
                    {
                        Type = channelType.ToLower(),
                        Guild = channel.GuildId,
                        Id = channel.Id
                    };

                    await RepositoryWrapper.SpecialChannelRepository.AddSpecialChannelAsync(specialChannel);
                    RepositoryWrapper.SpecialChannelRepository.SaveChanges();

                    var embSet = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Channel **{channel.Name}** has been flagged as **{channelType.ToLower()}** channel.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embSet.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("rspecialchan")]
        [Alias("rsc")]
        [Summary("Unflags a special channel.")]
        [PermissionCheck]
        public async Task RemoveSpecialChannelAsync(string channelType = null)
        {
            if (string.IsNullOrEmpty(channelType))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Channel type is null or empty. Both required.\n\nExample: !specialchan #channel channelType",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else if (!Constants.ChannelTypes.Contains(channelType.ToLower()))
            {
                var embNotFound = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Channel type **{channelType.ToLower()}** does not exist.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
            }
            else
            {
                var specialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync(channelType.ToLower());
                if (!specialChannel.IsObjectNull() && !specialChannel.IsEmpty())
                {
                    var oldChannel = Context.Guild.GetChannel(specialChannel.Id);
                    var oldChannelName = oldChannel != null ? oldChannel.Name : "";

                    RepositoryWrapper.SpecialChannelRepository.RemoveSpecialChannel(specialChannel);
                    RepositoryWrapper.SpecialChannelRepository.SaveChanges();

                    var embSet = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Channel **{oldChannelName}** has been unflagged as **{channelType.ToLower()}** channel.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embSet.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Channel type **{channelType}** could not be found in DB.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("specialchanlist")]
        [Alias("scl")]
        [Summary("Shows list of special channels.")]
        [PermissionCheck]
        public async Task SetSpecialChannelAsync()
        {
            var specialChannels = await RepositoryWrapper.SpecialChannelRepository.GetAllSpecialChannelsAsync();
            if (specialChannels.Count() == 0)
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"There are no special channels set.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else
            {
                var guild = Context.Guild;
                var stringBuilder = new StringBuilder();

                foreach (var specialChannel in specialChannels)
                {
                    var guildChannel = guild.GetChannel(specialChannel.Id);
                    stringBuilder.Append($"**{(guildChannel != null ? guildChannel.Name : "chnnel not found")}** is flagged as **{specialChannel.Type}**\n");
                }

                var embList = new EmbedBuilder()
                {
                    Color = Constants.SuccessColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embList.Build()).ConfigureAwait(false);
            }
        }

        [Command("minaccage")]
        [Alias("mac")]
        [Summary("Updates minimum account age threshold for new users.")]
        [PermissionCheck]
        public async Task SetMinAccAgeAsync([Remainder] int minAccAge = -1)
        {
            var configMinAccAge = Config.GetValue<int>(Constants.ConfigMinAccAge);

            if (minAccAge <= -1)
            {
                var embCurrent = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"Current minimum account age for new discord members is: **{configMinAccAge}** hours.\n\n**0** means disabled. Everything else above 0 means enabled.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embCurrent.Build()).ConfigureAwait(false);
                return;
            }

            if (minAccAge == 0 && configMinAccAge == 0)
            {
                var embAlreadyDisabled = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"Discord account age check is already disabled.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embAlreadyDisabled.Build()).ConfigureAwait(false);
            }
            else
            {
                JObject configObj = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile)));
                JObject configValues = (JObject)configObj["configValues"];
                configValues["minAccAge"] = minAccAge;
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile), configObj.ToString());

                if (minAccAge == 0)
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Discord account age check has been disabled.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Discord account age check has been enabled. Minimum discord account age required is now **{minAccAge}** hours.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("archive")]
        [Alias("arc")]
        [Summary("Enables / Disables archiving of deleted messages.")]
        [PermissionCheck]
        public async Task ArchiveAsync(int archivingValue = 0)
        {
            var configArchivingValue = Config.GetValue<int>(Constants.ConfigArchiving);
            if (archivingValue == 0 && configArchivingValue == 0)
            {
                var embAlreadyDisabled = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"Archiving is already disabled.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embAlreadyDisabled.Build()).ConfigureAwait(false);
            }
            else
            {
                JObject configObj = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile)));
                JObject configValues = (JObject)configObj["configValues"];
                configValues["messageArchive"] = archivingValue < 0 ? 0 : archivingValue;
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile), configObj.ToString());

                if (archivingValue <= 0)
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Archiving has been disabled.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Archiving has been enabled.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("cmdchan")]
        [Alias("cc")]
        [Summary("Binds a command to a channel")]
        [PermissionCheck]
        public async Task BindCmdToChannellAsync(string cmd = null, IGuildChannel channel = null)
        {
            var cmdMatch = Config[$"cmdDefaults:{cmd}"];

            if (string.IsNullOrEmpty(cmd) || channel == null || cmdMatch == null)
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provided command or channel is invalid.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else
            {
                var cmdChannel = await RepositoryWrapper.CmdChannelRepository.GetCmdChannelAsync(cmd.ToLower(), channel.Id);
                if (cmdChannel.IsObjectNull() || cmdChannel.IsEmpty())
                {
                    cmdChannel = new CmdChannel()
                    {
                        Command = cmd.ToLower(),
                        Guild = Context.Guild.Id,
                        ChannelId = channel.Id
                    };

                    await RepositoryWrapper.CmdChannelRepository.AddCmdChannelAsync(cmdChannel);
                    RepositoryWrapper.CmdChannelRepository.SaveChanges();

                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Command:  **{cmd.ToLower()}**  has been bound to channel:  **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                }
                else
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Command:  **{cmd.ToLower()}**  is already bound to channel:  **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("rcmdchan")]
        [Alias("rcc")]
        [Summary("Unbinds a command from a channel")]
        [PermissionCheck]
        public async Task UnbindChannelFromCmdAsync(string cmd = null, IGuildChannel channel = null)
        {
            var cmdMatch = Config[$"cmdDefaults:{cmd}"];

            if (string.IsNullOrEmpty(cmd) || cmdMatch == null)
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provided command is invalid.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else
            {
                if (channel == null)
                {
                    var cmdChannels = await RepositoryWrapper.CmdChannelRepository.GetCmdChannelsAsync(cmd.ToLower());
                    if (cmdChannels.Count() > 0)
                    {
                        RepositoryWrapper.CmdChannelRepository.DeleteManyCmdChannels(cmdChannels);
                        RepositoryWrapper.TicketRepository.SaveChanges();

                        var embAll = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = $"Command:  **{cmd.ToLower()}**  has been unbound from **ALL** channels.",
                            Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embAll.Build()).ConfigureAwait(false);
                    }
                }
                else
                {
                    var cmdChannel = await RepositoryWrapper.CmdChannelRepository.GetCmdChannelAsync(cmd.ToLower(), channel.Id);
                    if (cmdChannel.IsObjectNull() || cmdChannel.IsEmpty())
                    {
                        var embNotFound = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = $"Command:  **{cmd.ToLower()}**  is not bound to to channel: **{channel.Name}**.",
                            Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        RepositoryWrapper.CmdChannelRepository.DeleteCmdChannel(cmdChannel);
                        RepositoryWrapper.CmdChannelRepository.SaveChanges();

                        var embUnbound = new EmbedBuilder()
                        {
                            Color = Constants.SuccessColor,
                            Description = $"Command:  **{cmd.ToLower()}**  has been unbound from channel: **{channel.Name}**.",
                            Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embUnbound.Build()).ConfigureAwait(false);
                    }
                }
            }
        }

        [Command("cmdrole")]
        [Alias("cr")]
        [Summary("Binds a command to a role.")]
        [PermissionCheck]
        public async Task BindCmdToRolelAsync(string cmd = null, string role = null)
        {
            var cmdMatch = Config[$"cmdDefaults:{cmd}"];
            if (string.IsNullOrEmpty(cmd) || role == null || cmdMatch == null)
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provided command or role is invalid.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else
            {
                var guildRole = Context.Guild.Roles.ToList().Find(x => x.Name.ToLower().CompareTo(role.ToLower()) == 0);
                if (guildRole == null)
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"Provided role:  **{role}**  could not be found.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                    return;
                }

                var cmdRole = await RepositoryWrapper.CmdRoleRepository.GetCmdRoleAsync(cmd.ToLower());
                if (cmdRole.IsObjectNull() || cmdRole.IsEmpty())
                {
                    cmdRole = new CmdRole()
                    {
                        Command = cmd.ToLower(),
                        Guild = Context.Guild.Id,
                        MinRoleId = guildRole.Id
                    };

                    await RepositoryWrapper.CmdRoleRepository.AddCmdRoleAsync(cmdRole);
                    RepositoryWrapper.CmdRoleRepository.SaveChanges();

                    var embBound = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Command:  **{cmd.ToLower()}**  has been bound to role:  **{guildRole.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embBound.Build()).ConfigureAwait(false);
                }
                else
                {
                    var oldRole = Context.Guild.GetRole(cmdRole.MinRoleId);

                    cmdRole.MinRoleId = guildRole.Id;

                    RepositoryWrapper.CmdRoleRepository.UpdateCmdRole(cmdRole);
                    RepositoryWrapper.CmdRoleRepository.SaveChanges();

                    var embDone = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Command:  **{cmd.ToLower()}**  was already bound to role:  **{oldRole.Name}**.\nBut is now bound to role: **{guildRole.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDone.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("rcmdrole")]
        [Alias("rcr")]
        [Summary("Unbinds a command from a role")]
        [PermissionCheck]
        public async Task UnbindRoleFromCmdAsync(string cmd = null)
        {
            var cmdMatch = Config[$"cmdDefaults:{cmd}"];

            if (string.IsNullOrEmpty(cmd) || cmdMatch == null)
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provided command is invalid.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
            }
            else
            {
                var cmdRole = await RepositoryWrapper.CmdRoleRepository.GetCmdRoleAsync(cmd.ToLower());
                if (cmdRole.IsObjectNull() || cmdRole.IsEmpty())
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Command:  **{cmd.ToLower()}**  is not bound to any role.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                }
                else
                {
                    var oldRole = Context.Guild.GetRole(cmdRole.MinRoleId);

                    RepositoryWrapper.CmdRoleRepository.DeleteCmdRole(cmdRole);
                    RepositoryWrapper.CmdRoleRepository.SaveChanges();

                    var embUnbound = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Command:  **{cmd.ToLower()}**  has been unbound from role: **{oldRole.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUnbound.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("cmdpermissions")]
        [Alias("cp")]
        [Summary("Lists currently sest command permissions.")]
        [PermissionCheck]
        public async Task ListCCmdPermissionsAsync()
        {
            var cmdChannels = await RepositoryWrapper.CmdChannelRepository.GetAllCmdChannelsAsync();
            var cmdRoles = await RepositoryWrapper.CmdRoleRepository.GetAllCmdRolesAsync();

            if (cmdChannels.Count() == 0 && cmdRoles.Count() == 0)
            {
                var embUnbound = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"There are no commands bound to a channel nor a role.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embUnbound.Build()).ConfigureAwait(false);
            }
            else
            {
                var stringBuilder = new StringBuilder();
                var embs = new List<Embed>();
                var embList = new EmbedBuilder();
                var guild = Context.Guild;

                stringBuilder.Append("**Channels:**\n");

                foreach (var cmdChannel in cmdChannels)
                {
                    var guildChannel = guild.GetChannel(cmdChannel.ChannelId);

                    var newCmdChannelLine = $"Command: **{cmdChannel.Command}** is bound to channel: **{guildChannel.Name}**\n";

                    if (stringBuilder.Length + newCmdChannelLine.Length <= 2000)
                        stringBuilder.Append(value: newCmdChannelLine);
                    else
                    {
                        embList = new EmbedBuilder()
                        {
                            Description = stringBuilder.ToString(),
                            Color = Constants.InfoColor,
                            Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                        };

                        embs.Add(embList.Build());
                        stringBuilder.Clear();
                    }
                }

                embList = new EmbedBuilder()
                {
                    Description = stringBuilder.ToString(),
                    Color = Constants.InfoColor,
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                embs.Add(embList.Build());

                stringBuilder.Clear();
                stringBuilder.Append("**Roles:**\n");

                foreach (var cmdRole in cmdRoles)
                {
                    var guildRole = guild.GetRole(cmdRole.MinRoleId);

                    var newCmdRolelLine = $"Command: **{cmdRole.Command}** requires minimun role: **{guildRole.Name}**\n";

                    if (stringBuilder.Length + newCmdRolelLine.Length <= 2000)
                        stringBuilder.Append(value: newCmdRolelLine);
                    else
                    {
                        embList = new EmbedBuilder()
                        {
                            Description = stringBuilder.ToString(),
                            Color = Constants.InfoColor,
                            Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                        };

                        embs.Add(embList.Build());
                        stringBuilder.Clear();
                    }
                }

                embList = new EmbedBuilder()
                {
                    Description = stringBuilder.ToString(),
                    Color = Constants.InfoColor,
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                embs.Add(embList.Build());

                foreach (var emb in embs)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
        }

        [Command("verificationcode")]
        [Alias("vc")]
        [Summary("Updates a verification code for given type.")]
        [PermissionCheck]
        public async Task SetVeryficationCodeAsync(string type = null, [Remainder] string code = null)
        {
            if (string.IsNullOrEmpty(type) || string.IsNullOrEmpty(code) || !Constants.VerificationTypes.Contains(type.ToLower()))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Verification code type or code is invalid.\n\n**Verification code types are:**\nvaderstreams, lightstreams, holodisc, beasttv",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                return;
            }

            try
            {
                var oldCode = Config[$"verifiedRoleCodes:{type}"];

                JObject configObj = JObject.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile)));
                JObject configValues = (JObject)configObj["verifiedRoleCodes"];
                configValues[type.ToLower()] = code;
                File.WriteAllText(Path.Combine(AppContext.BaseDirectory, Constants.ConfigFile), configObj.ToString());

                var guildRole = Context.Guild.Roles.Where(x => x.Name.ToLower().Contains(type.ToLower())).FirstOrDefault();

                var embUpdated = new EmbedBuilder()
                {
                    Color = Constants.SuccessColor,
                    Description = $"{guildRole.Name} verification code has been updated.\n\nOld code was: {oldCode}\nNew code is: {code.ToLower()}",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Setting new verification code failed.\n\n{ex.Message}",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
            }
        }

        [Command("automessage")]
        [Alias("am")]
        [Summary("Sets an automessage to be posten in the given channel.")]
        [PermissionCheck]
        public async Task AutoMessageAsync(IGuildChannel channel = null, int timeInterval = 0, [Remainder] string message = null)
        {
            if (channel == null || timeInterval <= 0 || string.IsNullOrEmpty(message))
            {
                var embFailed = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Failed to set auto message.\nPossible reasons: Channel incorrect, time interval is 0 or negative, message is empty.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embFailed.Build()).ConfigureAwait(false);
            }
            else
            {
                var autoMessage = await RepositoryWrapper.AutoMessageRepository.GetAutoMessageAsync(channel.Id);

                if (autoMessage == null || autoMessage.Channel == 0)
                {
                    autoMessage = new AutoMessage()
                    {
                        Channel = channel.Id,
                        Message = message,
                        TimeInterval = timeInterval,
                        MessagesInterval = 0,
                        Expiration = new DateTime()
                    };

                    await AutoMessageService.AddAutoMessageAsync(autoMessage).ConfigureAwait(false);

                    var embSuccess = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Auto message has been added to channel **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embSuccess.Build()).ConfigureAwait(false);
                }
                else
                {
                    autoMessage.TimeInterval = timeInterval;

                    await AutoMessageService.UpdateAutoMessageAsync(autoMessage);

                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Auto message has been updated for channel **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("messageinterval")]
        [Alias("mi")]
        [Summary("Sets an autom essage message interval in the given channel.")]
        [PermissionCheck]
        public async Task MessageIntervalAsync(IGuildChannel channel = null, int messageInterval = 0)
        {
            if (channel == null ||messageInterval <= 0)
            {
                var embFailed = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Failed to set message interval.\nPossible reasons: Channel incorrect,message interval is 0 or negative.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embFailed.Build()).ConfigureAwait(false);
            }
            else
            {
                var autoMessage = await RepositoryWrapper.AutoMessageRepository.GetAutoMessageAsync(channel.Id);

                if (autoMessage == null || autoMessage.Channel == 0)
                {
                    var embFailed = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"Auto message could not be found for channel **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embFailed.Build()).ConfigureAwait(false);
                }
                else
                {
                    autoMessage.MessagesInterval = messageInterval;

                    await AutoMessageService.UpdateAutoMessageAsync(autoMessage).ConfigureAwait(false);

                    var embUpdated = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Auto message has been updated for channel **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embUpdated.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("rautomessage")]
        [Alias("ram")]
        [Summary("Removes an automessage from the given channel.")]
        [PermissionCheck]
        public async Task AutoMessageAsync(IGuildChannel channel = null)
        {
            if (channel == null)
            {
                var embFailed = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Failed to remove auto message.\nPossible reasons: Channel incorrect",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embFailed.Build()).ConfigureAwait(false);
            }
            else
            {
                var autoMessage = await RepositoryWrapper.AutoMessageRepository.GetAutoMessageAsync(channel.Id);

                if (autoMessage == null || autoMessage.Channel == 0)
                {
                    var embFailed = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"Auto message could not be found for channel **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embFailed.Build()).ConfigureAwait(false);
                }
                else
                {
                    AutoMessageService.RemoveAutoMessage(autoMessage);

                    var embRemoved = new EmbedBuilder()
                    {
                        Color = Constants.SuccessColor,
                        Description = $"Auto message has been removed from channel **{channel.Name}**.",
                        Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embRemoved.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("lautomessage")]
        [Alias("lam")]
        [Summary("Lists current automessages.")]
        [PermissionCheck]
        public async Task ListAutoMessageAsync()
        {
            var autoMessages = await RepositoryWrapper.AutoMessageRepository.GetAllAutoMessagesAsync();

            if (autoMessages.Count() == 0)
            {
                var embEmpty = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"There are currently no auto messages set.",
                    Footer = new EmbedFooterBuilder().WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embEmpty.Build()).ConfigureAwait(false);
            }
            else
            {
                var embs = new List<Embed>();
                var emb = new EmbedBuilder();
                var guild = Context.Guild;
                var stringBuilder = new StringBuilder();

                foreach (var autoMessage in autoMessages)
                {
                    var channel = guild.GetChannel(autoMessage.Channel);
                    var newAutoMessage = $"**{channel.Name} in { Math.Truncate(AutoMessageService.GetRemainingTime(autoMessage.Channel, autoMessage.TimeInterval))} mins :**\n{autoMessage.Message}";

                    if (stringBuilder.Length + newAutoMessage.Length <= 2000)
                        stringBuilder.Append($"{newAutoMessage}\n\n");
                    else
                    {
                        emb = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName("Currently set auto messages:").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                            Description = stringBuilder.ToString(),
                            Color = Constants.InfoColor,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embs.Add(emb.Build());
                        stringBuilder.Clear();
                    }
                }

                emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("Currently set auto messages:").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                    Description = stringBuilder.ToString(),
                    Color = Constants.InfoColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embs.Add(emb.Build());

                foreach (var embObject in embs)
                    await ReplyAsync("", false, embObject).ConfigureAwait(false);
            }
        }

        [Command("mail")]
        [Alias("m")]
        [Summary("Sends mails.")]
        [PermissionCheck]
        public async Task MailAsync(string provider, string when)
        {
            if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(when) || (provider.ToLower().CompareTo("v") != 0 && provider.ToLower().CompareTo("vader") != 0 && provider.ToLower().CompareTo("l") != 0 && provider.ToLower().CompareTo("lightstreams") != 0 && provider.ToLower().CompareTo("b") != 0 && provider.ToLower().CompareTo("beasttv") != 0))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provider is invalid.\n\n**Possible providers:**\nvader | v, lightstreams | l, b | beasttv",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                return;
            }

            var mailingList = new List<EmailInfo>();
            var countNoEmails = 0;

            var mailSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("mail");
            if (mailSpecialChannel.IsObjectNull() || mailSpecialChannel.IsEmpty())
            {
                Log.Error($"Mail channel has not been set.");

                var embNoAnnouncement = new EmbedBuilder()
                {
                    Description = $"Mail channel has not been set. Aborting.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNoAnnouncement.Build()).ConfigureAwait(false);
                return;
            }

            if (provider.ToLower().CompareTo("v") == 0 || provider.ToLower().CompareTo("vader") == 0)
            {
                var emailTemplate = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Config[Constants.ConfigVaderEmailTemplate]));
                if (string.IsNullOrEmpty(emailTemplate))
                {
                    var embTemplateEmpty = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"Email Template is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embTemplateEmpty.Build()).ConfigureAwait(false);
                    return;
                }

                var vaderAccInfoList = this.MapVaders(Config);
                var soonExpiredList = new List<VadertvAccInfo>();

                if (DateTime.TryParseExact(when, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime whenDateParsed))
                {
                    soonExpiredList = vaderAccInfoList.Where(x =>
                    whenDateParsed.Day == x.Expiration.Day &&
                    whenDateParsed.Month == x.Expiration.Month &&
                    whenDateParsed.Year == x.Expiration.Year &&
                    x.Email != "none" &&
                    !x.UserName.StartsWith("nxlvl") &&
                    !x.UserName.StartsWith("firm") &&
                    !x.UserName.StartsWith("em_")).Distinct().ToList();

                    countNoEmails = vaderAccInfoList.Where(x =>
                    whenDateParsed.Day == x.Expiration.Day &&
                    whenDateParsed.Month == x.Expiration.Month &&
                    whenDateParsed.Year == x.Expiration.Year &&
                    x.Email == "none" &&
                    !x.UserName.StartsWith("nxlvl") &&
                    !x.UserName.StartsWith("firm") &&
                    !x.UserName.StartsWith("em_")).Distinct().Count();

                    foreach (var accInfo in soonExpiredList)
                    {
                        mailingList.Add(new EmailInfo()
                        {
                            Username = accInfo.UserName,
                            From = Config[Constants.ConfigBillingEmailAddress],
                            To = accInfo.Email,
                            Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", when),
                            Subject = "Vaderstreams expiration reminder",
                        });
                    }

                    if (mailingList.Count == 0)
                    {
                        var embEmptyMailinglist = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = $"Mailinglist is empty.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embEmptyMailinglist.Build()).ConfigureAwait(false);
                        return;
                    }
                }
                else
                {
                    var embDateFailed = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Date could not be parsed. Expected format: MM-dd-yyyy.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDateFailed.Build()).ConfigureAwait(false);
                    return;
                }
            }
            else if (provider.ToLower().CompareTo("l") == 0 || provider.ToLower().CompareTo("lightstreams") == 0)
            {
                var emailTemplate = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Config[Constants.ConfigLightEmailTemplate]));
                if (string.IsNullOrEmpty(emailTemplate))
                {
                    var embTemplateEmpty = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"Email Template is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embTemplateEmpty.Build()).ConfigureAwait(false);
                    return;
                }

                var lightAccInfoList = this.MapLightStreams(Config);
                var soonExpiredList = new List<LightStreamAccInfo>();
                
                if (DateTime.TryParseExact(when, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime whenDateParsed))
                {
                    soonExpiredList = lightAccInfoList.Where(x => 
                    whenDateParsed.Day == x.Expiration.Day &&
                    whenDateParsed.Month == x.Expiration.Month && 
                    whenDateParsed.Year == x.Expiration.Year &&
                    x.Owner.ToLower().CompareTo("vaderstreamnet") == 0 &&
                    x.Notes != "-").Distinct().ToList();

                    countNoEmails = lightAccInfoList.Where(x =>
                    whenDateParsed.Day == x.Expiration.Day &&
                    whenDateParsed.Month == x.Expiration.Month &&
                    whenDateParsed.Year == x.Expiration.Year &&
                    x.Owner.ToLower().CompareTo("vaderstreamnet") == 0 &&
                    x.Notes == "-").Distinct().Count();

                    foreach (var accInfo in soonExpiredList)
                    {
                        var emailAddresses = accInfo.Notes.Replace(" ", "").Split(',');
                        if (emailAddresses.Length > 0)
                        {
                            foreach (var mail in emailAddresses)
                            {
                                mailingList.Add(new EmailInfo()
                                {
                                    Username = accInfo.UserName,
                                    From = Config[Constants.ConfigBillingEmailAddress],
                                    To = mail,
                                    Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", when),
                                    Subject = "Lightstreams expiration reminder",
                                });
                            }
                        }
                        else
                        {
                            mailingList.Add(new EmailInfo()
                            {
                                Username = accInfo.UserName,
                                From = Config[Constants.ConfigBillingEmailAddress],
                                To = accInfo.Notes.Replace(" ", ""),
                                Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", when),
                                Subject = "Lightstreams expiration reminder",
                            });
                        }
                    }

                    if (mailingList.Count == 0)
                    {
                        var embEmptyMailinglist = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = $"Mailinglist is empty.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embEmptyMailinglist.Build()).ConfigureAwait(false);
                        return;
                    }
                }
                else
                {
                    var embDateFailed = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Date could not be parsed. Expected format: MM-dd-yyyy.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDateFailed.Build()).ConfigureAwait(false);
                    return;
                }
            }
            else
            {
                var emailTemplate = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Config[Constants.ConfigBeastEmailTemplate]));
                if (string.IsNullOrEmpty(emailTemplate))
                {
                    var embTemplateEmpty = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"Email Template is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embTemplateEmpty.Build()).ConfigureAwait(false);
                    return;
                }

                var beastAccInfoList = this.MapBeastTv(Config);
                var soonExpiredList = new List<LightStreamAccInfo>();

                if (DateTime.TryParseExact(when, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime whenDateParsed))
                {
                    soonExpiredList = beastAccInfoList.Where(x =>
                    whenDateParsed.Day == x.Expiration.Day &&
                    whenDateParsed.Month == x.Expiration.Month &&
                    whenDateParsed.Year == x.Expiration.Year &&
                    x.Owner.ToLower().CompareTo("doctorbeefy@gmail.com") == 0 &&
                    x.Notes != "-").Distinct().ToList();

                    countNoEmails = beastAccInfoList.Where(x =>
                    whenDateParsed.Day == x.Expiration.Day &&
                    whenDateParsed.Month == x.Expiration.Month &&
                    whenDateParsed.Year == x.Expiration.Year &&
                    x.Owner.ToLower().CompareTo("doctorbeefy@gmail.com") == 0 &&
                    x.Notes == "-").Distinct().Count();

                    foreach (var accInfo in soonExpiredList)
                    {
                        var emailAddresses = accInfo.Notes.Replace(" ", "").Split(',');
                        if (emailAddresses.Length > 0)
                        {
                            foreach (var mail in emailAddresses)
                            {
                                mailingList.Add(new EmailInfo()
                                {
                                    Username = accInfo.UserName,
                                    From = Config[Constants.ConfigBillingEmailAddress],
                                    To = mail,
                                    Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", when),
                                    Subject = "BeastTV expiration reminder",
                                });
                            }
                        }
                        else
                        {
                            mailingList.Add(new EmailInfo()
                            {
                                Username = accInfo.UserName,
                                From = Config[Constants.ConfigBillingEmailAddress],
                                To = accInfo.Notes.Replace(" ", ""),
                                Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", when),
                                Subject = "BeastTV expiration reminder",
                            });
                        }
                    }

                    if (mailingList.Count == 0)
                    {
                        var embEmptyMailinglist = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = $"Mailinglist is empty.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        await ReplyAsync("", false, embEmptyMailinglist.Build()).ConfigureAwait(false);
                        return;
                    }
                }
                else
                {
                    var embDateFailed = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Date could not be parsed. Expected format: MM-dd-yyyy.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDateFailed.Build()).ConfigureAwait(false);
                    return;
                }
            }

            if (Context.Guild.GetChannel(mailSpecialChannel.Id) is ITextChannel guildMailChannel)
                await Task.Factory.StartNew(async () => { await this.SendMail(Config, guildMailChannel, mailingList, countNoEmails).ConfigureAwait(false); }).ConfigureAwait(false);
            else
            {
                var embDateFailed = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"GUild mail channel could not be found. Nothing will be sent.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embDateFailed.Build()).ConfigureAwait(false);
            }
        }

        [Command("maildates")]
        [Alias("md")]
        [Summary("Sends mails.")]
        [PermissionCheck]
        public async Task MailDateRangeAsync(string provider, string startingDate, string endingDate)
        {
            if (string.IsNullOrEmpty(provider) || (provider.ToLower().CompareTo("v") != 0 && provider.ToLower().CompareTo("vader") != 0 && provider.ToLower().CompareTo("l") != 0 && provider.ToLower().CompareTo("lightstreams") != 0 && provider.ToLower().CompareTo("b") != 0 && provider.ToLower().CompareTo("beasttv") != 0))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provider is invalid.\n\n**Possible providers:**\nvader | v, lightstreams | l, b | beasttv",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                return;
            }

            var mailSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("mail");
            if (mailSpecialChannel.IsObjectNull() || mailSpecialChannel.IsEmpty())
            {
                Log.Error($"Mail channel has not been set.");

                var embNoAnnouncement = new EmbedBuilder()
                {
                    Description = $"Mail channel has not been set. Aborting.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNoAnnouncement.Build()).ConfigureAwait(false);
                return;
            }

            var startingDateTime = new DateTime();
            var endingDateTime = new DateTime();
            var mailingList = new List<EmailInfo>();
            var countNoEmails = 0;

            if (DateTime.TryParseExact(startingDate, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out startingDateTime) && 
                DateTime.TryParseExact(endingDate, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out endingDateTime))
            {

                if (provider.ToLower().CompareTo("v") == 0 || provider.ToLower().CompareTo("vader") == 0)
                {
                    var emailTemplate = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Config[Constants.ConfigVaderEmailTemplate]));
                    var vaderAccs = this.MapVaders(Config);

                    var filteredAccs = vaderAccs.Where(x =>
                    (DateTime.Compare(x.Expiration, startingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, startingDateTime) > 0) 
                    &&
                    (DateTime.Compare(x.Expiration, endingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, endingDateTime) < 0)
                    &&
                    x.Email != "none" 
                    &&
                    !x.UserName.StartsWith("nxlvl") &&
                    !x.UserName.StartsWith("firm") &&
                    !x.UserName.StartsWith("em_")).Distinct().ToList();

                    countNoEmails = vaderAccs.Where(x =>
                    (DateTime.Compare(x.Expiration, startingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, startingDateTime) > 0)
                    &&
                    (DateTime.Compare(x.Expiration, endingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, endingDateTime) < 0)
                    &&
                    x.Email == "none"
                    &&
                    !x.UserName.StartsWith("nxlvl") &&
                    !x.UserName.StartsWith("firm") &&
                    !x.UserName.StartsWith("em_")).Distinct().Count();

                    foreach (var accInfo in filteredAccs)
                    {
                        mailingList.Add(new EmailInfo()
                        {
                            Username = accInfo.UserName,
                            From = Config[Constants.ConfigBillingEmailAddress],
                            To = accInfo.Email,
                            Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", accInfo.Expiration.ToString("MM-dd-yyyy")),
                            Subject = "Vaderstreams expiration reminder",
                        });
                    }
                }
                else if (provider.ToLower().CompareTo("l") == 0 || provider.ToLower().CompareTo("lightstreams") == 0)
                {
                    var emailTemplate = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Config[Constants.ConfigLightEmailTemplate]));
                    var lightsAccs = this.MapLightStreams(Config);

                    var filteredAccs = lightsAccs.Where(x =>
                     (DateTime.Compare(x.Expiration, startingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, startingDateTime) > 0)
                    &&
                    (DateTime.Compare(x.Expiration, endingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, endingDateTime) < 0)
                    &&
                    x.Owner.ToLower().CompareTo("vaderstreamnet") == 0 &&
                    x.Notes != "-").Distinct().ToList();

                    countNoEmails = lightsAccs.Where(x =>
                    (DateTime.Compare(x.Expiration, startingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, startingDateTime) > 0)
                    &&
                    (DateTime.Compare(x.Expiration, endingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, endingDateTime) < 0)
                    &&
                    x.Owner.ToLower().CompareTo("vaderstreamnet") == 0 &&
                    x.Notes == "-").Distinct().Count();

                    foreach (var accInfo in filteredAccs)
                    {
                        var emailAddresses = accInfo.Notes.Replace(" ", "").Split(',');
                        if (emailAddresses.Length > 0)
                        {
                            foreach (var mail in emailAddresses)
                            {
                                mailingList.Add(new EmailInfo()
                                {
                                    Username = accInfo.UserName,
                                    From = Config[Constants.ConfigBillingEmailAddress],
                                    To = mail,
                                    Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", accInfo.Expiration.ToString("MM-dd-yyyy")),
                                    Subject = "LightStreams expiration reminder",
                                });
                            }
                        }
                        else
                        {
                            mailingList.Add(new EmailInfo()
                            {
                                Username = accInfo.UserName,
                                From = Config[Constants.ConfigBillingEmailAddress],
                                To = accInfo.Notes.Replace(" ", ""),
                                Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", accInfo.Expiration.ToString("MM-dd-yyyy")),
                                Subject = "LightStreams expiration reminder",
                            });
                        }
                    }
                }
                else
                {
                    var emailTemplate = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, Config[Constants.ConfigBeastEmailTemplate]));
                    var beastTvAccs = this.MapBeastTv(Config);

                    var filteredAccs = beastTvAccs.Where(x =>
                     (DateTime.Compare(x.Expiration, startingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, startingDateTime) > 0)
                    &&
                    (DateTime.Compare(x.Expiration, endingDateTime) == 0 ||
                    DateTime.Compare(x.Expiration, endingDateTime) < 0)
                    &&
                    x.Owner.ToLower().CompareTo("doctorbeefy@gmail.com") == 0 &&
                    x.Notes != "-").Distinct().ToList();

                    foreach (var accInfo in filteredAccs)
                    {
                        var emailAddresses = accInfo.Notes.Replace(" ", "").Split(',');
                        if (emailAddresses.Length > 0)
                        {
                            foreach (var mail in emailAddresses)
                            {
                                mailingList.Add(new EmailInfo()
                                {
                                    Username = accInfo.UserName,
                                    From = Config[Constants.ConfigBillingEmailAddress],
                                    To = mail,
                                    Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", accInfo.Expiration.ToString("MM-dd-yyyy")),
                                    Subject = "BeastTV expiration reminder",
                                });
                            }
                        }
                        else
                        {
                            mailingList.Add(new EmailInfo()
                            {
                                Username = accInfo.UserName,
                                From = Config[Constants.ConfigBillingEmailAddress],
                                To = accInfo.Notes.Replace(" ", ""),
                                Body = emailTemplate.Replace("USERNAME", accInfo.UserName).Replace("DATE", accInfo.Expiration.ToString("MM-dd-yyyy")),
                                Subject = "BeastTV expiration reminder",
                            });
                        }
                    }
                }

                if (mailingList.Count == 0)
                {
                    var embEmptyMailinglist = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Mailinglist is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embEmptyMailinglist.Build()).ConfigureAwait(false);
                    return;
                }

                if (Context.Guild.GetChannel(mailSpecialChannel.Id) is ITextChannel guildMailChannel)
                    await Task.Factory.StartNew(async () => { await this.SendMail(Config, guildMailChannel, mailingList, countNoEmails).ConfigureAwait(false); }).ConfigureAwait(false);
                else
                {
                    var embDateFailed = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = $"GUild mail channel could not be found. Nothing will be sent.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDateFailed.Build()).ConfigureAwait(false);
                }
            }
            else
            {
                var embDateFailed = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = $"Date could not be parsed. Expected format: MM-dd-yyyy.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embDateFailed.Build()).ConfigureAwait(false);
                return;
            }
        }

    }
}