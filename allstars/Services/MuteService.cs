using allstars.Repositories;
using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace allstars.Services
{
    public class MuteService
    {
        private readonly Logger Log = LogManager.GetCurrentClassLogger();
        private readonly DiscordSocketClient DiscordClient;
        private readonly IConfigurationRoot Config;
        private IRepositoryWrapper RepositoryWrapper;
        private readonly Timer Timer;

        public MuteService(DiscordSocketClient client, IConfigurationRoot config, IRepositoryWrapper repositoryWrapper)
        {
            DiscordClient = client;
            Config = config;
            RepositoryWrapper = repositoryWrapper;

            Timer = new Timer(_ =>
            {
                var tasks = new List<Task>();
                tasks.Add(Task.Factory.StartNew(async () => { await CheckMutes().ConfigureAwait(false); }));
            },
                null,
                TimeSpan.FromSeconds(30),
                TimeSpan.FromSeconds(30));
        }

        private async Task CheckMutes()
        {
            var activeMutes = await RepositoryWrapper.MuteRepository.GetAllActiveMutesAsync();
            SocketGuild guild = null;
            IRole role = null;
            var counter = 0;

            foreach (var mute in activeMutes)
            {
                if (DateTime.Compare(mute.Expires, DateTime.UtcNow) <= 0)
                {
                    if (guild == null)
                        guild = DiscordClient.GetGuild(mute.GuildId);

                    if (role == null && guild != null)
                        role = guild.Roles.Where(x => x.Name.ToLower().CompareTo("muted") == 0).FirstOrDefault();

                    mute.Active = false;
                    RepositoryWrapper.MuteRepository.UpdateMute(mute);

                    var guildUser = guild.GetUser(mute.UserId);

                    await guildUser.RemoveRoleAsync(role).ConfigureAwait(false);

                    counter++;
                }
            }

            RepositoryWrapper.MuteRepository.SaveChanges();

            if (counter > 0)
                Log.Info($"{counter} mutes have been removed.");
        }
    }
}