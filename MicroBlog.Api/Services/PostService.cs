using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using MicroBlog.Api.Data;
using MicroBlog.Api.DTOs;
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

    // DTO projection: EF Core translates this into a SELECT of only these columns
    // (plus a JOIN for the author name). No Post/User entities are loaded and no cycle is possible.
    // The likes counter and the "did I like it?" flag are computed inside the same query
    // (no extra round trip per post). Anonymous callers get IsLikedByCurrentUser = false.
    private static Expression<Func<Post, PostResponseDto>> ToResponse(int? currentUserId)
    {
        int viewerId = currentUserId ?? 0; // 0 never matches a real user id

        return p => new PostResponseDto
        {
            Id = p.Id,
            Content = p.Content,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt,
            UserId = p.UserId,
            AuthorUsername = p.User.Username,
            LikesCount = p.Likes.Count,
            CommentsCount = p.Comments.Count,
            IsLikedByCurrentUser = p.Likes.Any(l => l.UserId == viewerId)
        };
    }

    public async Task<IEnumerable<PostResponseDto>> GetAllPostsAsync(int? currentUserId = null)
    {
        return await _context.Posts
            .AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Select(ToResponse(currentUserId))
            .ToListAsync();
    }

    public async Task<PostResponseDto?> GetPostByIdAsync(int id, int? currentUserId = null)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(ToResponse(currentUserId))
            .FirstOrDefaultAsync();
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

    public async Task<IEnumerable<PostResponseDto>> GetPostsByUserAsync(int userId, int? currentUserId = null)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Select(ToResponse(currentUserId))
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Posts.CountAsync();
    }

    public async Task<IEnumerable<PostResponseDto>> SearchPostsAsync(string term, int? currentUserId = null)
    {
        return await _context.Posts
            .AsNoTracking()
            .Where(p => p.Content.ToLower().Contains(term.ToLower()))
            .OrderByDescending(p => p.CreatedAt)
            .Select(ToResponse(currentUserId))
            .ToListAsync();
    }
}
