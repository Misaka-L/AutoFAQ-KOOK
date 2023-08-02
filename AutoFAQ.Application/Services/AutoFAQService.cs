using AutoFAQ.Database.DbContexts;
using AutoFAQ.Entity.Entity.FAQ;
using AutoFAQ.Entity.Entity.Manager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Application.Services {
    public class AutoFAQService {
        private readonly DefaultDbContext _defaultDbContext;
        private readonly DbSet<QuestionEntity> _questionEntities;
        private readonly ILogger<AutoFAQService> _logger;

        public AutoFAQService(DefaultDbContext defaultDbContext, ILogger<AutoFAQService> logger) {
            _defaultDbContext = defaultDbContext;
            _questionEntities = _defaultDbContext.Set<QuestionEntity>();
            _logger = logger;
        }

        public async ValueTask<QuestionEntity> AddQuestionAsync(string regex, string answer, ulong guildId) {
            var entity = await _questionEntities.AddAsync(new QuestionEntity {
                Regex = regex,
                Answer = answer,
                GuildId = guildId
            });

            await _defaultDbContext.SaveChangesAsync();
            _logger.LogInformation("服务器 {guild_id} 添加问题 ID:{id}", guildId, entity.Entity.Id);
            return await GetQuestionAsync(entity.Entity.Id);
        }

        public async Task RemoveQuestionAsync(long id) {
            if (!await _questionEntities.AsNoTracking().AnyAsync(question => question.Id == id))
                throw new ArgumentException("id 不存在");

            _logger.LogInformation("删除问题 {id}", id);
            _questionEntities.Remove(await _questionEntities.Where(question => question.Id == id).FirstAsync());
            await _defaultDbContext.SaveChangesAsync();
        }

        public async ValueTask<List<QuestionEntity>> GetQuestionsAsync(ulong guildId) {
            return await _questionEntities.AsNoTracking().Where(question => question.GuildId == guildId).ToListAsync();
        }

        public async ValueTask<QuestionEntity> GetQuestionAsync(long id) {
            return await _questionEntities.AsNoTracking().Where(question => question.Id == id).FirstAsync();
        }
    }
}
