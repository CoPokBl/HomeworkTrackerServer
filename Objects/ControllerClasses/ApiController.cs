using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RayKeys.Misc;

namespace HomeworkTrackerServer.Objects.ControllerClasses {
    public class ApiController : Controller {
        public override void OnActionExecuting(ActionExecutingContext context) {
            
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
}