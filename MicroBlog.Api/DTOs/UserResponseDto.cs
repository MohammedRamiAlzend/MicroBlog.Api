namespace MicroBlog.Api.DTOs;

// Never contains PasswordHash: even a hash must not leave the server (offline dictionary attacks)
public class UserResponseDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public DateTime CreatedAt { get; set; }
}
