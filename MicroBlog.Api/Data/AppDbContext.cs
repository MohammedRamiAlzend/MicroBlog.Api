using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Data;

public class AppDbContext : DbContext
{
    // The options (provider + connection string) are passed to the base class
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // Each DbSet represents a table
    public DbSet<User> Users => Set<User>();
    public DbSet<Post> Posts => Set<Post>();
}
