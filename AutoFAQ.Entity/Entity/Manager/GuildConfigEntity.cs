using AutoFAQ.Entity.Entity.FAQ;
using AutoFAQ.Entity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace AutoFAQ.Entity.Entity.Manager {
    public class GuildConfigEntity : BaseEntity {
        public ulong GuildId { get; set; }
        public string? ManagerRoles { get; set; }

        [NotMapped]
        public uint[] ManagerRolesArray {
            get {
                if (!string.IsNullOrEmpty(ManagerRoles) && JsonSerializer.Deserialize<uint[]>(ManagerRoles) is uint[] roles)
                    return roles;

                return new uint[0];
            }
            set {
                ManagerRoles = value != null ? JsonSerializer.Serialize(value) : null;
            }
        }
        
        public static void Configure(ModelBuilder modelBuilder) {
            modelBuilder.Entity<GuildConfigEntity>();
        }
    }
}
