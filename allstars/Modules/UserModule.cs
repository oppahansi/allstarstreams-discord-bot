using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using NLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Modules
{
    public class UserModule : ModuleBase<SocketCommandContext>
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly IConfigurationRoot Config;

        public UserModule(IConfigurationRoot config)
        {
            Config = config;
        }

        [Command("verify")]
        [Summary("Assigns a verified role to user for given code.")]
        public async Task MovieAsync([Remainder] string code = null)
        {
            var guild = Context.Client.GetGuild(ulong.Parse(Config[Constants.ConfigGuildId]));
            var author = Context.Message.Author;
            var guildUser = guild.GetUser(author.Id);

            if (!Context.IsPrivate)
                await Context.Message.DeleteAsync().ConfigureAwait(false);

            var dmChannel = await author.GetOrCreateDMChannelAsync();

            if (string.IsNullOrEmpty(code))
            {
                var embInvalid = new EmbedBuilder()
                {
                    Description = $"No verification code provided. Please try again with the correct verification code.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                if (dmChannel != null)
                {
                    try
                    {
                        await dmChannel.SendMessageAsync(author.Mention, false, embInvalid.Build()).ConfigureAwait(false);
                    }
                    catch
                    {
                        await ReplyAsync(author.Mention, false, embInvalid.Build()).ConfigureAwait(false);
                    }
                }
                else
                    await ReplyAsync(author.Mention, false, embInvalid.Build()).ConfigureAwait(false);
            }
            else
            {
                var codes = new List<string>() { Config[Constants.ConfigVaderStreams].ToLower(), Config[Constants.ConfigLightStreams].ToLower(), Config[Constants.ConfigHoloDisc].ToLower(), Config[Constants.ConfigDharma].ToLower(), Config[Constants.ConfigBeastTv].ToLower() };

                if (!codes.Contains(code.ToLower()))
                {
                    var embInvalid = new EmbedBuilder()
                    {
                        Description = $"Verification code is invalid. Please try again with the correct verification code.",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    if (dmChannel != null)
                    {
                        try
                        {
                            await dmChannel.SendMessageAsync(author.Mention, false, embInvalid.Build()).ConfigureAwait(false);
                        }
                        catch
                        {
                            await ReplyAsync(author.Mention, false, embInvalid.Build()).ConfigureAwait(false);
                        }
                    }
                    else
                        await ReplyAsync(author.Mention, false, embInvalid.Build()).ConfigureAwait(false);
                }
                else
                {
                    IRole guildRole = null;

                    if (codes[0].ToLower().CompareTo(code.ToLower()) == 0)
                        guildRole = guild.Roles.Where(x => x.Name.ToLower().Contains(Constants.VerifiedVaderStreamRole)).FirstOrDefault();
                    else if (codes[1].ToLower().CompareTo(code.ToLower()) == 0)
                        guildRole = guild.Roles.Where(x => x.Name.ToLower().Contains(Constants.VerifiedLightStreamRole)).FirstOrDefault();
                    else if (codes[2].ToLower().CompareTo(code.ToLower()) == 0)
                        guildRole = guild.Roles.Where(x => x.Name.ToLower().Contains(Constants.VerifiedHoloDiscRole)).FirstOrDefault();
                    else if (codes[3].ToLower().CompareTo(code.ToLower()) == 0)
                        guildRole = guild.Roles.Where(x => x.Name.ToLower().Contains(Constants.VerifiedDharmaRole)).FirstOrDefault();
                    else
                        guildRole = guild.Roles.Where(x => x.Name.ToLower().Contains(Constants.VerifiedBeastTvRole)).FirstOrDefault();

                    if (guildRole == null)
                    {
                        Log.Info("Verified role could not be found. Verification code was correct.");

                        var embRoleNull = new EmbedBuilder()
                        {
                            Description = $"Verification role could not be found. Please notify the discord server team.",
                            Color = Constants.FailureColor,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        if (dmChannel != null)
                        {
                            try
                            {
                                await dmChannel.SendMessageAsync(author.Mention, false, embRoleNull.Build()).ConfigureAwait(false);
                            }
                            catch
                            {
                                await ReplyAsync(author.Mention, false, embRoleNull.Build()).ConfigureAwait(false);
                            }
                        }
                        else
                            await ReplyAsync(author.Mention, false, embRoleNull.Build()).ConfigureAwait(false);
                        return;
                    }
                    else
                    {
                        await guildUser.AddRoleAsync(guildRole).ConfigureAwait(false);

                        Log.Error($"Verified role: {guildRole.Name} has been assigned to user: {guildUser.Username}#{guildUser.DiscriminatorValue}.");

                        var embRoleNull = new EmbedBuilder()
                        {
                            Description = $"Verified role: **{guildRole.Name}** assigned.",
                            Color = Constants.SuccessColor,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        if (dmChannel != null)
                        {
                            try
                            {
                                await dmChannel.SendMessageAsync(author.Mention, false, embRoleNull.Build()).ConfigureAwait(false);
                            }
                            catch
                            {
                                await ReplyAsync(author.Mention, false, embRoleNull.Build()).ConfigureAwait(false);
                            }
                        }
                        else
                            await ReplyAsync(author.Mention, false, embRoleNull.Build()).ConfigureAwait(false);
                    }
                }
            }
        }
    }
}