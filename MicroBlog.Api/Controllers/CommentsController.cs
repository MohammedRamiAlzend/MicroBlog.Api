using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Data;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Controllers;

// Nested route: a comment only makes sense inside its post => /api/posts/{postId}/comments
[ApiController]
[Route("api/posts/{postId:int}/comments")]
public class CommentsController : ControllerBase
{
    private readonly AppDbContext _context;

    public CommentsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/posts/{postId}/comments  (public, oldest first)
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CommentResponseDto>>> GetComments(int postId)
    {
        if (!await _context.Posts.AnyAsync(p => p.Id == postId))
        {
            return NotFound(new { message = $"المنشور رقم {postId} غير موجود." });
        }

        var comments = await _context.Comments
            .AsNoTracking()
            .Where(c => c.PostId == postId)
            .OrderBy(c => c.CreatedAt)
            .Select(c => new CommentResponseDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                PostId = c.PostId,
                UserId = c.UserId,
                AuthorUsername = c.User.Username
            })
            .ToListAsync();

        return Ok(comments);
    }

    // GET: api/posts/{postId}/comments/{commentId}  (target of the Location header after POST)
    [HttpGet("{commentId:int}", Name = "GetCommentById")]
    public async Task<ActionResult<CommentResponseDto>> GetCommentById(int postId, int commentId)
    {
        var comment = await _context.Comments
            .AsNoTracking()
            .Where(c => c.PostId == postId && c.Id == commentId)
            .Select(c => new CommentResponseDto
            {
                Id = c.Id,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                PostId = c.PostId,
                UserId = c.UserId,
                AuthorUsername = c.User.Username
            })
            .FirstOrDefaultAsync();

        if (comment == null)
        {
            return NotFound(new { message = $"التعليق رقم {commentId} غير موجود على هذا المنشور." });
        }

        return Ok(comment);
    }

    // POST: api/posts/{postId}/comments  (protected)
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateComment(int postId, [FromBody] CreateCommentRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية المستخدم." });
        }

        // Check the parent first: otherwise the database answers with "FOREIGN KEY constraint failed" (500)
        if (!await _context.Posts.AnyAsync(p => p.Id == postId))
        {
            return NotFound(new { message = $"المنشور رقم {postId} غير موجود." });
        }

        var author = await _context.Users.FindAsync(currentUserId);

        if (author == null)
        {
            return Unauthorized(new { message = "المستخدم صاحب الحساب غير موجود في النظام." });
        }

        var comment = new Comment
        {
            Content = request.Content,
            PostId = postId,
            UserId = currentUserId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        var response = new CommentResponseDto
        {
            Id = comment.Id,
            Content = comment.Content,
            CreatedAt = comment.CreatedAt,
            PostId = comment.PostId,
            UserId = comment.UserId,
            AuthorUsername = author.Username
        };

        return CreatedAtRoute("GetCommentById", new { postId, commentId = comment.Id }, response);
    }
}
