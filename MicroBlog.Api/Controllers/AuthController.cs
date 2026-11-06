using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Data;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    // POST: api/auth/register
    // [ApiController] already rejects invalid DTOs with 400 before this code runs
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
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
        await _context.SaveChangesAsync();

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
}
