using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace HomeworkTrackerServer.Objects.ControllerClasses; 

public class ApiController : Controller {
    public override void OnActionExecuting(ActionExecutingContext context) {
            
        // Allow connections from all origins
        foreach ((string, string) customHeader in Program.CustomHeaders) {
            HttpContext.Response.Headers.Add(customHeader.Item1, customHeader.Item2);
        }

        // debug headers
        if (Program.Debug) {  // I'd remove this but it's more efficient
            foreach (KeyValuePair<string, StringValues> header in HttpContext.Request.Headers) {
                Logger.Debug("Header | " + header.Key + ": " + header.Value);
            }
        }
            
        // get ip address
        IPAddress ip = Request.HttpContext.Connection.RemoteIpAddress;
        
        // Somehow it can be null
        string ipStr = ip == null ? "Unknown IP" : ip.ToString();

        base.OnActionExecuting(context);
        // Happens every request:
        Logger.Debug(context.HttpContext.Request.Headers.Keys.Contains("User-Agent")
            ? $"New request from: {ipStr} ({context.HttpContext.Request.Headers["User-Agent"]})"
            : $"New request from: {ipStr} (Unknown user agent)");
    }
}