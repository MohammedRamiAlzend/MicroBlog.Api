using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Follow> Follows => Set<Follow>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(u => u.Id);

            // Unique indexes: the database itself refuses duplicate emails and usernames
            entity.HasIndex(u => u.Username).IsUnique();
            entity.HasIndex(u => u.Email).IsUnique();

            entity.Property(u => u.Username).IsRequired().HasMaxLength(50);
            entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Content).IsRequired().HasMaxLength(280);
            entity.Property(p => p.CreatedAt).IsRequired();

            // Composite index (UserId, CreatedAt): serves "posts of these users, newest first"
            // (following feed, my-posts, user posts). Its first column also covers plain UserId
            // lookups and joins, so it replaces the old single-column IX_Posts_UserId.
            entity.HasIndex(p => new { p.UserId, p.CreatedAt }).HasDatabaseName("IX_Posts_UserId_CreatedAt");

            // One User has many Posts. Cascade: deleting a user deletes their posts,
            // so no orphan rows are left behind
            entity.HasOne(p => p.User)
                  .WithMany(u => u.Posts)
                  .HasForeignKey(p => p.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(entity =>
        {
            // Composite primary key: the database refuses a second like by the same user on the same post
            entity.HasKey(l => new { l.UserId, l.PostId });

            // Deleting a user or a post also deletes the related likes
            entity.HasOne(l => l.User)
                  .WithMany(u => u.Likes)
                  .HasForeignKey(l => l.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(l => l.Post)
                  .WithMany(p => p.Likes)
                  .HasForeignKey(l => l.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            // The PK index starts with UserId, so add one for "all likes of a post" (counts)
            entity.HasIndex(l => l.PostId);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(c => c.Content).IsRequired().HasMaxLength(500);

            // Comments of a post are read by PostId, oldest first
            entity.HasIndex(c => new { c.PostId, c.CreatedAt });

            // Deleting a post deletes its comments
            entity.HasOne(c => c.Post)
                  .WithMany(p => p.Comments)
                  .HasForeignKey(c => c.PostId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Two parents can lead to the same row (User -> Posts -> Comments and User -> Comments).
            // Cascading both ways causes "multiple cascade paths" on some databases, so the user
            // side is Restrict: a user who still has comments cannot be deleted by accident.
            entity.HasOne(c => c.User)
                  .WithMany(u => u.Comments)
                  .HasForeignKey(c => c.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Follow>(entity =>
        {
            // Composite key: the same pair can exist only once (no double follow)
            entity.HasKey(f => new { f.FollowerId, f.FollowingId });

            // Both relationships point to Users. Cascade on both would create a cycle
            // ("multiple cascade paths"), so both are Restrict.
            entity.HasOne(f => f.Follower)
                  .WithMany(u => u.Following)
                  .HasForeignKey(f => f.FollowerId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(f => f.Following)
                  .WithMany(u => u.Followers)
                  .HasForeignKey(f => f.FollowingId)
                  .OnDelete(DeleteBehavior.Restrict);

            // The PK index starts with FollowerId; add one for "who follows X" (followers count)
            entity.HasIndex(f => f.FollowingId);
        });
    }
}
