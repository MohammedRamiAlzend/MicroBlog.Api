using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using MicroBlog.Api.Entities;

namespace MicroBlog.Api.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config)
    {
        _config = config;
    }

    public string GenerateToken(User user, int? expirationMinutes = null)
    {
        var settings = _config.GetSection("JwtSettings");

        // 1. The signing key (HMAC-SHA256 needs at least 32 characters)
        var secretKey = settings["Key"]
            ?? throw new InvalidOperationException("JwtSettings:Key is missing from the configuration.");
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // 2. The claims stored in the token payload
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        // 3. Lifetime: the argument wins, otherwise the configured value, otherwise 60 minutes
        double minutes = expirationMinutes
            ?? double.Parse(settings["ExpiryMinutes"] ?? "60");

        var token = new JwtSecurityToken(
            issuer: settings["Issuer"],
            audience: settings["Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(minutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
