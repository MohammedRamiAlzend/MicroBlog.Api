using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.Services;

namespace MicroBlog.Api.Controllers;

[ApiController]
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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FeedPostResponseDto>>> GetGlobalFeed(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        int? currentUserId = int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int id) ? id : null;

        var feed = await _postService.GetFeedAsync(page, pageSize, currentUserId);
        return Ok(feed);
    }
}
