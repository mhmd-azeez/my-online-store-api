using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using MyOnlineStoreAPI.Data;
using MyOnlineStoreAPI.Helpers;

using System;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace MyOnlineStoreAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly AuthOptions _options;

        public AuthController(ApplicationDbContext applicationDbContext, IOptions<AuthOptions> options)
        {
            _applicationDbContext = applicationDbContext;
            _options = options.Value;
        }

        [HttpPost("token")]
        public async Task<ActionResult<TokenResponse>> LoginFormPost([FromForm] TokenRequest request)
        {
            var user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Username.ToLowerInvariant());
            if (user is null) return Unauthorized();

            var result = PasswordHelper.Hash(request.Password, user.PasswordSalt);
            if (result.PasswordHash != user.PasswordHash)
                return Unauthorized();

            if (user.IsActive == false)
                return Forbid();

            // https://dotnetcoretutorials.com/2020/01/15/creating-and-validating-jwt-tokens-in-asp-net-core/
            // Note: _options.Secret must be at least 16 characters
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));

            var lifetime = TimeSpan.FromHours(8);

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, request.Username),
                    new Claim(ClaimTypes.Role, user.Role),
                }),
                Expires = DateTime.UtcNow.Add(lifetime),
                Issuer = _options.Issuer,
                Audience = _options.Audience,
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            var response = new TokenResponse
            {
                AccessToken = tokenHandler.WriteToken(token),
                ExpiresIn = (int)lifetime.TotalSeconds
            };

            return Ok(response);
        }
    }

    public class TokenRequest
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; }
        [JsonPropertyName("token_type")]
        public string TokenType { get; } = "Bearer";
        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
