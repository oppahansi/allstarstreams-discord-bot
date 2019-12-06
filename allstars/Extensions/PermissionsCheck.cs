using allstars.Models;
using allstars.Repositories;
using allstars.Utils;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace allstars.Extensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal class PermissionCheck : PreconditionAttribute
    {
        public async override Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider provider)
        {
            var user = context.User as IGuildUser;
            var guild = context.Guild as IGuild;
            var channel = context.Channel as IGuildChannel;
            var config = provider.GetService(typeof(IConfigurationRoot)) as IConfigurationRoot;
            var repositoryWrapper = provider.GetService(typeof(IRepositoryWrapper)) as IRepositoryWrapper;
            var cmdRole = await repositoryWrapper.CmdRoleRepository.GetCmdRoleAsync(command.Name);
            var cmdChannels = await repositoryWrapper.CmdChannelRepository.GetCmdChannelsAsync(command.Name);

            ulong.TryParse(config[Constants.ConfigOwnerId], out ulong ownerId);
            ulong.TryParse(config[Constants.ConfigCreatorId], out ulong creatorId);

            if (context.User.Id == ownerId || context.User.Id == creatorId)
                return PreconditionResult.FromSuccess();

            if (config == null)
                return PreconditionResult.FromError("Config is null.");

            var cmdRoleDefaultValue = config[$"cmdDefaults:{command.Name.ToLower()}"];

            var cmdRoleDefault = new CmdRole();
            if (!string.IsNullOrEmpty(cmdRoleDefaultValue))
            {
                var guildRole = guild.Roles.ToList().Find(x => x.Name.ToLower().CompareTo(cmdRoleDefaultValue.ToLower()) == 0);
                if (guildRole == null)
                    return PreconditionResult.FromError("Guild role not found.");

                cmdRoleDefault = new CmdRole() { Command = command.Name, Guild = guild.Id, MinRoleId = guildRole.Id };
            }
            else
                return PreconditionResult.FromError("Default min role not set for the command.");

            if (await CheckChannelPermissions(context, guild, channel, cmdChannels as List<CmdChannel>) && CheckRolePermissions(command.Name.ToLower(), guild, user, cmdRole, cmdRoleDefault))
                return PreconditionResult.FromSuccess();
            else
                return PreconditionResult.FromError("Command cannot be executed. Possible reasons: **wrong channel** or **no permissions**.\n");
        }

        internal async Task<bool> CheckChannelPermissions(ICommandContext context, IGuild guild, IGuildChannel channel, List<CmdChannel> cmdChannels)
        {
            var guildChannels = await guild.GetChannelsAsync();

            if (cmdChannels == null)
                return true;

            if (cmdChannels.Count == 0)
                return true;
            else
            {
                if (cmdChannels.Contains(cmdChannels.Find(x => x.ChannelId == channel.Id)))
                {
                    var guildChannel = await guild.GetChannelAsync(channel.Id);
                    if (guildChannels.ToList().Find(x => x.Id == guildChannel.Id) != null)
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }
        }

        internal bool CheckRolePermissions(string cmd, IGuild guild, IGuildUser user, CmdRole permission, CmdRole cmdRoleDefault)
        {
            var userRoles = user.RoleIds;
            var guildRoles = guild.Roles.ToList();

            IRole guildRole = null;

            if (permission != null)
                guildRole = guildRoles.Find(x => x.Id == permission.MinRoleId);
            else
                guildRole = guildRoles.Find(x => x.Id == cmdRoleDefault.MinRoleId);

            if (guildRole == null)
                return true;
            else
            {
                foreach (var role in userRoles)
                {
                    var userRole = guild.GetRole(role);

                    if (userRole != null)
                    {
                        if (userRole.Position >= guildRole.Position)
                            return true;
                    }
                }

                return false;
            }
        }
    }
}