namespace MicroBlog.Api.DTOs;

// Returned after follow / unfollow: the new state of the button and the fresh counter
public class FollowResponseDto
{
    public int TargetUserId { get; set; }
    public bool IsFollowing { get; set; }
    public int FollowersCount { get; set; }
    public string Message { get; set; } = string.Empty;
}
