namespace MicroBlog.Api.DTOs;

public class FollowStatsDto
{
    public int UserId { get; set; }
    public int FollowersCount { get; set; }
    public int FollowingCount { get; set; }

    // False for anonymous callers
    public bool IsFollowedByCurrentUser { get; set; }
}
