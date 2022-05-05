using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace HomeworkTrackerServer.Objects {
    
    public static class Authentication {
    
    	// for get perms functions null return means that the token is invalid

        public static Permissions GetPermsFromToken(Microsoft.AspNetCore.Http.HttpContext content) {
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

        private static Permissions GetPermsFromToken(string token) => 
            TokenHandler.ValidateCurrentToken(token, out string username) ? new Permissions(username) : null;

    }

    public class Permissions {
        public readonly bool IsSysAdmin;
        public readonly string Id;  // Now ids

        public Permissions(string userId) {
            Id = userId;
            IsSysAdmin = false;
        }
        
        public Permissions(string userId, bool isSysAdmin) {
            Id = userId;
            IsSysAdmin = isSysAdmin;
        }

        // Just for ease of access
        public bool IsAuthed(string id) => Id == id;

    }
    
}
