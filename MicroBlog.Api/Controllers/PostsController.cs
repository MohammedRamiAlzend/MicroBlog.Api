using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.DTOs;
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

    // GET: api/posts/my-posts  (protected: only the caller's own posts)
    [Authorize]
    [HttpGet("my-posts")]
    public async Task<IActionResult> GetMyPosts()
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح." });
        }

        var myPosts = await _postService.GetPostsByUserAsync(currentUserId);
        return Ok(myPosts);
    }

    // GET: api/posts/count
    [HttpGet("count")]
    public async Task<ActionResult> GetTotalCount()
    {
        var count = await _postService.GetTotalCountAsync();
        return Ok(new { total = count, totalPosts = count });
    }

    // POST: api/posts  (protected)
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePostDto dto)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية الناشر." });
        }

        var post = new Post
        {
            Title = dto.Title,
            Content = dto.Content,
            Author = User.FindFirstValue(ClaimTypes.Name) ?? "AuthenticatedUser",
            UserId = currentUserId
        };

        var createdPost = await _postService.CreatePostAsync(post);
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
