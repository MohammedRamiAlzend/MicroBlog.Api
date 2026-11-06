using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

// The client sends only the text. The author comes from the token, the post from the URL.
public class CreateCommentRequest
{
    [Required(ErrorMessage = "محتوى التعليق مطلوب ولا يمكن أن يكون فارغاً.")]
    [MaxLength(500, ErrorMessage = "لا يمكن أن يتجاوز التعليق 500 حرف.")]
    public string Content { get; set; } = string.Empty;
}
