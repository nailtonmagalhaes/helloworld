using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Models;
using Services;

namespace helloworld.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IConfiguration _configuration;
    private readonly TokenService _tokenService;
    private readonly RefreshTokenStore _refreshStore;

    public AuthController(
        ILogger<AuthController> logger,
        IConfiguration configuration,
        TokenService tokenService,
        RefreshTokenStore refreshStore)
    {
        _logger = logger;
        _configuration = configuration;
        _tokenService = tokenService;
        _refreshStore = refreshStore;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public IActionResult Login([FromBody] LoginModel request)
    {
        // Simulação de autenticação
        if (request.Username != "admin" || request.Password != "123") return Unauthorized();

        var userId = "user-001";
        var accessToken = _tokenService.GenerateAccessToken(userId);
        var refreshToken = _tokenService.GenerateRefreshToken();

        _refreshStore.Save(userId, refreshToken);

        return Ok(new TokenResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken
        });
    }

    [HttpPost("refresh")]
    public IActionResult Refresh([FromBody] TokenResponse request)
    {
        var principal = GetPrincipalFromExpiredToken(request.AccessToken);
        var userId = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userId == null || !_refreshStore.Validate(userId, request.RefreshToken))
            return Unauthorized();

        var newAccessToken = _tokenService.GenerateAccessToken(userId);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        _refreshStore.Rotate(userId, newRefreshToken);

        return Ok(new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken
        });
    }

    private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = false, // Ignora expiração

            ValidIssuer = _configuration["Jwt:Issuer"],
            ValidAudience = _configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        return principal;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin-data")]
    public IActionResult GetAdminData()
    {
        return Ok("Somente Admins podem ver isso.");
    }

    [Authorize]
    [HttpGet("profile")]
    public IActionResult GetProfile()
    {
        var username = User.Identity.Name;
        var department = User.FindFirst("Department")?.Value;
        var accessLevel = User.FindFirst("AccessLevel")?.Value;

        return Ok(new { username, department, accessLevel });
    }
}
