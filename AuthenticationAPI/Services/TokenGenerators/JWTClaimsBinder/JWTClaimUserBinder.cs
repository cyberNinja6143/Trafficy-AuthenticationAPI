using AuthenticationAPI.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AuthenticationAPI.Services.TokenGenerators.JWTClaimsBinder
{
    public class JWTClaimUserBinder
    {
        public User turnToUser(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadJwtToken(token);
            ClaimsPrincipal claims = new ClaimsPrincipal(new ClaimsIdentity(jwtToken.Claims));
            
            User decodedUser = new User();
            decodedUser.UUID = Guid.Parse(claims.Claims.FirstOrDefault(c => c.Type == "UUID").Value);
            decodedUser.Username = claims.Claims.FirstOrDefault(c => c.Type == "Username").Value;
            decodedUser.Email = claims.Claims.FirstOrDefault(c => c.Type == "Email").Value;
            return decodedUser;
        }
    }
}
