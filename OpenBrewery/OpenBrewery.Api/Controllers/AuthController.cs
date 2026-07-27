using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenBrewery.Core.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OpenBrewery.Api.Controllers
{
    [Controller]
    [Route("api/auth")]
    public class AuthController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly JwtOptions _jwtOptions;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            _jwtOptions = _configuration.GetSection("Jwt").Get<JwtOptions>() ?? throw new InvalidOperationException("JWT configuration is missing.");
        }

        [AllowAnonymous]
        [HttpPost("token")]
        public IActionResult GenerateToken()
        {

            var key = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_jwtOptions.SecretKey)
                );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
                {
                    new(JwtRegisteredClaimNames.Sub, "user-123"),
                    new(JwtRegisteredClaimNames.UniqueName, "testuser"),
                    new(ClaimTypes.Role, "Reader")
                };

            JwtSecurityToken jwtSecurityToken = new JwtSecurityToken(
                    issuer: _jwtOptions.Issuer,
                    audience: _jwtOptions.Audience,
                    claims: claims,
                    expires: DateTime.UtcNow.AddMinutes(_jwtOptions.Expirations),
                    signingCredentials: credentials
                    );

            var accessToken = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
            return Ok(accessToken);
        }
    }
}
