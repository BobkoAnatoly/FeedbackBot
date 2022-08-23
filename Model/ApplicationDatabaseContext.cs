using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Model.Models;

namespace Model
{
    public class ApplicationDatabaseContext : DbContext
    {
        public ApplicationDatabaseContext(DbContextOptions<ApplicationDatabaseContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }

        public DbSet<Professor> Professors { get; set; } = null!;
        public DbSet<Feedback> Feedbacks { get; set; } = null!;

    }
}
