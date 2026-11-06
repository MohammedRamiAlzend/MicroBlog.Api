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

            // Index on the foreign key: faster joins and filtering by user
            entity.HasIndex(p => p.UserId).HasDatabaseName("IX_Posts_UserId");

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
    }
}
