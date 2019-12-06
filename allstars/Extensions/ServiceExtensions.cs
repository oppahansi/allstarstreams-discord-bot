using allstars.Contexts;
using allstars.Repositories;
using allstars.Services;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;

namespace allstars.Extensions
{
    public static class ServiceExtensions
    {
        public static void ConfigureDiscordConfig(this IServiceCollection services)
        {
            services.AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
#if DEBUG
                LogLevel = LogSeverity.Debug,
#endif
#if RELEASE
                LogLevel = LogSeverity.Info,
#endif
                MessageCacheSize = 1000
            }));

            services.AddSingleton(new CommandService(new CommandServiceConfig
            {
                DefaultRunMode = RunMode.Async,
#if DEBUG
                LogLevel = LogSeverity.Debug,
#endif
#if RELEASE
                LogLevel = LogSeverity.Info,
#endif
            }));
        }

        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<LoggingService>();            
            services.AddSingleton<StartupService>();
            services.AddSingleton<WebService>();
            services.AddSingleton<MuteService>();
            services.AddSingleton<CmdUserCdService>();
            services.AddSingleton<UpcomingReleasesService>();
            services.AddSingleton<AutoMessageService>();
            services.AddSingleton<EventService>();
        }

        public static void ConfigureDatabaseContext(this IServiceCollection services)
        {
            services.AddDbContext<BotDbContext>(o => o.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, Constants.ConfigDbName)}"));
        }

        public static void ConfigureRepositoryWrapper(this IServiceCollection services)
        {
            services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
        }
    }
}