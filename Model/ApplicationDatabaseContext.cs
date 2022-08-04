using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Model.Models;

namespace Model
{
    public class ApplicationDatabaseContext : DbContext
    {
        public ApplicationDatabaseContext()
        {
            Database.EnsureCreated();
        }

        public DbSet<Professor> Professors { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlServer("Server=localhost;Database=FeedbackBotDB;Trusted_Connection=True;TrustServerCertificate=True;");
        }
    }
}
