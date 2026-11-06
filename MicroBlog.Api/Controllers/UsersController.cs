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

        return Ok(ToProfile(user));
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

        return Ok(ToProfile(user));
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

        return Ok(ToProfile(user));
    }

    // Entity -> DTO mapping, so PasswordHash never leaves the server
    private static UserProfileResponse ToProfile(User user)
    {
        return new UserProfileResponse
        {
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
