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
    // [ApiController] validates the DTO automatically and returns 400 before this code runs
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register([FromBody] RegisterRequest request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return BadRequest(new { message = "البريد الإلكتروني مُسجل بالفعل." });
        }

        // DTO -> Entity
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = $"hashed_{request.Password}", // placeholder: real hashing (BCrypt) comes in Session 12
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        // Entity -> safe response DTO (PasswordHash is never returned)
        var response = new UserResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };

        return CreatedAtAction(nameof(Register), new { id = response.Id }, response);
    }
}
