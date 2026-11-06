using Microsoft.AspNetCore.Mvc;

namespace MicroBlog.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatusController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStatus()
    {
        return Ok(new
        {
            Status = "Operational",
            ServerName = Environment.MachineName,
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0"
        });
    }
}
