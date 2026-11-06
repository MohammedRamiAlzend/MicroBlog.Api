using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

// The client sends only the title and the content.
// The author and UserId come from the token, so they cannot be forged.
public class CreatePostDto
{
    [Required(ErrorMessage = "عنوان المنشور مطلوب.")]
    [MaxLength(120, ErrorMessage = "العنوان يجب ألا يتجاوز 120 حرفاً.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "محتوى المنشور مطلوب.")]
    [MaxLength(280, ErrorMessage = "لا يمكن أن يتجاوز النص 280 حرفاً.")]
    public string Content { get; set; } = string.Empty;
}
