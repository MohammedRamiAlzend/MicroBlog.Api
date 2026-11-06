using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Dependency Injection Container (Services)
// ==========================================
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

app.UseAuthorization();

// Quick health check using a Minimal API endpoint (kept for later sessions)
app.MapGet("/api/ping", () => Results.Ok(new
{
    Message = "MicroBlog API is running successfully!",
    Session = 8,
    Timestamp = DateTime.UtcNow
})).WithName("GetPingStatus");

app.MapControllers();

app.Run();
