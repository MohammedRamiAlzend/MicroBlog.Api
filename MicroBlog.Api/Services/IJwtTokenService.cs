using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Services;

public interface IJwtTokenService
{
    // expirationMinutes overrides the value from appsettings.json when provided
    string GenerateToken(User user, int? expirationMinutes = null);
}
