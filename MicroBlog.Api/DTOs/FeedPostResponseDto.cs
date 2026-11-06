namespace MicroBlog.Api.DTOs;

// One card of the global feed: the post, its author, the aggregated counters,
// and whether the person looking at the feed already liked it.
public class FeedPostResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public int AuthorId { get; set; }
    public string AuthorUsername { get; set; } = string.Empty;

    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }

    // False for anonymous visitors
    public bool IsLikedByCurrentUser { get; set; }
}
