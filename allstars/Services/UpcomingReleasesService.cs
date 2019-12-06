using allstars.Extensions;
using allstars.Models;
using allstars.Repositories;
using allstars.Utils;
using Discord;
using Discord.WebSocket;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace allstars.Services
{
    internal class UpcomingReleasesService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient DiscordClient;
        private readonly IConfigurationRoot Config;
        private IRepositoryWrapper RepositoryWrapper;
        private readonly Timer TimerCheck;
        private readonly Timer TimerPost;

        public UpcomingReleasesService(DiscordSocketClient client, IConfigurationRoot config, IRepositoryWrapper repositoryWrapper)
        {
            DiscordClient = client;
            Config = config;
            RepositoryWrapper = repositoryWrapper;

            TimerCheck = new Timer(_ =>
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Factory.StartNew(async () => { await CheckUpcomingReleasesAsync().ConfigureAwait(false); }));
            },
            null,
            TimeSpan.FromSeconds(60),
            TimeSpan.FromHours(1));

            TimerPost = new Timer(_ =>
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Factory.StartNew(async () => { await PostReleasesAsync().ConfigureAwait(false); }));
            },
            null,
            TimeSpan.FromSeconds(120),
            TimeSpan.FromSeconds(20));
        }

        private async Task CheckUpcomingReleasesAsync()
        {
            Log.Info($"Checking upcoming releaes..");

            try
            {
                using (var webClient = new WebClient())
                {
                    var bluRayJsonObjects = new List<JObject>();
                    var dateNow = DateTime.UtcNow;
                    webClient.Headers.Add("User-Agent: Other");

                    string htmlCode = webClient.DownloadString(new Uri(string.Format(Constants.BluRayReleasesLink, dateNow.Year, dateNow.Month)));

                    while (true)
                    {
                        var cancelPosition = htmlCode.IndexOf("function initLayout");
                        if (cancelPosition < 20)
                            break;

                        var currentPosition = htmlCode.IndexOf("movies[");
                        var currentPosiiontJson = htmlCode.IndexOf("{", currentPosition);
                        var jsonEndPosition = htmlCode.IndexOf("};", currentPosiiontJson);
                        var json = htmlCode.Substring(currentPosiiontJson, jsonEndPosition - currentPosiiontJson + 1);

                        bluRayJsonObjects.Add(JObject.Parse(json));

                        htmlCode = htmlCode.Substring(jsonEndPosition);
                    }

                    var upcomingBlueRayReleases = new List<UpcomingRelease>();
                    var upcomingUhdReleases = new List<UpcomingRelease>();

                    foreach (var jsonObject in bluRayJsonObjects)
                    {
                        var title = jsonObject["title"].ToString();
                        var titleExtended = jsonObject["extended"].ToString();
                        var imageUrl = string.Format(Constants.BluRayImageLink, jsonObject["id"]);
                        var datePieces = jsonObject["releasedate"].ToString().Replace(",", "").Split(" ");
                        var day = int.Parse(datePieces[1].StartsWith("0") ? datePieces[1].Substring(1) : datePieces[1]);

                        var bluRayReleaseDate = new DateTime(dateNow.Year, dateNow.Month, day);

                        var newUpcomingRelease = new UpcomingRelease()
                        {
                            Title = title,
                            TitleExtended = titleExtended,
                            ImageUrl = imageUrl,
                            DvdReleaseDate = new DateTime(),
                            DvdReleasePosted = false,
                            BluRayReleaseDate = new DateTime(),
                            BluRayPosted = false,
                            UhdReleaseDate = new DateTime(),
                            UhdPosted = false
                        };

                        if (newUpcomingRelease.Title.ToLower().Contains("4k"))
                        {
                            newUpcomingRelease.UhdReleaseDate = bluRayReleaseDate;

                            if (upcomingUhdReleases.Find(x => x.Title.ToLower().CompareTo(newUpcomingRelease.Title.ToLower()) == 0) == null)
                                upcomingUhdReleases.Add(newUpcomingRelease);
                        }
                        else
                        {
                            newUpcomingRelease.BluRayReleaseDate = bluRayReleaseDate;

                            if (upcomingBlueRayReleases.Find(x => x.Title.ToLower().CompareTo(newUpcomingRelease.Title.ToLower()) == 0) == null)
                                upcomingBlueRayReleases.Add(newUpcomingRelease);
                        }
                    }

                    foreach (var release in upcomingUhdReleases)
                    {
                        var match = upcomingBlueRayReleases.Find(x => x.Title.ToLower().Contains(release.Title.ToLower().Replace(" 4k", "")));
                        if (match == null)
                            upcomingBlueRayReleases.Add(release);
                        else
                            match.UhdReleaseDate = release.UhdReleaseDate;
                    }

                    htmlCode = webClient.DownloadString(new Uri(string.Format(Constants.DvdReleasesLink, dateNow.Year, dateNow.Month)));

                    var htmlDoc = new HtmlDocument();
                    htmlDoc.LoadHtml(htmlCode);

                    var releaseDays = htmlDoc.DocumentNode.SelectNodes("//table[@class='fieldtable-inner']");
                    var upcomingDvdReleases = new List<UpcomingRelease>();

                    foreach (var releaseDay in releaseDays)
                    {
                        var date = releaseDay.ChildNodes[0].ChildNodes[0].InnerText;
                        var datePieces = date.Replace(",", "").Split();
                        var day = int.Parse(datePieces[1].StartsWith("0") ? datePieces[2].Substring(2) : datePieces[2]);

                        var dvdReleaseDate = new DateTime(dateNow.Year, dateNow.Month, day);

                        var movies = releaseDay.Descendants("td").Where(x => x.HasClass("dvdcell"));

                        foreach (var movie in movies)
                        {
                            var imageUrl = movie.ChildNodes[0].ChildNodes[0].Attributes[3].Value;
                            var title = movie.ChildNodes[2].InnerText;

                            if (movie.ChildNodes.Count == 6)
                            {
                                var releaseTypes = movie.ChildNodes[5].ChildNodes[0];

                                var releaseBlueRay = new DateTime();
                                var releaseUhd = new DateTime();

                                if (releaseTypes.ChildNodes.Count == 2)
                                {
                                    var releaseType = releaseTypes.ChildNodes[1].FirstChild.InnerText;
                                    if (releaseType.ToLower().CompareTo("Blu-ray") == 0)
                                        releaseBlueRay = dvdReleaseDate;
                                    else
                                        releaseUhd = dvdReleaseDate;
                                }
                                else if (releaseTypes.ChildNodes.Count == 3)
                                {
                                    releaseBlueRay = dvdReleaseDate;
                                    releaseUhd = dvdReleaseDate;
                                }

                                upcomingDvdReleases.Add(new UpcomingRelease()
                                {
                                    Title = title,
                                    ImageUrl = imageUrl,
                                    DvdReleaseDate = dvdReleaseDate,
                                    DvdReleasePosted = false,
                                    BluRayReleaseDate = releaseBlueRay,
                                    BluRayPosted = false,
                                    UhdReleaseDate = releaseUhd,
                                    UhdPosted = false
                                });
                            }
                        }
                    }

                    foreach (var dvdRelease in upcomingDvdReleases)
                    {
                        var movieMatch = upcomingBlueRayReleases.Find(x => x.Title.ToLower().Contains(dvdRelease.Title.ToLower()));
                        if (movieMatch != null)
                            movieMatch.DvdReleaseDate = dvdRelease.DvdReleaseDate;
                    }

                    foreach (var upcomingRelease in upcomingBlueRayReleases)
                    {
                        if (upcomingRelease.DvdReleaseDate == new DateTime())
                            upcomingRelease.DvdReleasePosted = true;

                        if (upcomingRelease.BluRayReleaseDate == new DateTime())
                            upcomingRelease.BluRayPosted = true;

                        if (upcomingRelease.UhdReleaseDate == new DateTime())
                            upcomingRelease.UhdPosted = true;
                    }

                    var releasesInDb = await RepositoryWrapper.UpcomingReleaseRepository.GetAllUpcomingReleases();
                    var toBeRemovedList = new List<UpcomingRelease>();

                    
                    foreach (var release in upcomingBlueRayReleases)
                    {
                        var match = (releasesInDb as List<UpcomingRelease>).Find(x => x.Title.ToLower().CompareTo(release.Title.ToLower()) == 0);
                        if (match != null)
                            toBeRemovedList.Add(release);
                    }

                    upcomingBlueRayReleases.RemoveAll(x => toBeRemovedList.Contains(x));

                    try
                    {
                        await RepositoryWrapper.UpcomingReleaseRepository.AddManyUpcomingReleasesAsync(upcomingBlueRayReleases);
                        await RepositoryWrapper.UpcomingReleaseRepository.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Adding new releases failed: {ex.Message}");

                        if (ex.InnerException != null)
                            Log.Error($"{ex.InnerException.Message}");
                    }

                    Log.Info($"Successfully checked upcoming releases.");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");
            }
        }

        private async Task PostReleasesAsync()
        {
            var releasesInDb = await RepositoryWrapper.UpcomingReleaseRepository.GetUpcomingReleases(DateTime.UtcNow.Month, true);
            var releasesSpecialChannel = await RepositoryWrapper.SpecialChannelRepository.GetSpecialChannelByTypeAsync("releases");
            var guild = DiscordClient.GetGuild(ulong.Parse(Config[Constants.ConfigGuildId]));
            var counter = 0;

            if (releasesSpecialChannel.IsObjectNull() || releasesSpecialChannel.IsEmpty())
                Log.Error($"Releases channel has not been set.");
            else
            {
                foreach (var release in releasesInDb)
                {
                    var dvdRelease = false;
                    if (DateTime.UtcNow.Year == release.DvdReleaseDate.Year &&
                        DateTime.UtcNow.Month == release.DvdReleaseDate.Month &&
                        DateTime.UtcNow.Day == release.DvdReleaseDate.Day)
                    {
                        dvdRelease = true;
                    }

                    var bluRayRelease = false;
                    if (DateTime.UtcNow.Year == release.BluRayReleaseDate.Year &&
                        DateTime.UtcNow.Month == release.BluRayReleaseDate.Month &&
                        DateTime.UtcNow.Day == release.BluRayReleaseDate.Day)
                    {
                        bluRayRelease = true;
                    }

                    var uhdRelease = false;
                    if (DateTime.UtcNow.Year == release.UhdReleaseDate.Year &&
                        DateTime.UtcNow.Month == release.UhdReleaseDate.Month &&
                        DateTime.UtcNow.Day == release.UhdReleaseDate.Day)
                    {
                        uhdRelease = true;
                    }

                    if ((dvdRelease && !release.DvdReleasePosted) || (bluRayRelease && !release.BluRayPosted) || (uhdRelease && !release.UhdPosted))
                    {
                        var embRelease = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName($"{release.Title} : {release.TitleExtended}").WithIconUrl(release.ImageUrl),
                            Color = Constants.InfoColor,
                            ImageUrl = release.ImageUrl,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        }
                        .AddField(new EmbedFieldBuilder().WithName("Is now available on:").WithIsInline(true)
                        .WithValue((dvdRelease ? "| DVD |" : "") + (bluRayRelease ? "| Blu-Ray| " : "") + (uhdRelease ? "| Blu-Ray 4K |" : "")));

                        if (!release.DvdReleasePosted && dvdRelease)
                            release.DvdReleasePosted = dvdRelease;

                        if (!release.BluRayPosted && bluRayRelease)
                            release.BluRayPosted = bluRayRelease;

                        if (!release.UhdPosted && uhdRelease)
                            release.UhdPosted = uhdRelease;

                        try
                        {
                            RepositoryWrapper.UpcomingReleaseRepository.UpdateUpcomingRelease(release);
                            RepositoryWrapper.UpcomingReleaseRepository.SaveChanges();
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Updating failed: {ex.Message}");

                            if (ex.InnerException != null)
                                Log.Error($"{ex.InnerException.Message}");
                        }

                        if (guild.GetChannel(releasesSpecialChannel.Id) is ITextChannel guildReleasesChannel)
                            await guildReleasesChannel.SendMessageAsync("", false, embRelease.Build()).ConfigureAwait(false);

                        counter++;
                        if (counter == 3)
                            break;
                    }
                }
            }
        }
    }
}