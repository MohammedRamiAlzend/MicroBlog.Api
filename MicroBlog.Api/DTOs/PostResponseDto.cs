namespace MicroBlog.Api.DTOs;

// What the API returns for a post. Returning the Post entity directly would
// serialize Post -> User -> Posts -> User ... and fail with an object-cycle error
public class PostResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int UserId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;
    public int LikesCount { get; set; }
    public bool IsLikedByCurrentUser { get; set; }
}
