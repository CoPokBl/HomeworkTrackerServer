using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HomeworkTrackerServer.Objects {
    public class ApiController : Controller {
        public override void OnActionExecuting(ActionExecutingContext context) {
            base.OnActionExecuting(context);
            // Happens every request:
            Program.Debug($"New request from: {context.HttpContext.Request}");
        }
    }
}