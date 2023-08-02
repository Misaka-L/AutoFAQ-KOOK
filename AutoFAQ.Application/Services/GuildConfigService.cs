using AutoFAQ.Database.DbContexts;
using AutoFAQ.Entity.Entity.Manager;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Application.Services {
    public class GuildConfigService {
        private readonly DefaultDbContext _defaultDbContext;
        private readonly DbSet<GuildConfigEntity> _guildConfigEntities;

        public GuildConfigService(DefaultDbContext defaultDbContext) {
            _defaultDbContext = defaultDbContext;
            _guildConfigEntities = _defaultDbContext.Set<GuildConfigEntity>();
        }

        public async ValueTask<GuildConfigEntity> GetConfigAsync(long id) {
            return await _guildConfigEntities.AsNoTracking().Where(config => config.Id == id).FirstAsync();
        }

        public async ValueTask<GuildConfigEntity?> GetConfigByGuildIdAsync(ulong guildId) {
            if (await _guildConfigEntities.AsNoTracking().AnyAsync(config => config.GuildId == guildId))
                return await _guildConfigEntities.Where(config => config.GuildId == guildId).FirstAsync();

            return null;
        }

        public async ValueTask<GuildConfigEntity> UpdateConfigByGuildIdAsync(ulong guildId, Func<GuildConfigEntity, GuildConfigEntity> func) {
            var isConfigExitis = await _guildConfigEntities.AsNoTracking().AnyAsync(config => config.GuildId == guildId);
            GuildConfigEntity guildConfigEntity = new GuildConfigEntity();
            guildConfigEntity.GuildId = guildId;

            if (isConfigExitis) {
                guildConfigEntity = await GetConfigByGuildIdAsync(guildId);
            }

            guildConfigEntity = func(guildConfigEntity);
            if (isConfigExitis) {
                _guildConfigEntities.Update(guildConfigEntity);
            } else {
                await _guildConfigEntities.AddAsync(guildConfigEntity);
            }

            await _defaultDbContext.SaveChangesAsync();
            return await GetConfigAsync(guildConfigEntity.Id);
        }
    }
}
