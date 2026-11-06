using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using MicroBlog.Api.Data;
using MicroBlog.Api.Middleware;
using MicroBlog.Api.Services;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Dependency Injection Container (Services)
// ==========================================
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<AuthorizeOperationTransformer>();
});

// Register the service layer in the DI container (Scoped = one instance per HTTP request)
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Register the database context (SQLite). The connection string comes from appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));


// Authentication: validate the JWT sent in the Authorization header
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JwtSettings:Key is missing from the configuration.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // no extra grace period after the token expires
    };
});

// Authorization: enables [Authorize] / [AllowAnonymous]
builder.Services.AddAuthorization();

// CORS policy: lets the React/Vite app (port 5173) call this API from the browser
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowViteApp", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// ==========================================
// 2. Middleware Request Pipeline
// ==========================================
// First in the pipeline so it can catch exceptions thrown by everything after it
app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    // Generates the OpenAPI JSON document at /openapi/v1.json
    app.MapOpenApi();

    // Scalar is only a UI: it reads the document produced by MapOpenApi()
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("MicroBlog API Reference")
               .WithTheme(ScalarTheme.Moon)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

app.UseHttpsRedirection();

// Order matters: UseCors goes after UseHttpsRedirection and before UseAuthorization / MapControllers
app.UseCors("AllowViteApp");

// Order matters: first identify the user (Authentication), then check permissions (Authorization)
app.UseAuthentication();
app.UseAuthorization();

// Quick health check using a Minimal API endpoint (kept for later sessions)
app.MapGet("/api/ping", () => Results.Ok(new
{
    Message = "MicroBlog API is running successfully!",
    Session = 24,
    Timestamp = DateTime.UtcNow
})).WithName("GetPingStatus");

app.MapControllers();

app.Run();
