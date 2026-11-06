namespace MicroBlog.Api.Entities;

public class User
{
    // EF Core treats a property named Id as the primary key by convention
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
