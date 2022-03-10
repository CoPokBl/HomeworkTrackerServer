using System.Collections.Generic;
using System.Net.Http;

namespace HomeworkTrackerServer.Objects {
    
    public class Authentication {

        public static Permissions GetPermsFromToken(HttpContent content) {
            if (!content.Headers.Contains("Authorization")) {
                // doesn't have header
                return null;
            }
            
            IEnumerable<string> authHeaders = content.Headers.GetValues("Authorization");
            List<string> validNames = new List<string>();
            
            foreach (string authHeader in authHeaders) {
                // TODO: make sure authHeader type is correct
                string[] sections = authHeader.Split(" ");
                if (sections.Length != 2) {
                    // not valid header
                    continue;
                }

                if (Program.TokenHandler.ValidateCurrentToken(sections[1], out string username)) {
                    // successful token validation
                    validNames.Add(username);
                }
                // invalid token
                // it will ignore
                    
            }

            return validNames.Count == 0 ? null : new Permissions(validNames.ToArray());
        }

        private static Permissions GetPermsFromToken(string token) {
            return Program.TokenHandler.ValidateCurrentToken(token, out string username) ? new Permissions(username) : null;
        }
        
    }

    public class Permissions {
        public bool IsSysAdmin;
        public string[] Username;

        public Permissions(string user) {
            Username = new [] { user };
            IsSysAdmin = false;
        }
        
        public Permissions(string[] users) {
            Username = users;
            IsSysAdmin = false;
        }
        
        public Permissions(string user, bool isSysAdmin) {
            Username = new [] { user };
            IsSysAdmin = isSysAdmin;
        }
        
        public Permissions(string[] users, bool isSysAdmin) {
            Username = users;
            IsSysAdmin = isSysAdmin;
        }
        
    }
    
}