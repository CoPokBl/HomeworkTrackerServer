using System;
using System.Collections.Generic;
using System.Net;

namespace HomeworkTrackerServer.Objects {
    
    public static class RateLimiting {
        
        /// <summary>
        /// List of all requests, all tasks older than 1 minute are removed
        /// </summary>
        private static readonly List<(IPAddress, DateTime)> Requests = new List<(IPAddress, DateTime)>();
        
        /// <summary>
        /// List of IP addresses currently being rate limited
        /// </summary>
        private static readonly List<(IPAddress, DateTime)> Blocked = new List<(IPAddress, DateTime)>();

        /// <summary>
        /// Called every request to check if the IP address is rate limited
        /// </summary>
        /// <param name="ip">The IP of the requester</param>
        /// <returns>Whether the request should be processed</returns>
        public static bool CheckRequest(IPAddress ip) {
            
            // remove entries in _blocked older than 5 minutes
            Blocked.RemoveAll(x => x.Item2 < DateTime.Now.AddMinutes(-5));
            
            if (Blocked.Contains((ip, DateTime.Now))) {  // IP is blocked
                return false;
            }
            
            // remove all entries older than 1 minute
            Requests.RemoveAll(x => x.Item2 < DateTime.Now.AddMinutes(-1));
            
            // check how many requests have been made from this ip
            List<(IPAddress, DateTime)> requests = Requests.FindAll(x => Equals(x.Item1, ip));

            if (requests.Count >= 30) {
                // if more than 30 requests have been made from this ip, block it
                Blocked.Add((ip, DateTime.Now));
                return false;
            }

            // add the request to the list and allow it
            Requests.Add((ip, DateTime.Now));
            return true;
        }
        
    }
    
}