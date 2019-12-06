using allstars.Repositories;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace allstars.Services
{
    public class CmdUserCdService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient DiscordClient;
        private readonly IConfigurationRoot Config;
        private IRepositoryWrapper RepositoryWrapper;
        private readonly Timer Timer;

        public CmdUserCdService(DiscordSocketClient client, IConfigurationRoot config, IRepositoryWrapper repositoryWrapper)
        {
            DiscordClient = client;
            Config = config;
            RepositoryWrapper = repositoryWrapper;

            Timer = new Timer(_ =>
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Factory.StartNew(async () => { await CheckExpiredCmdUserCds().ConfigureAwait(false); }));
            },
                null,
                TimeSpan.FromSeconds(45),
                TimeSpan.FromSeconds(180));
        }

        private async Task CheckExpiredCmdUserCds()
        {
            var expiredCds = await RepositoryWrapper.CmdUserCdRepository.GetExpiredCmdUserCds();

            RepositoryWrapper.CmdUserCdRepository.DeleteManyCmdUserCds(expiredCds);
            RepositoryWrapper.CmdUserCdRepository.SaveChanges();
        }
    }
}