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
        
        public string GenerateToken(string username) {
            string mySecret = _config["TokenSecret"];
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(mySecret));
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor {
                Subject = new ClaimsIdentity(new Claim[] {
                    new Claim("username", username),
                    new Claim("password", Program.Storage.GetUserPassword(username))
                }),
                Expires = DateTime.UtcNow.AddHours(int.Parse(_config["TokenExpirationHours"])),
                Issuer = _config["TokenIssuer"],
                Audience = _config["TokenAudience"],
                SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256Signature),
            };
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        public bool ValidateCurrentToken(string token, out string username) {
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
                username = null;
                return false;
            }
            JwtSecurityTokenHandler tokenHandler2 = new JwtSecurityTokenHandler();
            JwtSecurityToken securityToken = tokenHandler2.ReadToken(token) as JwtSecurityToken;
            string user = null;
            string pass = null;
            foreach (Claim claim in securityToken.Claims) {
                switch (claim.Type) {
                    
                    case "username":
                        user = claim.Value;
                        break;
                    
                    case "password":
                        pass = claim.Value;
                        break;
                }
            }

            string correctPass = Program.Storage.GetUserPassword(user);
            if (pass != correctPass) {
                // wrong password hash in token
                throw new Exception("Invalid password in token");
            }
            username = user;
            return true;
        }
        
    }
}