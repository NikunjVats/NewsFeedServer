using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using NewsFeedServer.Models;
using NewsFeedServer.Services.Interfaces;
namespace NewsFeedServer.Services;

public class AuthenticationService : IAuthenticationService
{
    private IConfiguration _config;
    public AuthenticationService(IConfiguration config)
    {
        _config = config;
    }
    public string Authenticate(User user)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                new Claim("Name", user.Name),
                new Claim("role", "viewer")
            }),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(_config["Jwt:ExpireMinutes"])),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);
            return jwt;
        }
        catch (Exception ex)
        {
            throw new Exception("Error Authenticating the user", ex);
        }
    }
}
