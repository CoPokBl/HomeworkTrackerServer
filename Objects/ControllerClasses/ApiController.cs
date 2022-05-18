using System.Collections.Generic;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace HomeworkTrackerServer.Objects.ControllerClasses; 

public class ApiController : Controller {
    public override void OnActionExecuting(ActionExecutingContext context) {
            
        // Allow connections from all origins
        HttpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Credentials", "true");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Methods", "GET,HEAD,OPTIONS,POST,PUT,PATCH,DELETE,TRACE,CONNECT");
        HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");

        // debug headers
        if (Program.Debug) {  // I'd remove this but it's more efficient
            foreach (KeyValuePair<string, StringValues> header in HttpContext.Request.Headers) {
                Logger.Debug("Header | " + header.Key + ": " + header.Value);
            }
        }
            
        // get ip address
        IPAddress ip = Request.HttpContext.Connection.RemoteIpAddress;
        string ipStr = ip.ToString();
            
        // rate limit
        if (!RateLimiting.CheckRequest(ip)) {
            Logger.Debug("Rate limit exceeded for ip: " + ipStr);
            // rate limit exceeded
            context.Result = new ContentResult {
                Content = "Rate limit exceeded",
                StatusCode = (int) HttpStatusCode.TooManyRequests
            };
            return;
        }
            

        base.OnActionExecuting(context);
        // Happens every request:
        Logger.Debug(context.HttpContext.Request.Headers.Keys.Contains("User-Agent")
            ? $"New request from: {ipStr} ({context.HttpContext.Request.Headers["User-Agent"]})"
            : $"New request from: {ipStr} (Unknown user agent)");
    }
}