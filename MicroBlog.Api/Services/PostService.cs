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
            .Include(p => p.User)
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Post?> GetPostByIdAsync(int id)
    {
        // FindAsync cannot Include related data, so use a normal query
        return await _context.Posts
            .Include(p => p.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post> CreatePostAsync(Post newPost)
    {
        newPost.CreatedAt = DateTime.UtcNow;

        await _context.Posts.AddAsync(newPost);
        await _context.SaveChangesAsync();
        // Load the author so the caller can build the response (AuthorUsername)
        await _context.Entry(newPost).Reference(p => p.User).LoadAsync();
        return newPost;
    }

    public async Task<IEnumerable<Post>> GetPostsByUserAsync(int userId)
    {
        return await _context.Posts
            .Include(p => p.User)
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
            .Include(p => p.User)
            .AsNoTracking()
            .Where(p => p.Content.ToLower().Contains(term.ToLower()))
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
