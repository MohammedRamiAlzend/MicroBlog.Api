using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.Data;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.Entities;
using MicroBlog.Api.Services;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _postService;
    private readonly AppDbContext _context;

    public PostsController(IPostService postService, AppDbContext context)
    {
        _postService = postService;
        _context = context;
    }

    // GET: api/posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetAll()
    {
        var posts = await _postService.GetAllPostsAsync();
        return Ok(posts);
    }

    // GET: api/posts/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponseDto>> GetById(int id)
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
        return Ok(myPosts.Select(ToDto));
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
    public async Task<IActionResult> Create([FromBody] CreatePostRequest request)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية الناشر." });
        }

        // A valid token is not enough: the account must still exist in the database
        var author = await _context.Users.FindAsync(currentUserId);

        if (author == null)
        {
            return Unauthorized(new { message = "المستخدم صاحب الحساب غير موجود في النظام." });
        }

        if (author.IsBanned)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "حسابك محظور من إنشاء منشورات جديدة." });
        }

        var post = new Post
        {
            Content = request.Content,
            UserId = currentUserId
        };

        var createdPost = await _postService.CreatePostAsync(post);
        var response = ToDto(createdPost);
        return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
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

        return Ok(results.Select(ToDto));
    }

    // DELETE: api/posts/{id}  (protected)
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(new { message = "المستخدم غير موثق." });
        }

        var deleted = await _postService.DeletePostAsync(id);

        if (!deleted)
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        // Who may delete which post (ownership) is added in a later session
        return Ok(new
        {
            Message = "تم حذف المنشور بنجاح",
            PostId = id,
            DeletedBy = currentUserId,
            DeletedAt = DateTime.UtcNow
        });
    }

    // Entity -> DTO: flat shape without the User -> Posts cycle
    private static PostResponseDto ToDto(Post post)
    {
        return new PostResponseDto
        {
            Id = post.Id,
            Content = post.Content,
            CreatedAt = post.CreatedAt,
            UpdatedAt = post.UpdatedAt,
            UserId = post.UserId,
            AuthorUsername = post.User?.Username ?? string.Empty
        };
    }
}
