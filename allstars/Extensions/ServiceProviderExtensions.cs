using allstars.Contexts;
using allstars.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace allstars.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static async Task GetRequiredServicesAsync(this ServiceProvider provider)
        {
            provider.GetRequiredService<IConfigurationRoot>();
            provider.GetRequiredService<LoggingService>();
            provider.GetRequiredService<BotDbContext>();
            provider.GetRequiredService<WebService>();
            provider.GetRequiredService<AutoMessageService>();
            provider.GetRequiredService<EventService>();
            provider.GetRequiredService<MuteService>();
            provider.GetRequiredService<CmdUserCdService>();
            provider.GetRequiredService<UpcomingReleasesService>();

            await provider.GetRequiredService<StartupService>().StartAsync();
        }
    }
}