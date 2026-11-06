using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroBlog.Api.Entities;

// The "many" side of the User (1) -> Posts (*) relationship
public class Post
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "محتوى المنشور مطلوب.")]
    [MaxLength(280, ErrorMessage = "محتوى المنشور يجب ألا يتجاوز 280 حرفاً.")]
    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Null until the post is edited
    public DateTime? UpdatedAt { get; set; }

    // Explicit foreign key: lets us set the owner without loading the whole User
    // (without it EF Core would create a hidden "shadow" property)
    [Required]
    public int UserId { get; set; }

    // Reference navigation property: the user who wrote the post
    [ForeignKey(nameof(UserId))]
    public User User { get; set; } = null!;
}
