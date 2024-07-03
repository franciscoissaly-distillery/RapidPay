using Microsoft.IdentityModel.Tokens;
using RapidPay.Api.Framework.Authentication;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace RapidPay.Auth.Api.Logic
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;

        public JwtTokenService(IConfiguration configuration)
        {
            _jwtSettings = JwtSettings.ReadFromConfiguration(configuration);
        }

        public string GenerateToken(string username)
        {
            var claims = new[] { new Claim(ClaimTypes.Name, username) };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.Now.AddHours(10), //expires: DateTime.Now.AddMinutes(10),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}