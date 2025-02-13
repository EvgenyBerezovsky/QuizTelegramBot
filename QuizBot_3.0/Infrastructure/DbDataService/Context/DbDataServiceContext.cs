using Microsoft.EntityFrameworkCore;
using QuizBot_3._0.Infrastructure.DbDataService.EntityConfigurations;
using QuizBot_3._0.Infrastructure.DbDataService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuizBot_3._0.Infrastructure.DbDataService.Context
{
    public class DbDataServiceContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Quiz> Quizzes { get; set; }
        public DbSet<Score> Scores { get; set; }
        public DbSet<QuestionItem> Questions { get; set; }
        public DbSet<Options> Options { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=botData.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfigurations());
            modelBuilder.ApplyConfiguration(new ScoreConfigurations());
            modelBuilder.ApplyConfiguration(new OptionsConfigurations());
            modelBuilder.ApplyConfiguration(new QuizConfigurations());
            modelBuilder.ApplyConfiguration(new QuestionItemConfigurations());
        }
    }
}
