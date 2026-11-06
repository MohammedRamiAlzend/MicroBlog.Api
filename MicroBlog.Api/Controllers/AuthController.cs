using System.IdentityModel.Tokens.Jwt;
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
[Produces("application/json")]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    // Common weak passwords that are rejected (session lab)
    private static readonly string[] ForbiddenPasswords = { "123456", "password", "qwerty", "12345678" };

    public AuthController(AppDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    // POST: api/auth/register
    // [ApiController] already rejects invalid DTOs with 400 before this code runs
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        // 0. Reject very common passwords
        if (ForbiddenPasswords.Contains(request.Password.ToLower()))
        {
            return BadRequest(new { message = "كلمة المرور هذه سهلة التخمين وشائعة جداً، يرجى اختيار كلمة مرور أقوى." });
        }

        // 1. Duplicate username check (AnyAsync produces a cheap EXISTS query)
        bool usernameExists = await _context.Users
            .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower());

        if (usernameExists)
        {
            return Conflict(new { message = "اسم المستخدم مستخدم بالفعل." });
        }

        // 2. Duplicate email check
        bool emailExists = await _context.Users
            .AnyAsync(u => u.Email.ToLower() == request.Email.ToLower());

        if (emailExists)
        {
            return Conflict(new { message = "البريد الإلكتروني مسجل بالفعل." });
        }

        // 3. One-way hash with an automatic random salt (never store the plain password)
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // 4. DTO -> Entity, then save
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = passwordHash,
            Bio = request.Bio,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            // Two identical requests can both pass the AnyAsync checks above at the same time;
            // the unique indexes (Username / Email) are the real guard and reject the second insert.
            return Conflict(new { message = "اسم المستخدم أو البريد الإلكتروني مستخدم بالفعل." });
        }

        // 5. Entity -> clean response DTO (no PasswordHash)
        var response = new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }

    // POST: api/auth/login
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Sign in with either the username or the email
        var identifier = !string.IsNullOrWhiteSpace(request.Username) ? request.Username : request.Email;

        var user = await _context.Users.FirstOrDefaultAsync(u =>
            u.Username.ToLower() == identifier.ToLower() ||
            u.Email.ToLower() == identifier.ToLower());

        // One identical 401 for "unknown user" and "wrong password":
        // different messages would let attackers discover which accounts exist (enumeration)
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة (Invalid username or password)." });
        }

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        var token = _jwtTokenService.GenerateToken(user);

        var response = new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Email = user.Email,
            ExpiresAt = new JwtSecurityTokenHandler().ReadJwtToken(token).ValidTo,
            User = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Bio = user.Bio,
                CreatedAt = user.CreatedAt
            }
        };

        return Ok(response);
    }

    // GET: api/auth/me  (protected)
    // Without a valid token the middleware answers 401 before this method runs
    [Authorize]
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        // The identity comes from the token claims, not from the request body
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var username = User.FindFirstValue(ClaimTypes.Name);
        var email = User.FindFirstValue(ClaimTypes.Email);
        var role = User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(new { message = "بيانات الهوية غير مكتملة في التوكن." });
        }

        return Ok(new
        {
            Id = userId,
            Username = username,
            Email = email,
            Role = role,
            IsAuthenticated = User.Identity?.IsAuthenticated ?? false
        });
    }
}
