using Microsoft.EntityFrameworkCore;
using CheckLiveBot.Models;

namespace CheckLiveBot.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<TrackedUid> TrackedUids { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=checkLiveBot.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TelegramUserId).IsUnique();
                entity.Property(e => e.ExpiryDate).IsRequired();
            });

            modelBuilder.Entity<TrackedUid>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.UserId, e.Uid }).IsUnique();
                entity.Property(e => e.Uid).IsRequired().HasMaxLength(20);
                entity.HasOne(e => e.User)
                      .WithMany(u => u.TrackedUids)
                      .HasForeignKey(e => e.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}