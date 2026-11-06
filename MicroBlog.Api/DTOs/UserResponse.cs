namespace MicroBlog.Api.DTOs;

// What the client is allowed to RECEIVE: no password or other sensitive fields
public class UserResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
