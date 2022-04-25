using System.Threading.Tasks;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Objects.ControllerClasses;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {
    
    [ApiController]
    [Route("/")]
    public class InfoPageController : ApiController {
        
        [HttpGet]
        public async Task<ActionResult> GetPage() {
            // Give it to them
            return Ok("HomeworkTrackerServer");
        }
        
        [HttpOptions]
        public IActionResult Options() {
            HttpContext.Response.Headers.Add("Allow", "GET,OPTIONS");
            return Ok();
        }
        
    }
    
}