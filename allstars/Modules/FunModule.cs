using allstars.Extensions;
using allstars.Services;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace allstars.Modules
{
    [RequireContext(ContextType.Guild)]
    public class FunModule : ModuleBase<SocketCommandContext>
    {
        private WebService WebService;
        private readonly IConfigurationRoot Config;

        public FunModule(WebService webService, IConfigurationRoot config)
        {
            WebService = webService;
            Config = config;
        }

        [Command("cat")]
        [Summary("Posts a random cat picture.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task CatAsync()
        {
            await ReplyAsync("", false, await WebService.GetRandomCatAsync());
        }

        [Command("dog")]
        [Summary("Posts a random dog picture.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task DogAsync()
        {
            await ReplyAsync("", false, await WebService.GetRandomDogAsync());
        }

        [Command("wp")]
        [Summary("Gets a random wallpaper for a given tag.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task WpAsync([Remainder] string tag = null)
        {
            await ReplyAsync("", false, await WebService.GetWallpaperLinksAsync(tag));
        }

        [Command("cn")]
        [Summary("Gets a random chuck norris joke.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task ChuckAsync()
        {
            await ReplyAsync("", false, await WebService.GetRandomCnJokeAsync());
        }

        [Command("ym")]
        [Summary("Gets a random yo mamma joke.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task YoMommaAsync()
        {
            await ReplyAsync("", false, await WebService.GetRandomYoMommaJokeAsync());
        }

        [Command("anime")]
        [Summary("Returns an anime for the given search term.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task AnimeAsync([Remainder] string searchTerm = null)
        {
            if (searchTerm == null)
                await ReplyAsync("Search term is null or empty.").ConfigureAwait(false);
            else
                await ReplyAsync("", false, await WebService.SearchAnimeAsync(searchTerm));
        }

        [Command("manga")]
        [Summary("Returns a manga for the given search term.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task MangaAsync([Remainder] string searchTerm = null)
        {
            if (searchTerm == null)
                await ReplyAsync("Search term is null or empty.").ConfigureAwait(false);
            else
                await ReplyAsync("", false, await WebService.SearchMangaAsync(searchTerm));
        }

        [Command("urban")]
        [Summary("Returns an urban definition of given search term.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task UrbanAsync([Remainder] string searchTerm = null)
        {
            if (string.IsNullOrEmpty(searchTerm))
            {
                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"💢 Search token cannot be null or empty.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };
                await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync("", false, await WebService.GetUrbanDefinitionAsync(searchTerm));
            }
        }

        [Command("xkcd")]
        [Summary("Gets a random xkcd comic.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task XkcdAsync()
        {
            await ReplyAsync("", false, await WebService.GetRandomXkcdComicAsync()).ConfigureAwait(false);
        }

        [Command("movie")]
        [Summary("Returns a movie for the given search term.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task MovieAsync([Remainder] string searchTerm = null)
        {
            if (searchTerm == null)
                await ReplyAsync("Search term is null or empty.").ConfigureAwait(false);
            else
                await ReplyAsync("", false, await WebService.SearchMovieAsync(searchTerm));
        }

        [Command("series")]
        [Summary("Returns a movie for the given search term.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task SeriesAsync([Remainder] string searchTerm = null)
        {
            if (searchTerm == null)
                await ReplyAsync("Search term is null or empty.").ConfigureAwait(false);
            else
                await ReplyAsync("", false, await WebService.SearchSeriesAsync(searchTerm));
        }

        [Command("boobs")]
        [Summary("Returns a random boobs picture.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task BoobsAsync()
        {
            await ReplyAsync("", false, await WebService.GetBoobsAsync()).ConfigureAwait(false);
        }

        [Command("butts")]
        [Summary("Returns a random butts picture.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task ButtsAsync()
        {
            await ReplyAsync("", false, await WebService.GetButtsAsync()).ConfigureAwait(false);
        }

        [Command("rule34")]
        [Summary("Returns a rule34 for given tag.")]
        [PermissionCheck]
        [CooldownCheck]
        public async Task Rule34Async([Remainder] string tag = null)
        {
            if (tag == null)
            {
                var emb = new EmbedBuilder()
                {
                    Color = Constants.FailureColor,
                    Description = $"No search term provided.",
                    Footer = new EmbedFooterBuilder().WithIconUrl(Config[Constants.ConfigLogo]).WithText("AllStarStreams")
                };

                await ReplyAsync("", false, emb.Build()).ConfigureAwait(false);
            }
            else
            {
                tag = tag.Trim() ?? "";
                await ReplyAsync("", false, await WebService.GetRule34ImageAsync(tag)).ConfigureAwait(false);
            }
        }
    }
}