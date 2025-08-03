using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models;

namespace helloworld.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;

    public AuthController(ILogger<AuthController> logger)
    {
        _logger = logger;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel model)
    {
        if (model.Username != "admin" || model.Password != "password")
            return Unauthorized();

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, model.Username)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super-secret-key-12345"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "JwtVersionedApi",
            audience: "JwtVersionedApiClient",
            claims: claims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: creds);

        return Ok(new { token = new JwtSecurityTokenHandler().WriteToken(token) });
    }
}
