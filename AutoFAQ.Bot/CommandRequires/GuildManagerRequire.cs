using AutoFAQ.Application.Services;
using AutoFAQ.Entity.Entity.Manager;
using Kook.Commands;
using Kook.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Bot.CommandRequires {
    public class GuildManagerRequire : PreconditionAttribute {
        public override async Task<PreconditionResult> CheckPermissionsAsync(ICommandContext context, CommandInfo command, IServiceProvider services) {
            if (context.User is SocketGuildUser) {
                var socketClient = services.GetRequiredService<KookSocketClient>();
                var guildConfigService = services.GetRequiredService<GuildConfigService>();
                var guildUser = await socketClient.Rest.GetGuildUserAsync(context.Guild.Id, context.User.Id);

                if (await guildConfigService.GetConfigByGuildIdAsync(context.Guild.Id) is GuildConfigEntity config) {
                    var managerRoles = config.ManagerRolesArray;
                    foreach (var role in guildUser.RoleIds) {
                        if (managerRoles.Contains(role))
                            return PreconditionResult.FromSuccess();
                    }

                    await context.Channel.SendKMarkdownMessageAsync("权限检查不通过, 请确保你有有管理问答权限的角色组");
                    return PreconditionResult.FromError($"权限检查不通过, 请确保你有有管理问答权限的角色组");
                }

                await context.Channel.SendKMarkdownMessageAsync("没有设置管理员角色组，请使用 `.faq op @要添加权限的角色组`");
                return PreconditionResult.FromError("该服务器未配置管理员角色组");
            }

            return PreconditionResult.FromError("错误的上下文环境，请在服务器执行");
        }
    }
}
