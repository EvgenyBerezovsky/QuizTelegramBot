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
    public class QuizConfigurations : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
            builder.
                HasKey(q => q.Id);
            builder
                .Property(q => q.Topic)
                .HasColumnType("nvarchar(100)");
            builder
                .HasMany(q => q.Questions)
                .WithOne(qs => qs.Quiz)
                .HasForeignKey(q => q.QuizId);
        }
    }
}
