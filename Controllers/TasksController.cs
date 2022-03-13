using System.Collections.Generic;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Objects.ControllerClasses;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {
    
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ApiController {

        [HttpGet]
        public ActionResult GetTasks() {
            
            // Auth
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms == null) {
                HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
                return Unauthorized();  // Kick em out
            }

            return Ok(Program.Storage.GetTasks(perms.Id).ToArray());
        }

        [HttpPost]
        // TODO: Fix whatever tf is happening here
        public ActionResult AddTask(Dictionary<string, string> task) {
            
            // Auth
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms == null) {
                HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
                return Unauthorized();  // Kick em out
            }
            
            if (task == null) { return BadRequest(); }

            if (!Program.Storage.AddTask(perms.Id, task, out string taskId)) {
                // Failed
                // Invalid args
                BadRequest();
            }
            
            Converter.DictionaryToHomeworkTask(task, out HomeworkTask obj, true);
            obj.Owner = perms.Id;
            return CreatedAtAction(nameof(GetTasks), taskId, obj);

        }
        
    }
    
}