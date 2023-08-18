using AutoFAQ.Application.CardMessages;
using AutoFAQ.Application.Services;
using AutoFAQ.Bot.CommandRequires;
using AutoFAQ.Entity.Entity.Manager;
using Kook;
using Kook.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Bot.Modules {
    [Group("faq")]
    [Summary("自动问答模块")]
    [RequireContext(ContextType.Guild)]
    public class AutoFAQModule : ModuleBase<SocketCommandContext> {
        private readonly GuildConfigService _guildConfigService;
        private readonly AutoFAQService _autoFAQService;

        public AutoFAQModule(GuildConfigService guildConfigService, AutoFAQService autoFAQService) {
            _guildConfigService = guildConfigService;
            _autoFAQService = autoFAQService;
        }

        [Command("op")]
        [Summary("给角色组添加管理员权限")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task AddManager(IRole role) {
            await _guildConfigService.UpdateConfigByGuildIdAsync(Context.Guild.Id, (GuildConfigEntity config) => {
                var list = config.ManagerRolesArray.ToList();
                if (!list.Contains(role.Id)) list.Add(role.Id);
                config.ManagerRolesArray = list.ToArray();

                return config;
            });

            await ReplyTextAsync($"已成功为 (rol){role.Id}(rol) 角色组添加管理问答权限", true);
        }

        [Command("deop")]
        [Summary("去除角色组的管理员权限")]
        [RequireUserPermission(GuildPermission.ManageGuild)]
        public async Task RemoveManager(IRole role) {
            await _guildConfigService.UpdateConfigByGuildIdAsync(Context.Guild.Id, (GuildConfigEntity config) => {
                var list = config.ManagerRolesArray.ToList();
                if (list.Contains(role.Id))
                    list.Remove(role.Id);

                config.ManagerRolesArray = list.ToArray();
                return config;
            });

            await ReplyTextAsync($"已成功移除角色组 (rol){role.Id}(rol) 的管理问答权限", true);
        }

        [Command("list")]
        [Summary("列出所有问题")]
        [GuildManagerRequire]
        public async Task GetQuestions() {
            var result = await _autoFAQService.GetQuestionsAsync(Context.Guild.Id);
            await ReplyCardsAsync(new QuestionListCardMessage(result.ToArray(), Context.Guild).Build(), true);
        }

        [Command("add")]
        [Summary("添加问题")]
        [GuildManagerRequire]
        public async Task AddQuestion(string regex, string answer) {
            var question = await _autoFAQService.AddQuestionAsync(regex, answer, Context.Guild.Id);
            await ReplyTextAsync($"已添加问题 (id:{question.Id}, regex:{question.Regex}, answer:{question.Answer})", true);
        }

        [Command("remove")]
        [Summary("移除问题")]
        [GuildManagerRequire]
        public async Task RemoveQuestion(long id) {
            try {
                await _autoFAQService.RemoveQuestionAsync(id);
                await ReplyTextAsync($"已移除问题 {id}", true);
            } catch (ArgumentException) {
                await ReplyTextAsync($"不存在问题 {id}", true);
            }
        }
    }
}
