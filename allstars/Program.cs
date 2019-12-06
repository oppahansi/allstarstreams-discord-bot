using allstars.Extensions;
using allstars.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NLog;
using System;
using System.IO;
using System.Threading.Tasks;

namespace allstars
{
    internal class Program
    {
        public IConfigurationRoot ConfigurationRoot { get; private set; }

        private readonly Logger Log = LogManager.GetCurrentClassLogger();

        private static void Main(string[] args) => new Program().StartAsync().GetAwaiter().GetResult();

        public async Task StartAsync()
        {
            BuildConfig();

            var services = new ServiceCollection();
            services.AddSingleton(ConfigurationRoot);
            services.ConfigureDiscordConfig();
            services.ConfigureServices();
            services.ConfigureDatabaseContext();
            services.ConfigureRepositoryWrapper();

            var provider = services.BuildServiceProvider();
            await provider.GetRequiredServicesAsync();

            // the delay of doom
            await Task.Delay(-1);
        }

        private void BuildConfig()
        {
            try
            {
                var builder = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile(Constants.ConfigFile, optional: false, reloadOnChange: true);

                ConfigurationRoot = builder.Build();
            }
            catch
            {
                Log.Fatal(Messages.ConfigBuildError);
                throw new FileNotFoundException();
            }
        }
    }
}