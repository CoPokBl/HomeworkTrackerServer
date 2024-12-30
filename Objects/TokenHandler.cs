using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HomeworkTrackerServer.Objects; 

public static class TokenHandler {

    /// <summary>
    /// Create a JWT token for the given user
    /// </summary>
    /// <param name="id">The ID of the user that the token is for</param>
    /// <returns>The resulting JWT</returns>
    public static string GenerateToken(string id) {
        string mySecret = Program.Config["TokenSecret"];
        SymmetricSecurityKey securityKey = new(Encoding.ASCII.GetBytes(mySecret));
        JwtSecurityTokenHandler tokenHandler = new();
        SecurityTokenDescriptor tokenDescriptor = new() {
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
        
    /// <summary>
    /// Check if an ID is valid and get the owner
    /// </summary>
    /// <param name="token">The JWT token to check</param>
    /// <param name="id">The ID of the user</param>
    /// <returns>Whether or not the token is valid</returns>
    public static bool ValidateCurrentToken(string token, out string id) {
        string mySecret = Program.Config["TokenSecret"];
        SymmetricSecurityKey mySecurityKey = new(Encoding.ASCII.GetBytes(mySecret));
        JwtSecurityTokenHandler tokenHandler = new();
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
        JwtSecurityTokenHandler tokenHandler2 = new();
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
                
                default:
                    // If it isn't one of the ones we're looking for, we don't care
                    // Apparently, writing useless default cases is good practice
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