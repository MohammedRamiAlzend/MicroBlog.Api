using MicroBlog.Api.Models;

namespace MicroBlog.Api.Services;

public class PostService : IPostService
{
    // In-memory storage moved here from PostsController
    private static readonly List<Post> _posts = new()
    {
        new Post
        {
            Id = 1,
            Title = "الإعلان عن .NET 10",
            Content = "ميزات أداء جديدة ومذهلة في بناء تطبيقات الويب.",
            Author = "أحمد علي",
            Likes = 15,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        },
        new Post
        {
            Id = 2,
            Title = "دليل Dependency Injection",
            Content = "كيفية بناء معمارية نظيفة قابلة للصيانة والاختبار عبر فصل المسؤوليات.",
            Author = "سارة محمود",
            Likes = 42,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        }
    };

    public IEnumerable<Post> GetAllPosts()
    {
        return _posts;
    }

    public Post? GetPostById(int id)
    {
        return _posts.FirstOrDefault(p => p.Id == id);
    }

    public Post CreatePost(Post newPost)
    {
        newPost.Id = _posts.Count > 0 ? _posts.Max(p => p.Id) + 1 : 1;

        if (string.IsNullOrWhiteSpace(newPost.Author))
        {
            newPost.Author = "Anonymous";
        }

        newPost.CreatedAt = DateTime.UtcNow;

        _posts.Add(newPost);
        return newPost;
    }

    public int GetTotalCount()
    {
        return _posts.Count;
    }

    public IEnumerable<Post> SearchPosts(string term)
    {
        return _posts
            .Where(p => p.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        p.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public bool DeletePost(int id)
    {
        var post = _posts.FirstOrDefault(p => p.Id == id);

        if (post == null)
        {
            return false;
        }

        _posts.Remove(post);
        return true;
    }
}
