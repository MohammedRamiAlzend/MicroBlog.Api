using Microsoft.AspNetCore.Mvc;
using MicroBlog.Api.Data;
using MicroBlog.Api.DTOs;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/users/{id}
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new { message = $"المستخدم رقم {id} غير موجود." });
        }

        return Ok(ToResponse(user));
    }

    // Entity -> DTO mapping, so PasswordHash never leaves the server
    private static UserResponseDto ToResponse(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Bio = user.Bio,
            CreatedAt = user.CreatedAt
        };
    }
}
