using System.Threading.Tasks;
using HomeworkTrackerServer.Objects;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {
    
    [ApiController]
    [Route("/")]
    public class InfoPageController : ControllerBase {
        
        [HttpGet]
        public async Task<ActionResult> GetPage() {
            // Give it to them
            return Ok("Woooooo");

        }
        
    }
    
}