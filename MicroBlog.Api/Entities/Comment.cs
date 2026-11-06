using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.Entities;

// A comment belongs to TWO parents at the same time: the user who wrote it and the post it is on
public class Comment
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Foreign key + navigation to the author
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    // Foreign key + navigation to the post
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}
