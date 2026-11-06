using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.Entities;

// The "one" side of the User (1) -> Posts (*) relationship
public class User
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Username { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Only a BCrypt hash is stored, never the plain password
    [Required]
    public string PasswordHash { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Bio { get; set; }

    // Goes into the token as ClaimTypes.Role
    public string Role { get; set; } = "User";

    // A banned account can sign in but cannot publish new posts
    public bool IsBanned { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Updated on every successful login (null = never logged in)
    public DateTime? LastLoginAt { get; set; }

    // Collection navigation property: all posts written by this user.
    // Initialised with an empty list so adding posts never hits a null reference
    public ICollection<Post> Posts { get; set; } = new List<Post>();

    // The posts this user liked (through the Likes join table)
    public ICollection<Like> Likes { get; set; } = new List<Like>();

    // The comments this user wrote
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();

    // Follow records where this user is the one being followed (their followers)
    public ICollection<Follow> Followers { get; set; } = new List<Follow>();

    // Follow records where this user is the follower (the accounts they follow)
    public ICollection<Follow> Following { get; set; } = new List<Follow>();
}
