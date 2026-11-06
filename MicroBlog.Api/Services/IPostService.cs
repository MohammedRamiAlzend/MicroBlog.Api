using MicroBlog.Api.DTOs;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Services;

// Database calls are asynchronous, so every method now returns a Task
public interface IPostService
{
    Task<IEnumerable<PostResponseDto>> GetAllPostsAsync();
    Task<PostResponseDto?> GetPostByIdAsync(int id);
    Task<Post> CreatePostAsync(Post newPost);
    Task<IEnumerable<PostResponseDto>> GetPostsByUserAsync(int userId);
    Task<int> GetTotalCountAsync();
    Task<IEnumerable<PostResponseDto>> SearchPostsAsync(string term);
}
