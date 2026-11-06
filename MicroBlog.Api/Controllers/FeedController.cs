using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.Services;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private const int MaxPageSize = 50;

    private readonly IPostService _postService;

    public FeedController(IPostService postService)
    {
        _postService = postService;
    }

    // GET: api/feed?page=1&pageSize=10  (public, optional authentication)
    // No [Authorize]: visitors see the feed too. If a valid token is sent, the JWT middleware fills
    // User, so we can tell which posts the caller liked.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedPostResponseDto>>> GetGlobalFeed(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var invalid = ValidatePaging(page, ref pageSize);
        if (invalid != null)
        {
            return invalid;
        }

        int? currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;

        var feed = await _postService.GetFeedAsync(page, pageSize, currentUserId);
        return Ok(feed);
    }

    // GET: api/feed/following?page=1&pageSize=10  (protected: needs to know WHO is asking)
    // A user who follows nobody gets 200 and [] - that is a normal state, not an error.
    [Authorize]
    [HttpGet("following")]
    public async Task<ActionResult<IEnumerable<FeedPostResponseDto>>> GetFollowingFeed(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var invalid = ValidatePaging(page, ref pageSize);
        if (invalid != null)
        {
            return invalid;
        }

        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية المستخدم." });
        }

        var feed = await _postService.GetFollowingFeedAsync(page, pageSize, currentUserId);
        return Ok(feed);
    }

    // Without these checks pageSize=-5 returns EVERY post (SQLite LIMIT -5 = no limit)
    // and page=0 silently returns the first page
    private BadRequestObjectResult? ValidatePaging(int page, ref int pageSize)
    {
        if (page < 1)
        {
            return BadRequest(new { message = "رقم الصفحة يجب أن يكون 1 أو أكبر." });
        }

        if (pageSize < 1)
        {
            return BadRequest(new { message = "حجم الصفحة يجب أن يكون 1 أو أكبر." });
        }

        // A client cannot ask for the whole table in one request
        pageSize = Math.Min(pageSize, MaxPageSize);
        return null;
    }
}
