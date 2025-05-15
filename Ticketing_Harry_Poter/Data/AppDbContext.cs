using System;
using System.Configuration;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Ticketing_Harry_Poter.Models;

namespace Ticketing_Harry_Poter.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Comment> Comments { get; set; }

        public AppDbContext() { }
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
                return;

            if (File.Exists("ticketing.db"))
                optionsBuilder.UseSqlite("Data Source=ticketing.db");
            else
            {
                var cs = ConfigurationManager
                            .ConnectionStrings["DefaultConnection"]
                            .ConnectionString;
                optionsBuilder.UseSqlServer(cs);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Ticket>().ToTable("Tickets");
            modelBuilder.Entity<Comment>().ToTable("Comments");

            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Ticket)
                .WithMany(t => t.Comments)
                .HasForeignKey(c => c.TicketId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(modelBuilder);
        }
    }
}
