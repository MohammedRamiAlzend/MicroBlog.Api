using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.Models;
using MicroBlog.Api.Services;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    // Constructor Injection: the DI container passes in the registered service
    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    // GET: api/posts
    [HttpGet]
    public ActionResult<IEnumerable<Post>> GetAll()
    {
        var posts = _postService.GetAllPosts();
        return Ok(posts);
    }

    // GET: api/posts/{id}
    [HttpGet("{id:int}")]
    public ActionResult<Post> GetById(int id)
    {
        var post = _postService.GetPostById(id);

        if (post == null)
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        return Ok(post);
    }

    // POST: api/posts
    [HttpPost]
    public ActionResult<Post> Create([FromBody] Post newPost)
    {
        // The controller only validates the request; the service owns the business rules
        if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Content))
        {
            return BadRequest(new { message = "عنوان المنشور ومحتواه مطلوبان ولا يمكن أن يكونا فارغين." });
        }

        var createdPost = _postService.CreatePost(newPost);
        return CreatedAtAction(nameof(GetById), new { id = createdPost.Id }, createdPost);
    }

    // GET: api/posts/count
    [HttpGet("count")]
    public ActionResult GetTotalCount()
    {
        var count = _postService.GetTotalCount();
        return Ok(new { total = count, totalPosts = count });
    }
}
