using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QuizBot_3._0.Infrastructure.DbDataService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Infrastructure.DbDataService.EntityConfigurations
{
    public class ScoreConfigurations : IEntityTypeConfiguration<Score>
    {
        public void Configure(EntityTypeBuilder<Score> builder)
        {
            builder
                .HasKey(s => s.Id);
            builder
                .Property(s => s.Topic)
                .HasColumnType("nvarchar(100)");
        }
    }
}
