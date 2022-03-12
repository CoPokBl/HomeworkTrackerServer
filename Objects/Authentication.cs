using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Primitives;

namespace HomeworkTrackerServer.Objects {
    
    public class Authentication {
    
    	// for get perms functions null return means that the token is invalid

        public static Permissions GetPermsFromToken(Microsoft.AspNetCore.Http.HttpContext content) {
            if (!content.Request.Headers.ContainsKey("Authorization")) {
                // doesn't have header
                return null;
            }
            
            // Get all instances of the Auth header
            IEnumerable<KeyValuePair<string, StringValues>> authHeaders = 
                content.Request.Headers.Where(head => head.Key == "Authorization");
            List<string> validIds = new List<string>();

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

                
                if (Program.TokenHandler.ValidateCurrentToken(sections[1], out string id)) {
                    // successful token validation
                    validIds.Add(id);
                }
                // invalid token
                // it will ignore
            }

	        // If no names validated then return null, otherwise return the permissions object
            return validIds.Count == 0 ? null : new Permissions(validIds.ToArray());
        }

        private static Permissions GetPermsFromToken(string token) => 
            Program.TokenHandler.ValidateCurrentToken(token, out string username) ? new Permissions(username) : null;

    }

    public class Permissions {
        public bool IsSysAdmin;
        public readonly string[] Ids;  // Now ids

        public Permissions(string userId) {
            Ids = new [] { userId };
            IsSysAdmin = false;
        }
        
        public Permissions(string[] userIds) {
            Ids = userIds;
            IsSysAdmin = false;
        }
        
        public Permissions(string userId, bool isSysAdmin) {
            Ids = new [] { userId };
            IsSysAdmin = isSysAdmin;
        }
        
        public Permissions(string[] userIds, bool isSysAdmin) {
            Ids = userIds;
            IsSysAdmin = isSysAdmin;
        }
        
        // Just for ease of access
        public bool IsAuthed(string id) => Ids.Contains(id);

    }
    
}
