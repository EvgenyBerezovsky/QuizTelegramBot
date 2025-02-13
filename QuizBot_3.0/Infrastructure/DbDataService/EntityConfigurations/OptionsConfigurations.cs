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
    public class OptionsConfigurations : IEntityTypeConfiguration<Options>
    {
        public void Configure(EntityTypeBuilder<Options> builder)
        {
            builder
                .HasKey(o => o.Id);
            builder
                .Property(o => o.Option1)
                .HasColumnType("nvarchar(100)");
            builder
                .Property(o => o.Option2)
                .HasColumnType("nvarchar(100)");
            builder
                .Property(o => o.Option3)
                .HasColumnType("nvarchar(100)");
            builder
                .Property(o => o.Option4)
                .HasColumnType("nvarchar(100)");
        }
    }
}
