namespace MicroBlog.Api.Models;

public class Post
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Likes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
