using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace helloworld.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class HelloController : ControllerBase
{
    private readonly ILogger<HelloController> _logger;

    public HelloController(ILogger<HelloController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public IActionResult Get()
    {
        return Ok(new { Message = "Hello, World!", Version = "1.0" });
    }
}
