using System;
using System.Collections.Generic;
using System.Net;

namespace HomeworkTrackerServer.Objects {
    
    public static class RateLimiting {
        
        private static List<(IPAddress, DateTime)> _requests = new List<(IPAddress, DateTime)>();
        private static List<(IPAddress, DateTime)> _blocked = new List<(IPAddress, DateTime)>();

        public static bool CheckRequest(IPAddress ip) {
            
            // remove entries in _blocked older than 5 minutes
            _blocked.RemoveAll(x => x.Item2 < DateTime.Now.AddMinutes(-5));
            
            if (_blocked.Contains((ip, DateTime.Now))) {  // IP is blocked
                return false;
            }
            
            // remove all entries older than 1 minute
            _requests.RemoveAll(x => x.Item2 < DateTime.Now.AddMinutes(-1));
            
            // check how many requests have been made from this ip
            List<(IPAddress, DateTime)> requests = _requests.FindAll(x => Equals(x.Item1, ip));

            if (requests.Count >= 30) {
                // if more than 30 requests have been made from this ip, block it
                _blocked.Add((ip, DateTime.Now));
                return false;
            }

            // add the request to the list and allow it
            _requests.Add((ip, DateTime.Now));
            return true;
        }
        
    }
    
}