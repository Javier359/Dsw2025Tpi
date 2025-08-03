using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthenticateController : ControllerBase
{
    private readonly JwtTokenService _jweTokenService;

    public AuthenticateController(JwtTokenService jwtTokenService)
    {
        _jweTokenService = jwtTokenService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginModel request)
    {
        if (request.UserName == "admin" && request.Password == "123456")
        {
            var token = _jweTokenService.GenerateToken(request.UserName);
            return Ok(new {token });
        }
        return Unauthorized();
    }

}
