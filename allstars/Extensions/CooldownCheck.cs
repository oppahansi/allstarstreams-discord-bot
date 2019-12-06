using allstars.Models;
using allstars.Repositories;
using Discord;
using Discord.Commands;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading.Tasks;

namespace allstars.Extensions
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    internal class CooldownCheck : PreconditionAttribute
    {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider provider)
        {
            var config = provider.GetService(typeof(IConfigurationRoot)) as IConfigurationRoot;
            var configValue = config.GetValue<int>($"cmdCds:{command.Name.ToLower()}");
            var user = context.Message.Author as IGuildUser;

            if (configValue == 0)
                return await Task.FromResult(PreconditionResult.FromSuccess());

            foreach (var role in user.RoleIds)
            {
                if (context.Guild.GetRole(role).Position > 9)
                    return await Task.FromResult(PreconditionResult.FromSuccess());
            }

            var repositoryWrapper = provider.GetService(typeof(IRepositoryWrapper)) as IRepositoryWrapper;
            var userCmdCd = await repositoryWrapper.CmdUserCdRepository.GetCmdUserCd(command.Name.ToLower(), user.Id);

            if (userCmdCd.IsObjectNull() || userCmdCd.IsEmpty())
            {
                userCmdCd = new CmdUserCd()
                {
                    Command = command.Name.ToLower(),
                    Guild = user.GuildId,
                    UserId = user.Id,
                    Expires = DateTime.UtcNow.AddSeconds(configValue)
                };

                await repositoryWrapper.CmdUserCdRepository.AddCmdUserCdAsync(userCmdCd);
                repositoryWrapper.CmdUserCdRepository.SaveChanges();

                return await Task.FromResult(PreconditionResult.FromSuccess());
            }
            else
            {
                if (DateTime.Compare(userCmdCd.Expires, DateTime.UtcNow) <= 0)
                {
                    userCmdCd.Expires = DateTime.UtcNow.AddSeconds(configValue);

                    repositoryWrapper.CmdUserCdRepository.UpdateCmdUserCd(userCmdCd);
                    repositoryWrapper.CmdUserCdRepository.SaveChanges();

                    return await Task.FromResult(PreconditionResult.FromSuccess());
                }
                else
                {
                    userCmdCd.Expires = userCmdCd.Expires.AddSeconds(configValue);
                    return await Task.FromResult(PreconditionResult.FromError($"Command: **{command.Name.ToLower()}** is on CD for you **{user.Username}**, increasing.\nWait for CD to run out."));
                }
            }
        }
    }
}