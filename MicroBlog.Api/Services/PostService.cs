using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Data;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Services;

public class PostService : IPostService
{
    private readonly AppDbContext _context;

    // The DI container injects the DbContext, just like it injects the service into the controller
    public PostService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Post>> GetAllPostsAsync()
    {
        return await _context.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        return await _context.Posts.FindAsync(id);
    }

    public async Task<Post> CreatePostAsync(Post newPost)
    {
        if (string.IsNullOrWhiteSpace(newPost.Author))
        {
            newPost.Author = "Anonymous";
        }

        newPost.CreatedAt = DateTime.UtcNow;
        newPost.PublishedAt = DateTime.UtcNow;

        await _context.Posts.AddAsync(newPost);
        await _context.SaveChangesAsync();
        return newPost;
    }

    public async Task<IEnumerable<Post>> GetPostsByUserAsync(int userId)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Posts.CountAsync();
    }

    public async Task<IEnumerable<Post>> SearchPostsAsync(string term)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.Title.ToLower().Contains(term.ToLower()) || p.Content.ToLower().Contains(term.ToLower()))
            .ToListAsync();
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var post = await _context.Posts.FindAsync(id);

        if (post == null)
        {
            return false;
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
}
