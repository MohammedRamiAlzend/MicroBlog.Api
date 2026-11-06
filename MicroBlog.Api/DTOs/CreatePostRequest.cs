using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

// Security rule: the client sends ONLY the content.
// There is no UserId here on purpose: if a client could send it, any user could
// publish posts in someone else's name (identity impersonation / IDOR).
// The owner is always taken from the validated JWT.
public class CreatePostRequest
{
    [Required(ErrorMessage = "محتوى المنشور مطلوب ولا يمكن أن يكون فارغاً.")]
    [MaxLength(280, ErrorMessage = "لا يمكن أن يتجاوز المحتوى 280 حرفاً.")]
    public string Content { get; set; } = string.Empty;
}
