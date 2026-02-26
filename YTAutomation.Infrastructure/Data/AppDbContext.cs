using Microsoft.EntityFrameworkCore;
using YTAutomation.Core.Entities;

namespace YTAutomation.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<VideoJob> VideoJobs => Set<VideoJob>();
    public DbSet<ScheduledPost> ScheduledPosts => Set<ScheduledPost>();
    public DbSet<MarketInsight> MarketInsights => Set<MarketInsight>();
    public DbSet<VideoAnalytics> VideoAnalytics => Set<VideoAnalytics>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Soft delete global filter
        modelBuilder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
        modelBuilder.Entity<VideoJob>().HasQueryFilter(v => !v.IsDeleted);
        modelBuilder.Entity<ScheduledPost>().HasQueryFilter(s => !s.IsDeleted);

        // User
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Email).HasMaxLength(256).IsRequired();
            e.Property(u => u.Username).HasMaxLength(100).IsRequired();
            e.Property(u => u.Role).HasMaxLength(50).HasDefaultValue("User");
        });

        // VideoJob
        modelBuilder.Entity<VideoJob>(e =>
        {
            e.HasOne(v => v.User).WithMany(u => u.VideoJobs).HasForeignKey(v => v.UserId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(v => v.Analytics).WithOne(a => a.VideoJob).HasForeignKey<VideoAnalytics>(a => a.VideoJobId);
            e.Property(v => v.Topic).HasMaxLength(500).IsRequired();
        });

        // ScheduledPost
        modelBuilder.Entity<ScheduledPost>(e =>
        {
            e.HasOne(s => s.User).WithMany(u => u.ScheduledPosts).HasForeignKey(s => s.UserId).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(s => s.VideoJob).WithMany().HasForeignKey(s => s.VideoJobId).OnDelete(DeleteBehavior.Cascade);
        });

        // MarketInsight
        modelBuilder.Entity<MarketInsight>(e =>
        {
            e.Property(m => m.NicheCategory).HasMaxLength(200).IsRequired();
            e.Property(m => m.AISource).HasMaxLength(50);
        });
    }
}
