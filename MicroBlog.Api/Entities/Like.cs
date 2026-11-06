namespace MicroBlog.Api.Entities;

// Join entity that breaks the User (*) <-> (*) Post relationship into two one-to-many relationships.
// The composite primary key (UserId, PostId) is configured in AppDbContext and guarantees
// that one user can like one post only once, even if two requests arrive at the same time.
public class Like
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int PostId { get; set; }
    public Post Post { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
