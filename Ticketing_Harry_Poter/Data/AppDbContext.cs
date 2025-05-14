using System.Configuration;
using Microsoft.EntityFrameworkCore;
using Ticketing_Harry_Poter.Models;

namespace Ticketing_Harry_Poter.Data
{
    public class AppDbContext : DbContext
    {
        // Vos tables
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                // Lit la connexion depuis App.config / Web.config
                var cs = ConfigurationManager
                            .ConnectionStrings["DefaultConnection"]
                            .ConnectionString;
                optionsBuilder.UseSqlServer(cs);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Nomme explicitement les tables
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Ticket>().ToTable("Tickets");
            modelBuilder.Entity<Comment>().ToTable("Comments");

            // Configure la relation Ticket ← Comment
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)        // nécessite ICollection<Comment> Comments dans Ticket
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
