using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace HomeworkTrackerServer.Objects {
    public class TokenHandler {
        private readonly IConfiguration _config;

        public TokenHandler(IConfiguration config) {
            _config = config;
        }
        
        public string GenerateToken(string id) {
            string mySecret = _config["TokenSecret"];
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim("id", id),
                    new Claim("password", Program.Storage.GetUserPassword(id))
                }),
                Expires = DateTime.UtcNow.AddHours(int.Parse(_config["TokenExpirationHours"])),
                Issuer = _config["TokenIssuer"],
                Audience = _config["TokenAudience"],
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        public bool ValidateCurrentToken(string token, out string id) {
            string mySecret = _config["TokenSecret"];
            SymmetricSecurityKey mySecurityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken validToken;
            try {
                tokenHandler.ValidateToken(token, new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidIssuer = _config["TokenIssuer"],
                    ValidAudience = _config["TokenAudience"],
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

            string correctPass = Program.Storage.GetUserPassword(oid);
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