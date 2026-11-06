using MicroBlog.Api.DTOs;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Services;

// Database calls are asynchronous, so every method now returns a Task
public interface IPostService
{
    Task<IEnumerable<PostResponseDto>> GetAllPostsAsync(int? currentUserId = null);
    Task<PostResponseDto?> GetPostByIdAsync(int id, int? currentUserId = null);
    Task<Post> CreatePostAsync(Post newPost);
    Task<IEnumerable<PostResponseDto>> GetPostsByUserAsync(int userId, int? currentUserId = null);
    Task<IEnumerable<FeedPostResponseDto>> GetFeedAsync(int page, int pageSize, int? currentUserId);
    Task<IEnumerable<FeedPostResponseDto>> GetFollowingFeedAsync(int page, int pageSize, int currentUserId);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<PostResponseDto>> SearchPostsAsync(string term, int? currentUserId = null);
}
