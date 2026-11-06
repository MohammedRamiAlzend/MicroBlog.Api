using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.DTOs.Common;
using MicroBlog.Api.Services;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Produces("application/json")]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private readonly IPostService _postService;

    public FeedController(IPostService postService)
    {
        _postService = postService;
    }

    // GET: api/feed?page=1&pageSize=10  (public, optional authentication)
    // No [Authorize]: visitors see the feed too. If a valid token is sent, the JWT middleware fills
    // User, so we can tell which posts the caller liked.
    // PaginationParams clamps page / pageSize, so no manual validation is needed here.
    [HttpGet]
    public async Task<ActionResult<PagedResult<FeedPostResponseDto>>> GetGlobalFeed(
        [FromQuery] PaginationParams paging, CancellationToken cancellationToken)
    {
        int? currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;

        var feed = await _postService.GetFeedAsync(paging, currentUserId, cancellationToken);
        return Ok(feed);
    }

    // GET: api/feed/following?page=1&pageSize=10  (protected: needs to know WHO is asking)
    // A user who follows nobody gets 200 with an empty Items list - a normal state, not an error.
    [Authorize]
    [HttpGet("following")]
    public async Task<ActionResult<PagedResult<FeedPostResponseDto>>> GetFollowingFeed(
        [FromQuery] PaginationParams paging, CancellationToken cancellationToken)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "غير مصرح: تعذر التثبت من هوية المستخدم." });
        }

        var feed = await _postService.GetFollowingFeedAsync(paging, currentUserId, cancellationToken);
        return Ok(feed);
    }
}
