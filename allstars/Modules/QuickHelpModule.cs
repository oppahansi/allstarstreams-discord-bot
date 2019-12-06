using allstars.Extensions;
using allstars.Models;
using allstars.Repositories;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Text;
using System.Threading.Tasks;

namespace allstars.Modules
{
    [RequireContext(ContextType.Guild)]
    public class QuickHelpModule : ModuleBase<SocketCommandContext>
    {
        private IRepositoryWrapper RepositoryWrapper;
        private readonly IConfigurationRoot Config;

        public QuickHelpModule(IRepositoryWrapper repositoryWrapper, IConfigurationRoot config)
        {
            RepositoryWrapper = repositoryWrapper;
            Config = config;
        }

        [Command("helpadd")]
        [Alias("ha")]
        [Summary("Adds a new quick help tag with given help description.")]
        [PermissionCheck]
        public async Task HelpAddAsync(string tag, [Remainder] string help = null)
        {
            if (string.IsNullOrEmpty(tag) || string.IsNullOrEmpty(help))
            {
                var emb = new EmbedBuilder()
                {
                    Description = $"Quick help tag or help is null or empty. Both are required.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
            }
            else
            {
                var newQuickHelp = await RepositoryWrapper.QuickHelpRepository.GetQuickHelpByTagAsync(tag.ToLower());
                if (!newQuickHelp.IsObjectNull() && !newQuickHelp.IsEmpty())
                {
                    newQuickHelp.Help = help;
                    RepositoryWrapper.QuickHelpRepository.UpdateQuickHelp(newQuickHelp);
                    RepositoryWrapper.QuickHelpRepository.SaveChanges();

                    var emb = new EmbedBuilder()
                    {
                        Description = $"Quick help tag **{tag.ToLower()}** has been updated and contains now:\n\n{help}",
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
                }
                else
                {
                    newQuickHelp = new QuickHelp()
                    {
                        Tag = tag.ToLower(),
                        Help = help
                    };

                    await RepositoryWrapper.QuickHelpRepository.AddQuickHelpAsync(newQuickHelp).ConfigureAwait(false);
                    RepositoryWrapper.QuickHelpRepository.SaveChanges();

                    var emb = new EmbedBuilder()
                    {
                        Description = $"Quick help tag **{tag.ToLower()}** has been added with following content:\n\n{help}",
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("helpdel")]
        [Alias("hd")]
        [Summary("Deletes quick help for the given tag.")]
        [PermissionCheck]
        public async Task HelpDeleteAsync(string tag)
        {
            if (string.IsNullOrEmpty(tag))
            {
                var emb = new EmbedBuilder()
                {
                    Description = $"Quick help tag is null or empty but is required.",
                    Color = Constants.FailureColor,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
            }
            else
            {
                var quickHelp = await RepositoryWrapper.QuickHelpRepository.GetQuickHelpByTagAsync(tag.ToLower());
                if (quickHelp.IsObjectNull() || quickHelp.IsEmpty())
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"Quick help tag **{tag.ToLower()}** could not be found. Typo?\nPlease check following quick help tags:",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                    await SendQuickHelpList(false).ConfigureAwait(false);
                }
                else
                {
                    RepositoryWrapper.QuickHelpRepository.RemoveQuickHelp(quickHelp);
                    RepositoryWrapper.QuickHelpRepository.SaveChanges();

                    var embDeleted = new EmbedBuilder()
                    {
                        Description = $"Quick help tag **{tag.ToLower()}** has been deleted.",
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embDeleted.Build()).ConfigureAwait(false);
                }
            }
        }

        [Command("help")]
        [Alias("h")]
        [Summary("Returns quick help for given tag.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task HelpAsync([Remainder] string tag = null)
        {
            if (string.IsNullOrEmpty(tag))
                await SendQuickHelpList(true).ConfigureAwait(false);
            else
            {
                var quickHelp = await RepositoryWrapper.QuickHelpRepository.GetQuickHelpByTagAsync(tag.ToLower());
                if (quickHelp.IsObjectNull() || quickHelp.IsEmpty())
                {
                    var embNotFound = new EmbedBuilder()
                    {
                        Description = $"Quick help tag **{tag.ToLower()}** could not be found. Typo?\nPlease check following quick help tags:",
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                    await SendQuickHelpList(false).ConfigureAwait(false);
                }
                else
                {
                    var embFound = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName($"Quick help regarding: - {tag.ToLower()} -").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                        Description = quickHelp.Help,
                        Color = Constants.SuccessColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    if (quickHelp.Help.Contains("http") &&
                    (quickHelp.Help.Contains(".png") ||
                    quickHelp.Help.Contains(".jpg") ||
                    quickHelp.Help.Contains(".jpeg")))
                    {
                        var stringChunks = quickHelp.Help.Split(' ');

                        foreach (var url in stringChunks)
                        {
                            if (url.StartsWith("http", StringComparison.Ordinal))
                            {
                                embFound.ImageUrl = url;
                                break;
                            }
                        }
                    }

                    await ReplyAsync("", false, embFound.Build()).ConfigureAwait(false);
                }
            }
        }

        private async Task SendQuickHelpList(bool tagNull)
        {
            if (tagNull)
                await ReplyAsync("Quick help tag is null or empty. Following quick help tags exist:\n\n").ConfigureAwait(false);

            var stringBuilder = new StringBuilder();

            foreach (var help in await RepositoryWrapper.QuickHelpRepository.GetAllQuickHelps())
            {
                if (stringBuilder.Length + help.Tag.Length >= 2000)
                {
                    var embPartially = new EmbedBuilder()
                    {
                        Description = stringBuilder.ToString().Replace("||", "|"),
                        Color = Constants.FailureColor,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embPartially.Build()).ConfigureAwait(false);

                    stringBuilder.Clear();
                    stringBuilder.Append($"| **{help.Tag}** |");
                }
                else
                    stringBuilder.Append($"| **{help.Tag}** |");
            }

            var emb = new EmbedBuilder()
            {
                Description = stringBuilder.ToString().Replace("||", "|"),
                Color = Constants.InfoColor,
                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
            };

            await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
        }
    }
}