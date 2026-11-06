using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.Entities;
using MicroBlog.Api.Services;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;

    public PostsController(IPostService postService)
    {
        _postService = postService;
    }

    // GET: api/posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Post>>> GetAll()
    {
        var posts = await _postService.GetAllPostsAsync();
        return Ok(posts);
    }

    // GET: api/posts/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<Post>> GetById(int id)
    {
        var post = await _postService.GetPostByIdAsync(id);

        if (post == null)
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        return Ok(post);
    }

    // GET: api/posts/count
    [HttpGet("count")]
    public async Task<ActionResult> GetTotalCount()
    {
        var count = await _postService.GetTotalCountAsync();
        return Ok(new { total = count, totalPosts = count });
    }

    // POST: api/posts
    [HttpPost]
    public async Task<ActionResult<Post>> Create([FromBody] Post newPost)
    {
        if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Content))
        {
            return BadRequest(new { message = "عنوان المنشور ومحتواه مطلوبان ولا يمكن أن يكونا فارغين." });
        }

        var createdPost = await _postService.CreatePostAsync(newPost);
        return CreatedAtAction(nameof(GetById), new { id = createdPost.Id }, createdPost);
    }

    // GET: api/posts/search?term=xxx
    [HttpGet("search")]
    public async Task<IActionResult> SearchPosts([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest(new { message = "يرجى إدخال كلمة للبحث." });
        }

        var results = await _postService.SearchPostsAsync(term);

        if (!results.Any())
        {
            return NotFound(new { message = $"لم يتم العثور على أي منشورات تحتوي على الكلمة: '{term}'." });
        }

        return Ok(results);
    }

    // DELETE: api/posts/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _postService.DeletePostAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        return NoContent();
    }
}
