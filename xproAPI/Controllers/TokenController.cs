
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using xproAPI.Models;

namespace xproAPI.Controllers
{
    
    public class TokenController : ControllerBase
    {
        private static Random random = new Random();
        
        
        public static string CreateToken(string userType)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, userType),
            };
            DateTime value = DateTime.Now.AddMinutes(20.0);
            byte[] bytes = Encoding.UTF8.GetBytes("AvadaKedavraExpectopatronum69420!");
            SigningCredentials signingCredentials = new SigningCredentials(new SymmetricSecurityKey(bytes), "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256");
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = value,
                SigningCredentials = signingCredentials,
                Subject = new ClaimsIdentity(claims),
                Audience = "xproFront",
                Issuer = "xproAPI",
                IssuedAt = DateTime.Now,
            };
            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            SecurityToken token = jwtSecurityTokenHandler.CreateToken(tokenDescriptor);
            return (jwtSecurityTokenHandler.WriteToken(token));
        }
    }
}
