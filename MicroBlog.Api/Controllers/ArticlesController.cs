using Microsoft.AspNetCore.Mvc;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ArticlesController : ControllerBase
{
    [HttpGet]
    public IActionResult GetArticles()
    {
        var articles = new[]
        {
            new { Id = 1, Title = "Introduction to ASP.NET Core 10", Content = "Learn how to build a modern and fast Web API..." },
            new { Id = 2, Title = "Interactive docs with Scalar", Content = "Scalar gives developers a great testing experience at /scalar/v1..." }
        };

        return Ok(articles);
    }
}
