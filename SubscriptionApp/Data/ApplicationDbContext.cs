using Microsoft.EntityFrameworkCore;
using SubscriptionApp.Models;

namespace SubscriptionApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }

        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            //modelBuilder.AddMassTransitMigrations();
        }
    }
}
