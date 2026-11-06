using System.ComponentModel.DataAnnotations;

namespace MicroBlog.Api.DTOs;

public class UpdateBioRequest
{
    [MaxLength(500, ErrorMessage = "النبذة الشخصية يجب ألا تتجاوز 500 حرف.")]
    public string Bio { get; set; } = string.Empty;
}
