using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// 1. Dependency Injection Container (Services)
// ==========================================
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
