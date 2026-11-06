namespace MicroBlog.Api.Entities;

public class User
{
    // EF Core treats a property named Id as the primary key by convention
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Only a BCrypt hash is stored, never the plain password
    public string PasswordHash { get; set; } = string.Empty;

    public string? Bio { get; set; }

    // Goes into the token as ClaimTypes.Role
    public string Role { get; set; } = "User";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Updated on every successful login (null = never logged in)
    public DateTime? LastLoginAt { get; set; }
}
