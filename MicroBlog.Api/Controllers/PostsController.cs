using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.Models;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    // Static list acting as in-memory storage (replaced by a real database in Session 10)
    private static readonly List<Post> Posts = new()
    {
        new Post
        {
            Id = 1,
            Title = "البدء مع .NET 10",
            Content = "شرح مفصل لأحدث ميزات الإطار وتحديثات الأداء.",
            Author = "أحمد علي",
            Likes = 15,
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        },
        new Post
        {
            Id = 2,
            Title = "دليل ASP.NET Core Controllers",
            Content = "كيفية بناء نقاط توصيل سريعة ومنظمة باستخدام Web API.",
            Author = "سارة محمود",
            Likes = 42,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        }
    };

    // GET: api/posts
    [HttpGet]
    public ActionResult<IEnumerable<Post>> GetAllPosts()
    {
        return Ok(Posts);
    }

    // GET: api/posts/{id}
    [HttpGet("{id:int}")]
    public ActionResult<Post> GetPostById(int id)
    {
        var post = Posts.FirstOrDefault(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { Message = $"المنشور ذو الرقم {id} غير موجود." });
        }

        return Ok(post);
    }

    // POST: api/posts
    [HttpPost]
    public IActionResult CreatePost([FromBody] Post newPost)
    {
        if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Content))
        {
            return BadRequest(new { Message = "عنوان المنشور ومحتواه مطلوبان ولا يمكن أن يكونا فارغين." });
        }

        newPost.Id = Posts.Count > 0 ? Posts.Max(p => p.Id) + 1 : 1;

        if (string.IsNullOrWhiteSpace(newPost.Author))
        {
            newPost.Author = "Anonymous";
        }

        newPost.CreatedAt = DateTime.UtcNow;
        Posts.Add(newPost);

        return CreatedAtAction(nameof(GetPostById), new { id = newPost.Id }, newPost);
    }

    // GET: api/posts/search?term=xxx
    [HttpGet("search")]
    public IActionResult SearchPosts([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest(new { Message = "يرجى إدخال كلمة للبحث." });
        }

        var results = Posts
            .Where(p => p.Title.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                        p.Content.Contains(term, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (!results.Any())
        {
            return NotFound(new { Message = $"لم يتم العثور على أي منشورات تحتوي على الكلمة: '{term}'." });
        }

        return Ok(results);
    }
}
