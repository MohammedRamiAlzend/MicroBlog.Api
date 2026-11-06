using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    // For public endpoints: the id of the caller if a valid token was sent, otherwise null.
    // No [Authorize] here, but the JWT middleware still fills User when a good token is present.
    private int? GetOptionalUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;
    }

    // GET: api/posts
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetAll()
    {
        var posts = await _postService.GetAllPostsAsync(GetOptionalUserId());
        return Ok(posts);
    }

    // GET: api/posts/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostResponseDto>> GetById(int id)
    {
        var post = await _postService.GetPostByIdAsync(id, GetOptionalUserId());

        if (post == null)
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        return Ok(post);
    }

    // GET: api/posts/user/{userId}  (public: the posts of one user, newest first)
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<IEnumerable<PostResponseDto>>> GetByUser(int userId)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);

        if (!userExists)
        {
            return NotFound(new { message = $"المستخدم برقم {userId} غير موجود." });
        }

        var posts = await _postService.GetPostsByUserAsync(userId, GetOptionalUserId());
        return Ok(posts);
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

        var myPosts = await _postService.GetPostsByUserAsync(currentUserId, currentUserId);
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

    // PUT: api/posts/{id}  (protected: only the owner may edit)
    // Order of checks: 401 (no valid token) -> 404 (post missing) -> 403 (not the owner)
    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePostRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية المستخدم." });
        }

        // Tracked query (no AsNoTracking) because we are going to modify and save it
        var post = await _context.Posts
            .Include(p => p.User)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post == null)
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        // The user is authenticated, the post exists, but it belongs to someone else => 403, not 401
        if (post.UserId != currentUserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "ليس لديك صلاحية لتعديل هذا المنشور." });
        }

        // Nothing changed: skip the database write and keep UpdatedAt untouched
        if (post.Content == request.Content)
        {
            return Ok(await ToDtoWithLikesAsync(post, currentUserId));
        }

        post.Content = request.Content;
        post.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(await ToDtoWithLikesAsync(post, currentUserId));
    }

    // POST: api/posts/{id}/like  (protected)
    [Authorize]
    [HttpPost("{id:int}/like")]
    public async Task<IActionResult> LikePost(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية المستخدم." });
        }

        if (!await _context.Posts.AnyAsync(p => p.Id == id))
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        var alreadyLiked = await _context.Likes
            .AnyAsync(l => l.PostId == id && l.UserId == currentUserId);

        if (alreadyLiked)
        {
            return BadRequest(new { message = "لقد قمت بالإعجاب بهذا المنشور مسبقاً." });
        }

        _context.Likes.Add(new Like { PostId = id, UserId = currentUserId, CreatedAt = DateTime.UtcNow });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two identical requests at once, or the account was deleted meanwhile:
            // the composite key / foreign key in the database is the last line of defense
            return BadRequest(new { message = "تعذر تسجيل الإعجاب. ربما أعجبت بهذا المنشور مسبقاً." });
        }

        return Ok(await BuildLikeResponseAsync(id, true, "تم الإعجاب بالمنشور بنجاح."));
    }

    // DELETE: api/posts/{id}/like  (protected)
    [Authorize]
    [HttpDelete("{id:int}/like")]
    public async Task<IActionResult> UnlikePost(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية المستخدم." });
        }

        if (!await _context.Posts.AnyAsync(p => p.Id == id))
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        var like = await _context.Likes
            .FirstOrDefaultAsync(l => l.PostId == id && l.UserId == currentUserId);

        if (like == null)
        {
            return BadRequest(new { message = "لم تقم بالإعجاب بهذا المنشور من قبل." });
        }

        _context.Likes.Remove(like);
        await _context.SaveChangesAsync();

        return Ok(await BuildLikeResponseAsync(id, false, "تم إلغاء الإعجاب بنجاح."));
    }

    private async Task<LikeResponseDto> BuildLikeResponseAsync(int postId, bool isLiked, string message)
    {
        return new LikeResponseDto
        {
            PostId = postId,
            LikesCount = await _context.Likes.CountAsync(l => l.PostId == postId),
            IsLiked = isLiked,
            Message = message
        };
    }

    // GET: api/posts/search?term=xxx
    [HttpGet("search")]
    public async Task<IActionResult> SearchPosts([FromQuery] string? term)
    {
        if (string.IsNullOrWhiteSpace(term))
        {
            return BadRequest(new { message = "يرجى إدخال كلمة للبحث." });
        }

        var results = await _postService.SearchPostsAsync(term, GetOptionalUserId());

        if (!results.Any())
        {
            return NotFound(new { message = $"لم يتم العثور على أي منشورات تحتوي على الكلمة: '{term}'." });
        }

        return Ok(results);
    }

    // DELETE: api/posts/{id}  (protected)
    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية المستخدم." });
        }

        var post = await _context.Posts.FirstOrDefaultAsync(p => p.Id == id);

        // Existence first (404), then ownership (403)
        if (post == null)
        {
            return NotFound(new { message = $"المنشور رقم {id} غير موجود." });
        }

        if (post.UserId != currentUserId)
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "ليس لديك صلاحية لحذف هذا المنشور." });
        }

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();

        // REST standard for a successful delete: 204 with an empty body
        return NoContent();
    }

    // Same as ToDto, plus the like info (a tracked entity has no Likes loaded)
    private async Task<PostResponseDto> ToDtoWithLikesAsync(Post post, int viewerId)
    {
        var dto = ToDto(post);
        dto.LikesCount = await _context.Likes.CountAsync(l => l.PostId == post.Id);
        dto.CommentsCount = await _context.Comments.CountAsync(c => c.PostId == post.Id);
        dto.IsLikedByCurrentUser = await _context.Likes.AnyAsync(l => l.PostId == post.Id && l.UserId == viewerId);
        return dto;
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
