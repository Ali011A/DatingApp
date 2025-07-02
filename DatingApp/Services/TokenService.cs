using DatingApp.Entities;
using DatingApp.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DatingApp.Services
{
    public class TokenService(IConfiguration configuration,UserManager<AppUser> userManager) : ITokenService
    {
        public async Task<string> CreateToken(AppUser user)
        {
            var tokenKey=configuration["TokenKey"]?? throw new Exception("Token key not found in configuration");
            if(tokenKey.Length<64) throw new Exception(" token key is too short");
            var Key=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenKey));
            var claims = new List<Claim>
            {
                new (ClaimTypes.NameIdentifier, user.Id.ToString()),
                new (ClaimTypes.Name, user.UserName)
            };
            var roles=await userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(roles=> new Claim(ClaimTypes.Role,roles)));
            var credentials = new SigningCredentials(Key, SecurityAlgorithms.HmacSha512Signature);
            var TokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = credentials
            };
            var TokenHandler = new JwtSecurityTokenHandler();
            var Token = TokenHandler.CreateToken(TokenDescriptor);
            return await Task.FromResult(TokenHandler.WriteToken(Token));
        }
    }
}
