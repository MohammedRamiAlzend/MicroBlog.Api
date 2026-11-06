namespace MicroBlog.Api.Entities;

public class Post
{
    // Primary key
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Author { get; set; } = "Anonymous";
    public int Likes { get; set; }
    public DateTime PublishedAt { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key pointing to the User who wrote the post
    public int UserId { get; set; }
}
