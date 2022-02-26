using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HomeworkTrackerServer {
    public class TokenHandler {
        
        public static string GenerateToken(string username) {
            string mySecret = Program.Config["TokenSecret"];
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim(ClaimTypes.NameIdentifier, username),
                }),
                Expires = DateTime.UtcNow.AddHours(int.Parse(Program.Config["TokenExpirationHours"])),
                Issuer = Program.Config["TokenIssuer"],
                Audience = Program.Config["TokenAudience"],
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        public static bool ValidateCurrentToken(string token, out string username) {
            string mySecret = Program.Config["TokenSecret"];
            SymmetricSecurityKey mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validToken;
            try {
                tokenHandler.ValidateToken(token, new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = Program.Config["TokenIssuer"],
                    ValidAudience = Program.Config["TokenAudience"],
                    IssuerSigningKey = mySecurityKey
                }, out validToken);
            }
            catch {
                username = null;
                return false;
            }
            JwtSecurityTokenHandler tokenHandler2 = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = tokenHandler2.ReadToken(token) as JwtSecurityToken;
            string stringClaimValue = securityToken!.Claims.First(claim => claim.Type == "nameid").Value;
            username = stringClaimValue;
            return true;
        }
        
    }
}