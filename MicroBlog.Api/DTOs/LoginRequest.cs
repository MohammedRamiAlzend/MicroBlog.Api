using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

// The user can sign in with either a username or an email
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة.")]
    public string Password { get; set; } = string.Empty;
}
