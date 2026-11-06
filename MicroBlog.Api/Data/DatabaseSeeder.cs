using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Data;

// Demo data for the final presentation: 3 users, follows, posts, comments and likes.
// Idempotent: does nothing when the database already has users.
public static class DatabaseSeeder
{
    public const string DemoPassword = "Password123!";

    public static async Task SeedAsync(AppDbContext context)
    {
        // Apply pending migrations (NOT EnsureCreated: it would skip the migrations history)
        await context.Database.MigrateAsync();

        if (await context.Users.AnyAsync())
        {
            return;
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(DemoPassword);

        var ahmad = new User { Username = "ahmad_dev", Email = "ahmad@tamkeen.local", PasswordHash = passwordHash,
            Bio = "Full-stack developer passionate about .NET 10 and React.", CreatedAt = DateTime.UtcNow.AddMonths(-2) };
        var sarah = new User { Username = "sarah_arch", Email = "sarah@tamkeen.local", PasswordHash = passwordHash,
            Bio = "Cloud and software architecture enthusiast.", CreatedAt = DateTime.UtcNow.AddMonths(-2) };
        var omar = new User { Username = "omar_coder", Email = "omar@tamkeen.local", PasswordHash = passwordHash,
            Bio = "Building scalable REST APIs with Entity Framework Core.", CreatedAt = DateTime.UtcNow.AddMonths(-1) };

        context.Users.AddRange(ahmad, sarah, omar);
        await context.SaveChangesAsync(); // ids are generated here

        context.Follows.AddRange(
            new Follow { FollowerId = ahmad.Id, FollowingId = sarah.Id, CreatedAt = DateTime.UtcNow.AddDays(-20) },
            new Follow { FollowerId = ahmad.Id, FollowingId = omar.Id, CreatedAt = DateTime.UtcNow.AddDays(-15) },
            new Follow { FollowerId = sarah.Id, FollowingId = ahmad.Id, CreatedAt = DateTime.UtcNow.AddDays(-18) },
            new Follow { FollowerId = omar.Id, FollowingId = ahmad.Id, CreatedAt = DateTime.UtcNow.AddDays(-10) });

        var post1 = new Post { UserId = ahmad.Id, CreatedAt = DateTime.UtcNow.AddDays(-10),
            Content = "Hello Tamkeen Full-Stack MicroBlog! Excited to showcase our .NET 10 Web API." };
        var post2 = new Post { UserId = sarah.Id, CreatedAt = DateTime.UtcNow.AddDays(-8),
            Content = "Clean Architecture and DTO Projections make queries lightning fast in EF Core." };
        var post3 = new Post { UserId = omar.Id, CreatedAt = DateTime.UtcNow.AddDays(-5),
            Content = "Self-referencing relationships in SQL allow elegant follow graphs with simple schemas." };
        var post4 = new Post { UserId = ahmad.Id, CreatedAt = DateTime.UtcNow.AddDays(-2),
            Content = "Offset-based pagination with defensive limits prevents DoS attacks on server memory." };

        context.Posts.AddRange(post1, post2, post3, post4);
        await context.SaveChangesAsync();

        context.Comments.AddRange(
            new Comment { PostId = post1.Id, UserId = sarah.Id, Content = "Great project structure Ahmad!", CreatedAt = DateTime.UtcNow.AddDays(-9) },
            new Comment { PostId = post1.Id, UserId = omar.Id, Content = "Looking forward to the graduation demo.", CreatedAt = DateTime.UtcNow.AddDays(-9) },
            new Comment { PostId = post2.Id, UserId = ahmad.Id, Content = "Totally agree, AsNoTracking is essential.", CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new Comment { PostId = post4.Id, UserId = sarah.Id, Content = "Defensive programming is key for production.", CreatedAt = DateTime.UtcNow.AddDays(-1) });

        context.Likes.AddRange(
            new Like { PostId = post1.Id, UserId = sarah.Id, CreatedAt = DateTime.UtcNow.AddDays(-9) },
            new Like { PostId = post1.Id, UserId = omar.Id, CreatedAt = DateTime.UtcNow.AddDays(-8) },
            new Like { PostId = post2.Id, UserId = ahmad.Id, CreatedAt = DateTime.UtcNow.AddDays(-7) },
            new Like { PostId = post3.Id, UserId = ahmad.Id, CreatedAt = DateTime.UtcNow.AddDays(-4) },
            new Like { PostId = post4.Id, UserId = sarah.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new Like { PostId = post4.Id, UserId = omar.Id, CreatedAt = DateTime.UtcNow.AddDays(-1) });

        await context.SaveChangesAsync();
    }
}
