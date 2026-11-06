namespace MicroBlog.Api.DTOs;

// Returned after like / unlike: the fresh counter plus the state of the heart button
public class LikeResponseDto
{
    public int PostId { get; set; }
    public int LikesCount { get; set; }
    public bool IsLiked { get; set; }
    public string Message { get; set; } = string.Empty;
}
