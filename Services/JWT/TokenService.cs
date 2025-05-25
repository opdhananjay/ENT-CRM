using System.Security.Claims;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

namespace ENT.Services.JWT
{
    public interface ITokenService
    {
       string GenerateToken(string email, string role, string orgId);
    }

    public class TokenService:ITokenService
    {
        public readonly string _key;
        public readonly string _issuer;
        public readonly string _audience;
        public readonly TimeSpan _expiration;

        public TokenService(IConfiguration configuration)
        {
            _key = configuration["jwt:key"];
            _issuer = configuration["jwt:Issuer"];
            _audience = configuration["jwt:Audience"];
            _expiration = TimeSpan.FromMinutes(int.Parse(configuration["jwt:ExpiryMinutes"])); // it is like => 00:30:00 => Converts in this format understanding
        }

        public string GenerateToken(string email,string role,string orgId)
        {
            try
            {
                var claims = new[]
                {
                    new Claim(ClaimTypes.Email, email),
                    new Claim(ClaimTypes.Role,role),
                    new Claim("OrgId",orgId)
                };

                var Key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
                var credential = new SigningCredentials(Key, SecurityAlgorithms.HmacSha256);

                // create token descriptors 
                var tokenDescriptors = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Issuer = _issuer,
                    Expires = DateTime.UtcNow.Add(_expiration),
                    Audience = _audience,
                    SigningCredentials = credential

                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptors);
                return tokenHandler.WriteToken(token);

            }
            catch(Exception ex)
            {
                throw ex;
            }
        }
    }
}
