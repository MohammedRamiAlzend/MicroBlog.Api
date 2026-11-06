using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Data;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Controllers;

// [Authorize] on the class protects every endpoint inside it,
// unless a method opts out with [AllowAnonymous]
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users/me
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileResponse>> GetCurrentUserProfile()
    {
        // The id comes from the validated token, never from the URL or the body
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out int userId))
        {
            return Unauthorized(new { message = "بيانات هوية التوكن غير صالحة." });
        }

        // Read-only query: AsNoTracking skips change tracking (less memory, faster)
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        // The token can outlive the account, so always check that the user still exists
        if (user == null)
        {
            return NotFound(new { message = "لم يتم العثور على المستخدم." });
        }

        return Ok(await ToProfileAsync(user));
    }

    // PUT: api/users/me/bio
    [HttpPut("me/bio")]
    public async Task<ActionResult<UserProfileResponse>> UpdateMyBio([FromBody] UpdateBioRequest request)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int userId))
        {
            return Unauthorized(new { message = "بيانات هوية التوكن غير صالحة." });
        }

        // No AsNoTracking here: EF Core must track the entity to save the change
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(new { message = "لم يتم العثور على المستخدم." });
        }

        user.Bio = request.Bio.Trim();
        await _context.SaveChangesAsync();

        return Ok(await ToProfileAsync(user));
    }

    // GET: api/users/claims  (diagnostic: shows every claim read from the token)
    [HttpGet("claims")]
    public IActionResult GetMyClaims()
    {
        var claims = User.Claims.Select(c => new { c.Type, c.Value }).ToList();
        return Ok(claims);
    }

    // GET: api/users/{id}  (public profile)
    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserProfileResponse>> GetUserById(int id)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(new { message = $"المستخدم ذو المعرّف {id} غير موجود." });
        }

        return Ok(await ToProfileAsync(user));
    }

    // POST: api/users/{id}/follow  (protected)
    [HttpPost("{id:int}/follow")]
    public async Task<IActionResult> FollowUser(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "بيانات هوية التوكن غير صالحة." });
        }

        // Rule 1: nobody follows themselves (the database would happily accept the pair 1-1)
        if (id == currentUserId)
        {
            return BadRequest(new { message = "لا يمكنك متابعة حسابك الشخصي." });
        }

        // Rule 2: the target must exist
        if (!await _context.Users.AnyAsync(u => u.Id == id))
        {
            return NotFound(new { message = $"المستخدم رقم {id} غير موجود." });
        }

        // Rule 3: no duplicate follow
        if (await _context.Follows.AnyAsync(f => f.FollowerId == currentUserId && f.FollowingId == id))
        {
            return BadRequest(new { message = "أنت تتابع هذا المستخدم بالفعل." });
        }

        _context.Follows.Add(new Follow { FollowerId = currentUserId, FollowingId = id, CreatedAt = DateTime.UtcNow });

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two identical requests at once: the composite key rejects the second one
            return BadRequest(new { message = "أنت تتابع هذا المستخدم بالفعل." });
        }

        return Ok(await BuildFollowResponseAsync(id, true, "تمت المتابعة بنجاح."));
    }

    // DELETE: api/users/{id}/follow  (protected)
    [HttpDelete("{id:int}/follow")]
    public async Task<IActionResult> UnfollowUser(int id)
    {
        if (!int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int currentUserId))
        {
            return Unauthorized(new { message = "بيانات هوية التوكن غير صالحة." });
        }

        if (id == currentUserId)
        {
            return BadRequest(new { message = "لا يمكنك إجراء هذه العملية على حسابك الشخصي." });
        }

        var follow = await _context.Follows
            .FirstOrDefaultAsync(f => f.FollowerId == currentUserId && f.FollowingId == id);

        if (follow == null)
        {
            return BadRequest(new { message = "أنت لا تتابع هذا المستخدم أصلاً." });
        }

        _context.Follows.Remove(follow);
        await _context.SaveChangesAsync();

        return Ok(await BuildFollowResponseAsync(id, false, "تم إلغاء المتابعة بنجاح."));
    }

    // GET: api/users/{id}/follow-stats  (public; the "followed by me" flag needs a token)
    [AllowAnonymous]
    [HttpGet("{id:int}/follow-stats")]
    public async Task<ActionResult<FollowStatsDto>> GetFollowStats(int id)
    {
        if (!await _context.Users.AnyAsync(u => u.Id == id))
        {
            return NotFound(new { message = $"المستخدم رقم {id} غير موجود." });
        }

        return Ok(await BuildFollowStatsAsync(id));
    }

    private async Task<FollowStatsDto> BuildFollowStatsAsync(int userId)
    {
        // Anonymous callers (or a bad token) simply get false
        bool isFollowedByMe = false;

        if (int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out int viewerId))
        {
            isFollowedByMe = await _context.Follows
                .AnyAsync(f => f.FollowerId == viewerId && f.FollowingId == userId);
        }

        return new FollowStatsDto
        {
            UserId = userId,
            FollowersCount = await _context.Follows.CountAsync(f => f.FollowingId == userId),
            FollowingCount = await _context.Follows.CountAsync(f => f.FollowerId == userId),
            IsFollowedByCurrentUser = isFollowedByMe
        };
    }

    private async Task<FollowResponseDto> BuildFollowResponseAsync(int targetUserId, bool isFollowing, string message)
    {
        return new FollowResponseDto
        {
            TargetUserId = targetUserId,
            IsFollowing = isFollowing,
            FollowersCount = await _context.Follows.CountAsync(f => f.FollowingId == targetUserId),
            Message = message
        };
    }

    // Entity -> DTO mapping, so PasswordHash never leaves the server
    private async Task<UserProfileResponse> ToProfileAsync(User user)
    {
        var stats = await BuildFollowStatsAsync(user.Id);

        return new UserProfileResponse
        {
            FollowersCount = stats.FollowersCount,
            FollowingCount = stats.FollowingCount,
            IsFollowedByCurrentUser = stats.IsFollowedByCurrentUser,
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            AccountAgeDays = (int)(DateTime.UtcNow - user.CreatedAt).TotalDays
        };
    }
}
