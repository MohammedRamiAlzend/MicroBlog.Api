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
}
