using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب.")]
    [StringLength(30, MinimumLength = 3, ErrorMessage = "يجب أن يكون اسم المستخدم بين 3 و 30 حرفاً.")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "اسم المستخدم يجب أن يحتوي على أحرف إنجليزية وأرقام وشرطة سفلية (_) فقط بدون مسافات.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب.")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
    [MinLength(6, ErrorMessage = "يجب ألا تقل كلمة المرور عن 6 خانات.")]
    public string Password { get; set; } = string.Empty;

    [MaxLength(100, ErrorMessage = "السيرة الذاتية يجب ألا تتجاوز 100 حرف.")]
    public string? Bio { get; set; }
}
