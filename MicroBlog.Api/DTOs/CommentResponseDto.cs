namespace MicroBlog.Api.DTOs;

// Flat shape for the client: never return the Comment entity (Comment -> Post -> Comments ... cycle)
public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
}
