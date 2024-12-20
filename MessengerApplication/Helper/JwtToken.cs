using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MessengerApplication.Models;
using Microsoft.IdentityModel.Tokens;

namespace MessengerApplication.Helper
{
    public static class JwtToken
    {
        public static string GenerateJwtToken(User user, IHttpContextAccessor httpContextAccessor)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("mWwmSqZYUBXZCtGgWB9XjiWdMlhCFjJ9"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("Id", user.Id),
                new Claim("Fullname", user.Profile.FullName),
                new Claim("Username", user.UserName)
            };

            var token = new JwtSecurityToken(
                issuer: "Messenger",
                audience: "Messenger",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );

            var jwt = new JwtSecurityTokenHandler().WriteToken(token);

            httpContextAccessor.HttpContext.Response.Cookies.Append("access_token", jwt, new CookieOptions
            {
                HttpOnly = true,
                Secure = false,
                SameSite = SameSiteMode.Strict,
                Expires = DateTime.Now.AddDays(7)
            });

            return jwt;
        }
    }
}