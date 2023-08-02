using AutoFAQ.Entity.Models;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Reflection;

namespace AutoFAQ.Database.DbContexts {

    public class DefaultDbContext : DbContext {
        public DefaultDbContext(DbContextOptions<DefaultDbContext> options) : base(options) {
            this.SavingChanges += DefaultDbContext_SavingChanges;
        }

        private void DefaultDbContext_SavingChanges(object? sender, SavingChangesEventArgs e) {
            var enrties = ChangeTracker.Entries().ToList();
            foreach (var entry in enrties) {
                switch (entry.State) {
                    case EntityState.Modified:
                        Entry(entry.Entity).Property(nameof(BaseEntity.UpdatedTime)).CurrentValue = DateTimeOffset.Now;
                        break;
                    case EntityState.Added:
                        Entry(entry.Entity).Property(nameof(BaseEntity.CreatedTime)).CurrentValue = DateTimeOffset.Now;
                        Entry(entry.Entity).Property(nameof(BaseEntity.UpdatedTime)).CurrentValue = DateTimeOffset.Now;
                        break;
                }
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);
            var assembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name.EndsWith("Entity"));
            foreach (Type type in assembly.GetExportedTypes()) {
                if (type.IsClass && type != typeof(BaseEntity) && typeof(BaseEntity).IsAssignableFrom(type)) {

                    var method = type.GetMethod("Configure");

                    if (method != null) {
                        method.Invoke(null, new object[] { modelBuilder });
                    }
                }
            }
        }
    }
}