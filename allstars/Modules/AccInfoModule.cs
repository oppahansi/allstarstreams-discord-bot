using allstars.Extensions;
using allstars.Models;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace allstars.Modules
{
    [RequireContext(ContextType.Guild)]
    public class AccInfoModule : ModuleBase<SocketCommandContext>
    {
        private IConfigurationRoot Config;

        public AccInfoModule(IConfigurationRoot config)
        {
            Config = config;
        }

        [Command("accinfo")]
        [Alias("ai")]
        [Summary("Get account info for given accId or userName")]
        [PermissionCheck]
        public async Task AccInfoAsync([Remainder] string accIdOrUserName = null)
        {
            await VaderTvAccInfoAsync(accIdOrUserName.ToLower()).ConfigureAwait(false);
            await LightStreamsAccInfoAsync(accIdOrUserName.ToLower()).ConfigureAwait(false);
            await BeastTvAccInfoAsync(accIdOrUserName.ToLower()).ConfigureAwait(false);
        }

        private async Task VaderTvAccInfoAsync([Remainder] string accIdOrUserName = null)
        {
            if (string.IsNullOrEmpty(accIdOrUserName))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provided account id or username is invalid.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                return;
            }

            var accInfoList = this.MapVaders(Config);
            var match = accInfoList.Find(x => x.UserName.CompareTo(accIdOrUserName) == 0);

            if (match == null)
            {
                var embNotFound = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Vaderstreams acc info could not be found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                return;
            }

            var embAccInfo = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder().WithName($"Vaderstreams, Acc info:").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                Color = Constants.SuccessColor,
                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
            }
            .AddField(new EmbedFieldBuilder().WithName("Username:").WithIsInline(true).WithValue(match.UserName))
            .AddField(new EmbedFieldBuilder().WithName("Email:").WithIsInline(true).WithValue(match.Email))
            .AddField(new EmbedFieldBuilder().WithName("Status").WithIsInline(true).WithValue(match.Status))
            .AddField(new EmbedFieldBuilder().WithName("Expiration").WithIsInline(true).WithValue(match.Expiration.ToString("MM-dd-yyyy")));

            await ReplyAsync("", false, embAccInfo.Build()).ConfigureAwait(false);
        }

        public async Task LightStreamsAccInfoAsync([Remainder] string username = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provided username is invalid.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                return;
            }

            var accInfoList = this.MapLightStreams(Config);
            var match = accInfoList.Find(x => x.UserName.CompareTo(username.ToLower()) == 0);
            if (match == null)
            {
                var embNotFound = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Lightstreams acc info could not be found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                return;
            }

            var embAccInfo = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder().WithName($"Lightstreams, Acc info:").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                Color = Constants.SuccessColor,
                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
            }
            .AddField(new EmbedFieldBuilder().WithName("Username:").WithIsInline(true).WithValue(match.UserName))
            .AddField(new EmbedFieldBuilder().WithName("Status").WithIsInline(true).WithValue(match.Status))
            .AddField(new EmbedFieldBuilder().WithName("Expiration").WithIsInline(true).WithValue(match.Expiration.ToString("dd/MM/yyyy HH:mm")))
            .AddField(new EmbedFieldBuilder().WithName("Owner").WithIsInline(true).WithValue(match.Owner))
            .AddField(new EmbedFieldBuilder().WithName("Max Connections").WithIsInline(true).WithValue(match.MaxConnections))
            .AddField(new EmbedFieldBuilder().WithName("Notes").WithIsInline(true).WithValue(match.Notes));

            await ReplyAsync("", false, embAccInfo.Build()).ConfigureAwait(false);
        }

        // TODO: Revamp this later to proper impl
        public async Task BeastTvAccInfoAsync([Remainder] string username = null)
        {
            if (string.IsNullOrEmpty(username))
            {
                var embNull = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"Provided username is invalid.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                return;
            }

            var accInfoList = this.MapBeastTv(Config);
            var match = accInfoList.Find(x => x.UserName.CompareTo(username.ToLower()) == 0);
            if (match == null)
            {
                var embNotFound = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"BeastTv acc info could not be found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, embNotFound.Build()).ConfigureAwait(false);
                return;
            }

            var embAccInfo = new EmbedBuilder()
            {
                Author = new EmbedAuthorBuilder().WithName($"BeastTv, Acc info:").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.AllStarsUrl),
                Color = Constants.SuccessColor,
                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
            }
            .AddField(new EmbedFieldBuilder().WithName("Username:").WithIsInline(true).WithValue(match.UserName))
            .AddField(new EmbedFieldBuilder().WithName("Status").WithIsInline(true).WithValue(match.Status))
            .AddField(new EmbedFieldBuilder().WithName("Expiration").WithIsInline(true).WithValue(match.Expiration.ToString("dd/MM/yyyy HH:mm")))
            .AddField(new EmbedFieldBuilder().WithName("Owner").WithIsInline(true).WithValue(match.Owner))
            .AddField(new EmbedFieldBuilder().WithName("Max Connections").WithIsInline(true).WithValue(match.MaxConnections))
            .AddField(new EmbedFieldBuilder().WithName("Notes").WithIsInline(true).WithValue(match.Notes));

            await ReplyAsync("", false, embAccInfo.Build()).ConfigureAwait(false);
        }


        [Command("expired")]
        [Alias("exp")]
        [Summary("Get a list of inactive accounts for given provider.")]
        [PermissionCheck]
        public async Task ExpiredAccInfoAsync(string provider, [Remainder] string when = "0")
        {
            if (string.IsNullOrEmpty(provider) || (provider.ToLower().CompareTo("v") != 0 && provider.ToLower().CompareTo("vader") != 0) && provider.ToLower().CompareTo("l") != 0 && provider.ToLower().CompareTo("lightstreams") != 0 && provider.ToLower().CompareTo("b") != 0 && provider.ToLower().CompareTo("beasttv") != 0)
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

            var stringBuilder = new StringBuilder();
            var embList = new List<Embed>();

            if (provider.ToLower().CompareTo("v") == 0 || provider.ToLower().CompareTo("vader") == 0)
            {
                var vaderAccInfoList = this.MapVaders(Config);
                var inactiveList = vaderAccInfoList.Where(x => x.Status.ToLower().CompareTo("expired") == 0);
                var inactiveListFiltered = new List<VadertvAccInfo>();

                if (DateTime.TryParseExact(when, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime whenDateParsed))
                    inactiveListFiltered = vaderAccInfoList.Where(x => whenDateParsed.CompareTo(x.Expiration) == 0).ToList();
                else
                {
                    DateTime whenDate;
                    if (int.TryParse(when, out int whenValue))
                        whenDate = DateTime.Now.AddDays(whenValue);
                    else
                        whenDate = DateTime.Today;

                    if (whenValue == 0)
                        inactiveListFiltered = inactiveList.Where(x => whenDate.CompareTo(x.Expiration) == 0).ToList();
                    else if (whenValue < 0)
                    {
                        inactiveListFiltered = inactiveList.Where(x => whenDate.CompareTo(x.Expiration) <= 0 && x.Expiration.CompareTo(DateTime.UtcNow) < 0).ToList();
                    }
                    else
                        inactiveListFiltered = vaderAccInfoList.Where(x => whenDate.CompareTo(x.Expiration) >= 0 && x.Expiration.CompareTo(DateTime.UtcNow) > 0).ToList();
                }

                if (inactiveListFiltered == null || inactiveListFiltered.Count() == 0)
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Expired accounts list is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                    return;
                }

                var newLine = "";

                foreach (var inactive in inactiveListFiltered)
                {
                    newLine = $"Username: {inactive.UserName}, Email: {inactive.Email}, Expiration: {inactive.Expiration.ToString("MM-dd-yyyy")}\n";

                    if (stringBuilder.Length + newLine.Length > 1950 && !stringBuilder.ToString().Contains(newLine))
                    {
                        var emb = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = stringBuilder.ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embList.Add(emb.Build());
                        stringBuilder.Clear();
                        stringBuilder.Append(newLine);
                    }
                    else
                        stringBuilder.Append(newLine);
                }

                var embLast = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embList.Add(embLast.Build());

                foreach (var emb in embList)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
            else if (provider.ToLower().CompareTo("l") == 0 || provider.ToLower().CompareTo("lightstreams") == 0)
            {
                var lightAccInfoList = this.MapLightStreams(Config);
                var inactiveList = new List<LightStreamAccInfo>();

                if (DateTime.TryParseExact(when, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime whenDateParsed))
                    inactiveList = lightAccInfoList.Where(x => whenDateParsed.Day == x.Expiration.Day && whenDateParsed.Month == x.Expiration.Month && whenDateParsed.Year == x.Expiration.Year).ToList();

                if (inactiveList == null || inactiveList.Count() == 0)
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Expired accounts list is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                    return;
                }

                var newLine = "";

                foreach (var inactive in inactiveList)
                {
                    newLine = $"Username: {inactive.UserName} , Owner: {inactive.Owner} , Expiration: {(inactive.Expiration.CompareTo(new DateTime()) == 0 ? "---" : inactive.Expiration.ToString("dd/MM/yyyy HH:mm"))},\nNotes: {(inactive.Notes != null ? inactive.Notes : "---")}\n";

                    if (stringBuilder.Length + newLine.Length > 1950 && !stringBuilder.ToString().Contains(newLine))
                    {
                        var emb = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = stringBuilder.ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embList.Add(emb.Build());
                        stringBuilder.Clear();
                        stringBuilder.Append(newLine);
                    }
                    else
                        stringBuilder.Append(newLine);
                }

                var embLast = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embList.Add(embLast.Build());

                foreach (var emb in embList)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
            else
            {
                var beastTvList = this.MapBeastTv(Config);
                var inactiveList = new List<LightStreamAccInfo>();

                if (DateTime.TryParseExact(when, "MM-dd-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime whenDateParsed))
                    inactiveList = beastTvList.Where(x => whenDateParsed.Day == x.Expiration.Day && whenDateParsed.Month == x.Expiration.Month && whenDateParsed.Year == x.Expiration.Year).ToList();

                if (inactiveList == null || inactiveList.Count() == 0)
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Expired accounts list is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                    return;
                }

                var newLine = "";

                foreach (var inactive in inactiveList)
                {
                    newLine = $"Username: {inactive.UserName} , Owner: {inactive.Owner} , Expiration: {(inactive.Expiration.CompareTo(new DateTime()) == 0 ? "---" : inactive.Expiration.ToString("dd/MM/yyyy HH:mm"))},\nNotes: {(inactive.Notes != null ? inactive.Notes : "---")}\n";

                    if (stringBuilder.Length + newLine.Length > 1950 && !stringBuilder.ToString().Contains(newLine))
                    {
                        var emb = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = stringBuilder.ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embList.Add(emb.Build());
                        stringBuilder.Clear();
                        stringBuilder.Append(newLine);
                    }
                    else
                        stringBuilder.Append(newLine);
                }

                var embLast = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embList.Add(embLast.Build());

                foreach (var emb in embList)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
        }

        [Command("disabled")]
        [Alias("dis")]
        [Summary("Get a list of disabled accounts for given provider.")]
        [PermissionCheck]
        public async Task InactiveAccInfoAsync(string provider)
        {
            if (string.IsNullOrEmpty(provider) || (provider.ToLower().CompareTo("v") != 0 && provider.ToLower().CompareTo("vader") != 0) && provider.ToLower().CompareTo("l") != 0 && provider.ToLower().CompareTo("lightstreams") != 0 && provider.ToLower().CompareTo("b") != 0 && provider.ToLower().CompareTo("beasttv") != 0)
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

            var stringBuilder = new StringBuilder();
            var embList = new List<Embed>();

            if (provider.ToLower().CompareTo("v") == 0 || provider.ToLower().CompareTo("vader") == 0)
            {
                var vaderAccInfoList = this.MapVaders(Config);
                var inactiveList = vaderAccInfoList.Where(x => x.Status.ToLower().CompareTo("disabled") == 0);

                if (inactiveList.Count() == 0)
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Disabled accounts list is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                    return;
                }

                var newLine = "";

                foreach (var inactive in inactiveList)
                {
                    newLine = $"Username: {inactive.UserName}\n";

                    if (stringBuilder.Length + newLine.Length > 1950 && !stringBuilder.ToString().Contains(newLine))
                    {
                        var emb = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = stringBuilder.ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embList.Add(emb.Build());
                        stringBuilder.Clear();
                        stringBuilder.Append(newLine);
                    }
                    else
                        stringBuilder.Append(newLine);
                }

                var embLast = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embList.Add(embLast.Build());

                foreach (var emb in embList)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
            else if (provider.ToLower().CompareTo("l") == 0 || provider.ToLower().CompareTo("lightstreams") == 0)
            {
                var lightAccInfoList = this.MapLightStreams(Config);
                var inactiveList = lightAccInfoList.Where(x => x.Status.ToLower().CompareTo("disabled") == 0 || x.Status.ToLower().CompareTo("banned") == 0);

                if (inactiveList.Count() == 0)
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Inactive accounts list is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                    return;
                }

                var newLine = "";

                foreach (var inactive in inactiveList)
                {
                    newLine = $"Username: {inactive.UserName}\n";

                    if (stringBuilder.Length + newLine.Length > 1950 && !stringBuilder.ToString().Contains(newLine))
                    {
                        var emb = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = stringBuilder.ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embList.Add(emb.Build());
                        stringBuilder.Clear();
                        stringBuilder.Append(newLine);
                    }
                    else
                        stringBuilder.Append(newLine);
                }

                var embLast = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embList.Add(embLast.Build());

                foreach (var emb in embList)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
            else
            {
                var beastAccInfoList = this.MapBeastTv(Config);
                var inactiveList = beastAccInfoList.Where(x => x.Status.ToLower().CompareTo("disabled") == 0 || x.Status.ToLower().CompareTo("banned") == 0);

                if (inactiveList.Count() == 0)
                {
                    var embNull = new EmbedBuilder()
                    {
                        Color = Constants.InfoColor,
                        Description = $"Inactive accounts list is empty.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    await ReplyAsync("", false, embNull.Build()).ConfigureAwait(false);
                    return;
                }

                var newLine = "";

                foreach (var inactive in inactiveList)
                {
                    newLine = $"Username: {inactive.UserName}\n";

                    if (stringBuilder.Length + newLine.Length > 1950 && !stringBuilder.ToString().Contains(newLine))
                    {
                        var emb = new EmbedBuilder()
                        {
                            Color = Constants.InfoColor,
                            Description = stringBuilder.ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        embList.Add(emb.Build());
                        stringBuilder.Clear();
                        stringBuilder.Append(newLine);
                    }
                    else
                        stringBuilder.Append(newLine);
                }

                var embLast = new EmbedBuilder()
                {
                    Color = Constants.InfoColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                embList.Add(embLast.Build());

                foreach (var emb in embList)
                    await ReplyAsync("", false, emb).ConfigureAwait(false);
            }
        }
    }
}