using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HomeworkTrackerServer.Objects {
    public static class TokenHandler {

        public static string GenerateToken(string id) {
            string mySecret = Program.Config["TokenSecret"];
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new[] {
                    new Claim("id", id),
                    new Claim("password", Program.Storage.GetUserPassword(id))
                }),
                Expires = DateTime.UtcNow.AddHours(int.Parse(Program.Config["TokenExpirationHours"])),
                Issuer = Program.Config["TokenIssuer"],
                Audience = Program.Config["TokenAudience"],
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        public static bool ValidateCurrentToken(string token, out string id) {
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
                id = null;
                return false;
            }
            JwtSecurityTokenHandler tokenHandler2 = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = tokenHandler2.ReadToken(token) as JwtSecurityToken;
            string oid = null;
            string pass = null;
            foreach (Claim claim in securityToken.Claims) {
                switch (claim.Type) {
                    
                    case "id":
                        oid = claim.Value;
                        break;
                    
                    case "password":
                        pass = claim.Value;
                        break;
                }
            }

            string correctPass;
            try { correctPass = Program.Storage.GetUserPassword(oid); }
            catch (KeyNotFoundException) {
                // User doesn't exist
                id = null;
                return false;
            }
            
            if (pass != correctPass) {
                // wrong password hash in token
                id = null;
                return false;
            }
            id = oid;
            return true;
        }
        
    }
}