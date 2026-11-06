using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

// What the client is allowed to SEND when registering
public class RegisterRequest
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب.")]
    [StringLength(20, MinimumLength = 3, ErrorMessage = "يجب أن يكون اسم المستخدم بين 3 و 20 حرفاً.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
    [MinLength(8, ErrorMessage = "يجب ألا تقل كلمة المرور عن 8 خانات.")]
    public string Password { get; set; } = string.Empty;
}
