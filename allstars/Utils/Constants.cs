using Discord;
using System.Collections.Generic;

namespace allstars.Utils
{
    public static class Constants
    {
        public readonly static string ConfigFile = "config.json";
        public readonly static string ConfigVaderstvAccInfoFile = "vadertvaccinfofile";
        public readonly static string ConfigLightStreamsAccInfoFile = "lightstreamaccinfofile";
        public readonly static string ConfigBeastTvAccInfoFile = "beasttvaccinfofile";
        public readonly static string ConfigDevBotToken = "tokens:dev";
        public readonly static string ConfigLiveBotToken = "tokens:live";
        public readonly static string ConfigCommandPrefix = "prefix";
        public readonly static string ConfigLogo = "logo";
        public readonly static string ConfigTmdbApiKey = "tokens:tmdbApiKey";
        public readonly static string ConfigOmdbApiKey = "tokens:omdbApiKey";
        public readonly static string ConfigDbName = "allstars.db";
        public readonly static string ConfigBotId = "tokens:botid";
        public readonly static string ConfigGuildId = "tokens:guildid";
        public readonly static string ConfigOwnerId = "tokens:ownerid";
        public readonly static string ConfigCreatorId = "tokens:creatorid";
        public readonly static string ConfigSpreadSheetId = "tokens:spreadsheetid";
        public readonly static string ConfigSheet = "tokens:sheet";
        public readonly static string ConfigGoogleAppName = "tokens:googleappname";
        public readonly static string ConfigVaderStreams = "verifiedRoleCodes:vaderstreams";
        public readonly static string ConfigLightStreams = "verifiedRoleCodes:lightstreams";
        public readonly static string ConfigDharma = "verifiedRoleCodes:dharma";
        public readonly static string ConfigHoloDisc = "verifiedRoleCodes:holodisc";
        public readonly static string ConfigBeastTv = "verifiedRoleCodes:beasttv";
        public readonly static string ConfigMinAccAge = "configValues:minAccAge";
        public readonly static string ConfigArchiving = "configValues:messageArchive";
        public readonly static string ConfigCmdDefaults = "cmdDefaults";
        public readonly static string ConfigLightEmailTemplate = "lightemailtemplate";
        public readonly static string ConfigBeastEmailTemplate = "beastemailtemplate";
        public readonly static string ConfigVaderEmailTemplate = "vaderemailtemplate";
        public readonly static string ConfigBillingEmailAddress = "billingemailaddress";
        public readonly static string ConfigBillingEmailAddressPassword = "billingemailaddresspassword";
        public readonly static string ConfigMailHost = "mailhost";

        public readonly static string LogsFolderName = "Logs";

        public readonly static string VerifiedVaderStreamRole = "vaderstreams";
        public readonly static string VerifiedLightStreamRole = "lightstreams";
        public readonly static string VerifiedHoloDiscRole = "holodisc";
        public readonly static string VerifiedBeastTvRole = "beasttv";
        public readonly static string VerifiedDharmaRole = "dharma";

        public readonly static List<string> VerificationTypes = new List<string>() { VerifiedVaderStreamRole, VerifiedLightStreamRole, VerifiedHoloDiscRole, VerifiedDharmaRole, VerifiedBeastTvRole };

        public readonly static string LogMessageLayout = @"${date:format=HH\:mm\:ss} | ${level:uppercase=true:padding=-5} | ${message}";

        public readonly static string AllStarsUrl = "https://www.allstarstreams.com/";
        public readonly static string RandomCatApiLink = "http://thecatapi.com/api/images/get?api_key=MTQwMzA0";
        public readonly static string RandomDogApiLink = "https://dog.ceo/api/breeds/image/random";
        public readonly static string DesktopNexusBase = "https://www.desktopnexus.com/";
        public readonly static string DesktopNexusSearch = "https://www.desktopnexus.com/search/";
        public readonly static string JokesCnApiLink = "http://api.icndb.com/jokes/random/";
        public readonly static string JokesYmApiLink = "http://api.yomomma.info/";
        public readonly static string KitsuAnime = "https://kitsu.io/anime/";
        public readonly static string KitsuManga = "https://kitsu.io/manga/";
        public readonly static string UrbanDictionaryApiUrl = "http://api.urbandictionary.com/v0/define?term=";
        public readonly static string XkcdLinkRandom = "https://c.xkcd.com/random/comic/";
        public readonly static string XkcdLink = "https://xkcd.com/";
        public readonly static string TmdbUrl = "https://www.themoviedb.org/";
        public readonly static string TmdbApiBaseLink = "https://api.themoviedb.org/3/search/movie?api_key={0}&query={1}";
        public readonly static string TmdbMovieBaseLink = "https://www.themoviedb.org/movie/";
        public readonly static string TmdbMovieImageBaseLink = "https://image.tmdb.org/t/p/w500";
        public readonly static string ImdbBaseLink = "https://www.imdb.com/title/";
        public readonly static string BluRayReleasesLink = "https://www.blu-ray.com/movies/releasedates.php?year={0}&month={1}";
        public readonly static string BluRayImageLink = "https://images.blu-ray.com/movies/covers/{0}_front.jpg";
        public readonly static string DvdReleasesLink = "https://www.dvdsreleasedates.com/releases/{0}/{1}/";
        public readonly static string BoobsApiLink = "http://api.oboobs.ru/boobs/";
        public readonly static string BoobsMediaLink = "http://media.oboobs.ru/";
        public readonly static string ButtsApiLink = "http://api.obutts.ru/butts/";
        public readonly static string ButtsMediaLink = "http://media.obutts.ru/";
        public readonly static string Rule34Link = "https://r34-json-api.herokuapp.com/posts?tags=";

        public readonly static Color SuccessColor = new Color(127, 255, 0);
        public readonly static Color FailureColor = new Color(220, 20, 60);
        public readonly static Color InfoColor = new Color(204, 204, 0);

        public static List<string> ChannelTypes = new List<string>() { "log", "announce", "tickets", "archive", "releases", "mail" };

        public enum UpcomingReleaeType
        {
            DVD = 0,
            BLURAY = 1,
            UHD = 2
        }
    }
}