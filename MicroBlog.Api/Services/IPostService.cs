using MicroBlog.Api.Models;

namespace MicroBlog.Api.Services;

// The contract: WHAT the service offers, not HOW it does it
public interface IPostService
{
    IEnumerable<Post> GetAllPosts();
    Post? GetPostById(int id);
    Post CreatePost(Post newPost);
    int GetTotalCount();
    IEnumerable<Post> SearchPosts(string term);
    bool DeletePost(int id);
}
