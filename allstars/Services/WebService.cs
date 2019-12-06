using allstars.Utils;
using Discord;
using HtmlAgilityPack;
using Kitsu.Anime;
using Kitsu.Manga;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using NLog;
using OMDbApiNet;
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMDbLib.Client;
using TMDbLib.Objects.General;
using TMDbLib.Objects.Search;

namespace allstars.Services
{
    public class WebService
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private IConfigurationRoot Config;

        public WebService(IConfigurationRoot config)
        {
            Config = config;
        }

        internal async Task<Embed> GetRandomCatAsync()
        {
            Log.Info($"Getting random Cat picture..");

            try
            {
                using (var http = new HttpClient())
                {
                    var response = await http.GetAsync(Constants.RandomCatApiLink);
                    var url = response.RequestMessage.RequestUri.ToString();
                    var emb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName("Random Cats").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://thecatapi.com/"),
                        Color = Constants.SuccessColor,
                        ImageUrl = url,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Cat not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> GetRandomDogAsync()
        {
            Log.Info($"Getting random Dog picture..");

            try
            {
                JObject obj;
                using (var http = new HttpClient())
                {
                    obj = JObject.Parse(await http.GetStringAsync(Constants.RandomDogApiLink));
                }

                var imgLink = obj["message"];

                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("Random Dogs").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("https://www.thedogapi.co.uk/"),
                    Color = Constants.SuccessColor,
                    ImageUrl = imgLink.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Dog not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> GetWallpaperLinksAsync(string tag)
        {
            EmbedBuilder emb;
            if (tag != null)
            {
                var rng = new Random();
                var baseSearchLink = $"{Constants.DesktopNexusSearch}" + Uri.EscapeUriString(tag);

                try
                {
                    Log.Info($"Getting Wallpaper for tag : {tag}");

                    using (var http = new HttpClient())
                    {
                        var searchResults = await http.GetStringAsync(baseSearchLink);
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(searchResults);
                        var pagesDiv = htmlDoc.DocumentNode.SelectNodes("//div[@class ='pageLinksDiv']//span");
                        var wallPaperThumb = "";
                        var wallPaperLink = "";

                        if (pagesDiv.Count != 0)
                        {
                            var pagesCount = Int32.Parse(Regex.Matches(pagesDiv[0].InnerText, "(\\d+)")[0].Value);

                            var randomPageValue = rng.Next(0, pagesCount);
                            var randomPage = randomPageValue == 0 ? "" : randomPageValue + "";

                            searchResults = await http.GetStringAsync(baseSearchLink + $"/{randomPage}");
                            htmlDoc.LoadHtml(searchResults);

                            var wallPaperLinks = htmlDoc.DocumentNode.SelectNodes("//div[@id='middlecolumn']//div//a[@href]");

                            var randomWallPaper = wallPaperLinks[rng.Next(0, wallPaperLinks.Count)];
                            wallPaperLink = "https:" + randomWallPaper.GetAttributeValue("href", null);
                            wallPaperThumb = "https:" + randomWallPaper.ChildNodes[0].GetAttributeValue("src", null);
                        }

                        emb = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName("Wallpaper from DesktopNexus").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(wallPaperLink),
                            Color = pagesDiv.Count != 0 ? Constants.SuccessColor : Constants.FailureColor,
                            ImageUrl = wallPaperThumb,
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"There was an exception. {ex.Message}");

                    emb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName("Wallpaper from DesktopNexus").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.DesktopNexusBase),
                        Color = Constants.FailureColor,
                        Description = $"💢 {ex.Message}",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                }
            }
            else
            {
                emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("Wallpaper from DesktopNexus").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.DesktopNexusBase),
                    Color = Constants.FailureColor,
                    Description = $"💢 Search tag cannot be null."
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> GetRandomCnJokeAsync()
        {
            try
            {
                Log.Info($"Getting random Chuck Norris Joke..");

                using (var http = new HttpClient())
                {
                    var response = await http.GetStringAsync(Constants.JokesCnApiLink);

                    var emb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName("Chuck Norris Jokes").WithUrl("http://www.icndb.com/"),
                        Color = Constants.SuccessColor,
                        Description = JObject.Parse(response)["value"]["joke"].ToString(),
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Chuck Norris Joke not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> GetRandomYoMommaJokeAsync()
        {
            try
            {
                Log.Info($"Getting random Yomomma Joke..");

                using (var http = new HttpClient())
                {
                    var response = await http.GetStringAsync(Constants.JokesYmApiLink);

                    var emb = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName("Yomomma Jokes").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("https://github.com/KiaFathi/tambalAPI"),
                        Color = Constants.SuccessColor,
                        Description = JObject.Parse(response)["joke"].ToString(),
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "YoMomma joke not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> SearchAnimeAsync(string searchTerm)
        {
            Log.Info($"Searching for {searchTerm}..");

            try
            {
                var anime = await Anime.GetAnimeAsync(searchTerm);
                var animeFirst = anime.Data[0];

                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(animeFirst.Attributes.Titles.EnJp).WithIconUrl(animeFirst.Attributes.PosterImage.Original).WithUrl(Constants.KitsuAnime + animeFirst.Id),
                    Color = Constants.SuccessColor,
                    Url = Constants.KitsuAnime + animeFirst.Id,
                    ImageUrl = animeFirst.Attributes.PosterImage.Original,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                }
                    .AddField(new EmbedFieldBuilder().WithName("Start").WithIsInline(true).WithValue(animeFirst.Attributes.StartDate ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Status").WithIsInline(true).WithValue(animeFirst.Attributes.Status ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Episodes").WithIsInline(true).WithValue(animeFirst.Attributes.EpisodeCount == null ? "???" : animeFirst.Attributes.EpisodeCount.ToString()))
                    .AddField(new EmbedFieldBuilder().WithName("Type").WithIsInline(true).WithValue(animeFirst.Attributes.Subtype ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Average Score").WithIsInline(true).WithValue(animeFirst.Attributes.AverageRating ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Title Japanese").WithIsInline(true).WithValue(animeFirst.Attributes.Titles.JaJp ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Description").WithIsInline(false).WithValue(animeFirst.Attributes.Synopsis.Length > 500 ? animeFirst.Attributes.Synopsis.Substring(0, 500) : animeFirst.Attributes.Synopsis));

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Anime not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> SearchMangaAsync(string searchTerm)
        {
            Log.Info($"Searching for {searchTerm}..");

            try
            {
                var manga = await Manga.GetMangaAsync(searchTerm, 0);
                var mangaFirst = manga.Data[0];

                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName(mangaFirst.Attributes.Titles.EnJp).WithIconUrl(mangaFirst.Attributes.PosterImage.Original).WithUrl(Constants.KitsuManga + mangaFirst.Id),
                    Color = Constants.SuccessColor,
                    Url = Constants.KitsuAnime + mangaFirst.Id,
                    ImageUrl = mangaFirst.Attributes.PosterImage.Original,
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                }
                    .AddField(new EmbedFieldBuilder().WithName("Start").WithIsInline(true).WithValue(mangaFirst.Attributes.StartDate ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Status").WithIsInline(true).WithValue(mangaFirst.Attributes.Status ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Chapters").WithIsInline(true).WithValue(mangaFirst.Attributes.ChapterCount == null ? "???" : mangaFirst.Attributes.ChapterCount.ToString()))
                    .AddField(new EmbedFieldBuilder().WithName("Type").WithIsInline(true).WithValue(mangaFirst.Attributes.Subtype ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Average Score").WithIsInline(true).WithValue(mangaFirst.Attributes.AverageRating ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Title Japanese").WithIsInline(true).WithValue(mangaFirst.Attributes.Titles.JaJp ?? "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Description").WithIsInline(false).WithValue(mangaFirst.Attributes.Synopsis.Length > 500 ? mangaFirst.Attributes.Synopsis.Substring(0, 500) : mangaFirst.Attributes.Synopsis));

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Manga not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> GetUrbanDefinitionAsync(string searchToken)
        {
            try
            {
                Log.Info($"Getting Urban Definition..");

                using (var http = new HttpClient())
                {
                    var response = await http.GetStringAsync(Constants.UrbanDictionaryApiUrl + searchToken);
                    var result = JObject.Parse(response)["list"];

                    if (result == null || !result.HasValues)
                    {
                        var emb = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName("Urban Dictionary").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://www.urbandictionary.com"),
                            Color = Constants.FailureColor,
                            Description = $"💢 There is no definition of it.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        var emb = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName("Urban Dictionary").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://www.urbandictionary.com"),
                            Color = Constants.SuccessColor,
                            Description = result[0]["definition"].ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        if (!string.IsNullOrEmpty(result[0]["example"].ToString()))
                        {
                            emb.AddField(new EmbedFieldBuilder().WithName("Example").WithIsInline(false).WithValue(result[0]["example"].ToString()));
                        }

                        emb.AddField(new EmbedFieldBuilder().WithName("More Definitions Here").WithIsInline(false).WithValue(result[0]["permalink"].ToString()));

                        return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Definition not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> GetRandomXkcdComicAsync()
        {
            try
            {
                Log.Info($"Getting random Xkcd comic..");

                using (var http = new HttpClient())
                {
                    WebRequest request = WebRequest.Create(Constants.XkcdLinkRandom);
                    WebResponse response = request.GetResponse();

                    if (response != null)
                    {
                        var json = JObject.Parse(await http.GetStringAsync(response.ResponseUri + "/info.0.json"));

                        var emb = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName($"Xkcd Comics: {json["safe_title"].ToString()}").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.XkcdLink),
                            Color = Constants.SuccessColor,
                            Description = response.ResponseUri.AbsoluteUri,
                            ImageUrl = json["img"].ToString(),
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                    }
                    else
                    {
                        var emb = new EmbedBuilder()
                        {
                            Color = Constants.FailureColor,
                            Description = "Xkcd comic not found.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Xkcd comic not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> SearchMovieAsync(string searchTerm)
        {
            Log.Info($"Searching for movie: {searchTerm}..");

            try
            {
                var apiKey = Config[Constants.ConfigTmdbApiKey];
                TMDbClient client = new TMDbClient(apiKey);

                int movieId;
                if (int.TryParse(searchTerm, out movieId))
                {
                    var movie = await client.GetMovieAsync(movieId);

                    var embMovie = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName(movie.Title).WithIconUrl(Constants.TmdbMovieImageBaseLink + movie.PosterPath).WithUrl(Constants.ImdbBaseLink + movie.ImdbId),
                        Color = Constants.SuccessColor,
                        Url = Constants.TmdbMovieImageBaseLink + movie.PosterPath,
                        ImageUrl = Constants.TmdbMovieImageBaseLink + movie.PosterPath,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    }
                    .AddField(new EmbedFieldBuilder().WithName("Release Date").WithIsInline(true).WithValue(movie.ReleaseDate.HasValue ? movie.ReleaseDate.Value.ToShortDateString() : "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Runtime").WithIsInline(true).WithValue(movie.Runtime.HasValue ? movie.Runtime.Value + " mins" : "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Original Language").WithIsInline(true).WithValue(!string.IsNullOrEmpty(movie.OriginalLanguage) ? movie.OriginalLanguage : "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Original Title").WithIsInline(true).WithValue(!string.IsNullOrEmpty(movie.OriginalTitle) ? movie.OriginalTitle : "??"))
                    .AddField(new EmbedFieldBuilder().WithName("Imdb Rating").WithIsInline(true).WithValue(movie.VoteAverage))
                    .AddField(new EmbedFieldBuilder().WithName("Imdb Votes").WithIsInline(true).WithValue(movie.VoteCount))
                    .AddField(new EmbedFieldBuilder().WithName("Overview").WithIsInline(true).WithValue(!string.IsNullOrEmpty(movie.Overview) ? movie.Overview : "??"));

                    return await Task.FromResult(embMovie.Build()).ConfigureAwait(false);
                }

                SearchContainer<SearchMovie> results = await client.SearchMovieAsync(searchTerm);

                if (results.Results.Count == 0)
                {
                    var embError = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = "No movies could be found.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    return await Task.FromResult(embError.Build()).ConfigureAwait(false);
                }

                var stringBuilder = new StringBuilder();
                var resultsRange = results.Results.GetRange(0, results.Results.Count < 20 ? results.Results.Count : 20);
                var counter = 1;

                foreach (var movie in resultsRange)
                {
                    stringBuilder.Append($"**{counter}.** {movie.Title} - !movie **{movie.Id}**\n");
                    counter++;
                }

                stringBuilder.Append($"\nPlease choose one movie and use the movie command with the item id.\nExample:\n\n!movie **123456**");

                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName($"Search results for: {searchTerm}").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(Constants.TmdbUrl),
                    Color = Constants.SuccessColor,
                    Description = stringBuilder.ToString(),
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build());
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Movie not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> SearchSeriesAsync(string searchTerm)
        {
            Log.Info($"Searching for series: {searchTerm}..");

            try
            {
                var omdb = new OmdbClient(Config[Constants.ConfigOmdbApiKey]);
                var series = omdb.GetItemByTitle(searchTerm, OmdbType.Series);

                if (series != null)
                {
                    var embSeries = new EmbedBuilder()
                    {
                        Author = new EmbedAuthorBuilder().WithName(series.Title).WithIconUrl(series.Poster).WithUrl(Constants.ImdbBaseLink + series.ImdbId),
                        Color = Constants.SuccessColor,
                        Url = series.Poster,
                        ImageUrl = series.Poster,
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    }
                .AddField(new EmbedFieldBuilder().WithName("Release Date").WithIsInline(true).WithValue(!string.IsNullOrEmpty(series.Released) ? series.Released : "??"))
                .AddField(new EmbedFieldBuilder().WithName("Language").WithIsInline(true).WithValue(!string.IsNullOrEmpty(series.Language) ? series.Language : "??"))
                .AddField(new EmbedFieldBuilder().WithName("Runtime").WithIsInline(true).WithValue(!string.IsNullOrEmpty(series.Runtime) ? series.Runtime : "??"))
                .AddField(new EmbedFieldBuilder().WithName("Total Seasons").WithIsInline(true).WithValue(!string.IsNullOrEmpty(series.TotalSeasons) ? series.TotalSeasons : "??"))
                .AddField(new EmbedFieldBuilder().WithName("Imdb Rating").WithIsInline(true).WithValue(!string.IsNullOrEmpty(series.ImdbRating) ? series.ImdbRating : "??"))
                .AddField(new EmbedFieldBuilder().WithName("Imdb Votes").WithIsInline(true).WithValue(!string.IsNullOrEmpty(series.ImdbVotes) ? series.ImdbVotes : "??"))
                .AddField(new EmbedFieldBuilder().WithName("Overview").WithIsInline(true).WithValue(!string.IsNullOrEmpty(series.Plot) ? series.Plot : "??"));

                    return await Task.FromResult(embSeries.Build()).ConfigureAwait(false);
                }
                else
                {
                    var emb = new EmbedBuilder()
                    {
                        Color = Constants.FailureColor,
                        Description = "Series not found.",
                        Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                    };

                    return await Task.FromResult(emb.Build()).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Series not found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build()).ConfigureAwait(false);
            }
        }

        internal async Task<Embed> GetBoobsAsync()
        {
            try
            {
                Log.Info($"Getting random Boobs picture..");

                JToken obj;

                using (var http = new HttpClient())
                {
                    obj = JArray.Parse(await http.GetStringAsync($"{Constants.BoobsApiLink}{ new Random().Next(0, 9880) }"))[0];
                }
                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("B(.)(.)Bs").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://oboobs.ru/"),
                    Color = Constants.SuccessColor,
                    ImageUrl = Constants.BoobsMediaLink + obj["preview"],
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                return await Task.FromResult(emb.Build());
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");
                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("B(.)(.)Bs").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://oboobs.ru/"),
                    Color = Constants.FailureColor,
                    Description = $"💢 {ex.Message}",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };
                return await Task.FromResult(emb.Build());
            }
        }

        internal async Task<Embed> GetButtsAsync()
        {
            try
            {
                Log.Info($"Getting random Butts picture..");
                JToken obj;
                using (var http = new HttpClient())
                {
                    obj = JArray.Parse(await http.GetStringAsync($"{Constants.ButtsApiLink}{ new Random().Next(0, 3873) }"))[0];
                }
                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("Butts").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://obutts.ru/"),
                    Color = Constants.SuccessColor,
                    ImageUrl = Constants.ButtsMediaLink + obj["preview"],
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };
                return await Task.FromResult(emb.Build());
            }
            catch (Exception ex)
            {
                Log.Error($"There was an exception. {ex.Message}");
                var emb = new EmbedBuilder()
                {
                    Author = new EmbedAuthorBuilder().WithName("Butts").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://oboobs.ru/"),
                    Color = Constants.FailureColor,
                    Description = $"💢 {ex.Message}",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };
                return await Task.FromResult(emb.Build());
            }
        }

        internal async Task<Embed> GetRule34ImageAsync(string tag)
        {
            try
            {
                Log.Info($"Getting Rule34 picture for tag: {tag}");
                var rng = new Random();
                var url = $"{Constants.Rule34Link}{tag.Replace(" ", "+")}";
                EmbedBuilder emb;

                using (var http = new HttpClient())
                {
                    var respone = await http.GetStringAsync(url);
                    var parsed = JArray.Parse(respone);
                    
                    if (parsed.Count == 0)
                    {
                        emb = new EmbedBuilder()
                        {
                            Author = new EmbedAuthorBuilder().WithName("Rule34").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://rule34.xxx/"),
                            Color = Constants.FailureColor,
                            Description = $"💢 There is no porn of it.",
                            Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                        };

                        return await Task.FromResult(emb.Build());
                    }
                    else
                    {
                        var currentIndex = parsed.Count == 1 ? 0: rng.Next(0, parsed.Count);
                        var imageUrl = parsed[currentIndex]["sample_url"].ToString();

                        if (string.IsNullOrEmpty(imageUrl))
                            imageUrl = parsed[currentIndex]["file_url"].ToString();

                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            emb = new EmbedBuilder()
                            {
                                Author = new EmbedAuthorBuilder().WithName("Rule34").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl(parsed[currentIndex]["file_url"].ToString()),
                                Color = Constants.SuccessColor,
                                ImageUrl = imageUrl,
                                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                            };
                        }
                        else
                        {
                            emb = new EmbedBuilder()
                            {
                                Author = new EmbedAuthorBuilder().WithName("Rule34").WithIconUrl(Config[Constants.ConfigLogo]).WithUrl("http://rule34.xxx/"),
                                Color = Constants.FailureColor,
                                Description = $"💢 There is no porn of it.",
                                Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                            };
                        }

                        return await Task.FromResult(emb.Build());
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"There was an error. {ex.Message}");

                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = "Rule34 picture could not be found.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };
                return await Task.FromResult(emb.Build());
            }
        }
    }
}