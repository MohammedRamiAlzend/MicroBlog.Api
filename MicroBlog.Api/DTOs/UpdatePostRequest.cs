using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

// Only the content can be edited. No UserId / CreatedAt here on purpose:
// the owner never changes and the timestamps are set by the server.
public class UpdatePostRequest
{
    [Required(ErrorMessage = "محتوى المنشور مطلوب ولا يمكن أن يكون فارغاً.")]
    [MaxLength(280, ErrorMessage = "لا يمكن أن يتجاوز المحتوى 280 حرفاً.")]
    public string Content { get; set; } = string.Empty;
}
