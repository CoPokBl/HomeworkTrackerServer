using System;
using System.Collections.Generic;
using System.Net;

namespace HomeworkTrackerServer.Objects; 

public static class RateLimiting {
        
    /// <summary>
    /// List of all requests, all requests older than 1 minute are removed, format is (user id, time)
    /// </summary>
    private static readonly List<(string, DateTime)> Requests = new();
        
    /// <summary>
    /// List of IP addresses currently being rate limited, format is (user id, time)
    /// </summary>
    private static readonly List<(string, DateTime)> Blocked = new();

    /// <summary>
    /// Called every request to check if the IP address is rate limited
    /// </summary>
    /// <param name="userid">The id of the requester</param>
    /// <returns>Whether the request should be processed</returns>
    public static bool CheckRequest(string userid) {
            
        // remove entries in _blocked older than 5 minutes
        Blocked.RemoveAll(x => x.Item2 < DateTime.Now.AddMinutes(-5));
            
        if (Blocked.Contains((userid, DateTime.Now))) {  // IP is blocked
            return false;
        }
            
        // remove all entries older than 1 minute
        Requests.RemoveAll(x => x.Item2 < DateTime.Now.AddMinutes(-1));
            
        // check how many requests have been made from this ip
        List<(string, DateTime)> requests = Requests.FindAll(x => Equals(x.Item1, userid));

        if (requests.Count >= 30) {
            // if more than 30 requests have been made from this ip, block it
            Blocked.Add((userid, DateTime.Now));
            return false;
        }

        // add the request to the list and allow it
        Requests.Add((userid, DateTime.Now));
        return true;
    }
        
}