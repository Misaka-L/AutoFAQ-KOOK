using AutoFAQ.Entity.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoFAQ.Entity.Entity.FAQ {
    public class QuestionEntity : BaseEntity {
        public string Regex { get; set; }
        public string Answer { get; set; }
        public ulong GuildId { get; set; }

        public static void Configure(ModelBuilder modelBuilder) {
            modelBuilder.Entity<QuestionEntity>();
        }
    }
}
