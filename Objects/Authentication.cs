using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace HomeworkTrackerServer.Objects; 

/// <summary>
/// This class provides a simple way to check Authentication headers.
/// </summary>
public static class Authentication {
    
    /// <summary>
    /// This method gets the request senders permissions from HttpContext.
    /// </summary>
    /// <param name="content">The HttpContext to obtain the Authorization header from</param>
    /// <returns>The request senders permissions, returns null if the token is invalid</returns>
    public static Permissions GetPermsFromToken(HttpContext content) {
        if (!content.Request.Headers.ContainsKey("Authorization")) {
            // doesn't have header
            return null;
        }
            
        // Get all instances of the Auth header
        IEnumerable<KeyValuePair<string, StringValues>> authHeaders = 
            content.Request.Headers.Where(head => head.Key == "Authorization");

        KeyValuePair<string, StringValues>[] authHeaderPairs = authHeaders.ToArray();

        foreach (KeyValuePair<string, StringValues> authHeaderPair in authHeaderPairs) {

            string authHeader = authHeaderPair.Value;
                
            string[] sections = authHeader.Split(" ");
            if (sections.Length != 2) {
                // not valid header
                continue;
            }
            if (sections[0] != "Bearer") {  // Bearer is a token
                // Header doesn't specify Bearer authentication
                // This service only supports Bearer
                // Therefore header is invalid
                continue;
            }

                
            if (TokenHandler.ValidateCurrentToken(sections[1], out string id)) {
                // successful token validation
                return new Permissions(id);  // Only 1 auth header is allowed
            }
            // invalid token
            // it will ignore
        }

        // If no names validated then return null, otherwise return the permissions object
        return null;
    }

    /// <summary>
    /// Gets the request senders permissions from a token string.
    /// </summary>
    /// <param name="token">The JWT token</param>
    /// <returns>The request senders permissions, returns null if the token is invalid</returns>
    private static Permissions GetPermsFromToken(string token) => 
        TokenHandler.ValidateCurrentToken(token, out string username) ? new Permissions(username) : null;

}

/// <summary>
/// Represents the permissions of a user.
/// </summary>
public class Permissions {

    /// <summary>
    /// Whether or not the user is a server admin (Owns the server)
    /// </summary>
    public bool IsSysAdmin { get; }

    /// <summary>
    /// The ID of the validated user
    /// </summary>
    public string Id { get; }

    public Permissions(string userId) {
        Id = userId;
        IsSysAdmin = false;
    }
        
    public Permissions(string userId, bool isSysAdmin) {
        Id = userId;
        IsSysAdmin = isSysAdmin;
    }

    // Just for ease of access
    /// <summary>
    /// Checks if a specified user ID is Authenticated
    /// </summary>
    /// <param name="id">The user ID to check</param>
    /// <returns>Whether the user ID is Authenticated</returns>
    public bool IsAuthed(string id) => Id == id;

}