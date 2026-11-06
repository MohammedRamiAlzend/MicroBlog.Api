namespace MicroBlog.Api.Entities;

// Join entity of a self-referencing many-to-many: a User follows many Users and is followed by many Users.
// Both foreign keys point to the SAME table (Users), so the roles need clear names.
public class Follow
{
    // The user who presses "Follow"
    public int FollowerId { get; set; }
    public User Follower { get; set; } = null!;

    // The user who is being followed
    public int FollowingId { get; set; }
    public User Following { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
