using Microsoft.EntityFrameworkCore;
using WorkCale.Domain.Entities;

namespace WorkCale.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<ShiftCategory> ShiftCategories => Set<ShiftCategory>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<CalendarShare> CalendarShares => Set<CalendarShare>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<InviteCode> InviteCodes => Set<InviteCode>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Email).IsRequired().HasMaxLength(255);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.DisplayName).IsRequired().HasMaxLength(100);
            e.Property(u => u.PasswordHash).HasMaxLength(500);
            e.Property(u => u.GoogleId).HasMaxLength(100);
            e.HasIndex(u => u.GoogleId).IsUnique().HasFilter("\"GoogleId\" IS NOT NULL");
            e.Property(u => u.AvatarUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<Job>(e =>
        {
            e.HasKey(j => j.Id);
            e.Property(j => j.Name).IsRequired().HasMaxLength(60);
            e.Property(j => j.Color).IsRequired().HasMaxLength(7);
            e.Property(j => j.Icon).HasMaxLength(50);
            e.HasIndex(j => j.UserId);
            // At most one default job per user (partial unique index on Postgres).
            e.HasIndex(j => j.UserId)
                .IsUnique()
                .HasFilter("\"IsDefault\" = TRUE")
                .HasDatabaseName("IX_Jobs_UserId_DefaultUnique");
            e.HasOne(j => j.User)
                .WithMany(u => u.Jobs)
                .HasForeignKey(j => j.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ShiftCategory>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Name).IsRequired().HasMaxLength(50);
            e.Property(c => c.Color).IsRequired().HasMaxLength(7);
            e.Property(c => c.DefaultStartTime).HasMaxLength(5);
            e.Property(c => c.DefaultEndTime).HasMaxLength(5);
            e.Property(c => c.Icon).HasMaxLength(50);
            e.HasIndex(c => c.UserId);
            e.HasIndex(c => new { c.UserId, c.JobId });
            e.HasOne(c => c.User)
                .WithMany(u => u.ShiftCategories)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(c => c.Job)
                .WithMany(j => j.Categories)
                .HasForeignKey(c => c.JobId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Shift>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.UnpaidBreakMinutes).HasDefaultValue(0);
            e.HasIndex(s => new { s.UserId, s.Date });
            e.HasIndex(s => new { s.UserId, s.JobId, s.Date });
            e.HasOne(s => s.User)
                .WithMany(u => u.Shifts)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.Job)
                .WithMany(j => j.Shifts)
                .HasForeignKey(s => s.JobId)
                .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(s => s.Category)
                .WithMany(c => c.Shifts)
                .HasForeignKey(s => s.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CalendarShare>(e =>
        {
            e.HasKey(s => s.Id);
            e.HasIndex(s => new { s.OwnerUserId, s.ViewerUserId }).IsUnique();
            e.HasIndex(s => s.ViewerUserId);
            e.HasOne(s => s.OwnerUser)
                .WithMany(u => u.SharesGranted)
                .HasForeignKey(s => s.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(s => s.ViewerUser)
                .WithMany(u => u.SharesReceived)
                .HasForeignKey(s => s.ViewerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InviteCode>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Code).IsRequired().HasMaxLength(32);
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Note).HasMaxLength(200);
            e.HasOne(c => c.IssuedByUser)
                .WithMany()
                .HasForeignKey(c => c.IssuedByUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).IsRequired().HasMaxLength(500);
            e.HasIndex(r => r.Token).IsUnique();
            e.HasIndex(r => r.UserId);
            e.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
