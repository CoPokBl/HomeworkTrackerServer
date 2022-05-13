using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeworkTrackerServer.Objects.ControllerClasses {
    
    public class TasksController : Controller {
        
        public override void OnActionExecuting(ActionExecutingContext context) {
            base.OnActionExecuting(context);
            // Happens every request:
            Logger.Debug(context.HttpContext.Request.Headers.Keys.Contains("User-Agent")
                ? $"New request from: {context.HttpContext.Request.Headers["User-Agent"]}"
                : "New request from unknown user agent");
            
            // Authenticate request
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms == null) {
                HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
                context.Result = Unauthorized();  // Kick em out
            }
            
            // Continue
        }
        
    }
    
}