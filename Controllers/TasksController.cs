using System;
using System.Collections.Generic;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Objects.ControllerClasses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {
    
    [ApiController]
    [Route("api/tasks")]
    public class TasksController : ApiController {

        [HttpGet]
        public ActionResult GetTasks() {
            
            // Auth
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms != null) return Ok(Program.Storage.GetTasks(perms.Id).ToArray());
            HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
            return Unauthorized();  // Kick em out

        }

        [HttpPost]
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
                return BadRequest();
            }
            
            Converter.DictionaryToHomeworkTask(task, out HomeworkTask obj, true);
            obj.Owner = perms.Id;
            return Ok(obj);

        }
        
        [HttpPatch("{id}")]
        public ActionResult EditTask(string id, [FromBody] JsonPatchDocument<ExternalHomeworkTask> patchData) {
            
            // Auth
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms == null) {
                HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
                return Unauthorized();  // Kick em out
            }

            string taskOwner = Program.Storage.GetOwnerOfTask(id);
            if (taskOwner == null) { return NotFound(); }
            if (taskOwner != perms.Id) { return Forbid(); }
            
            HomeworkTask internalTask = Program.Storage.GetTask(id);
            ExternalHomeworkTask externalTask = internalTask.ToExternal();
            try {
                patchData.ApplyTo(externalTask);
            }
            catch (Exception) {
                BadRequest();
            }

            try {
                
                if (internalTask.Class != externalTask.Class) 
                { Program.Storage.EditTask(perms.Id, id, "class", externalTask.Class); }
                if (internalTask.ClassColour != externalTask.ClassColour) 
                { Program.Storage.EditTask(perms.Id, id, "classColour", externalTask.ClassColour); }
                
                if (internalTask.Type != externalTask.Type) 
                { Program.Storage.EditTask(perms.Id, id, "type", externalTask.Type); }
                if (internalTask.TypeColour != externalTask.TypeColour) 
                { Program.Storage.EditTask(perms.Id, id, "typeColour", externalTask.TypeColour); }
                
                if (internalTask.Task != externalTask.Task) 
                { Program.Storage.EditTask(perms.Id, id, "task", externalTask.Task); }
                if (internalTask.DueDate != externalTask.DueDate) 
                { Program.Storage.EditTask(perms.Id, id, "dueDate", externalTask.DueDate.ToString()); }

            }
            catch (Exception) {
                // Failed to edit
                // Invalid values
                return BadRequest();
            }

            HomeworkTask obj = Program.Storage.GetTask(id);
            obj.Owner = perms.Id;
            obj.Id = internalTask.Id;
            return CreatedAtAction(nameof(GetTasks), id, obj);

        }

        [HttpDelete("{id}")]
        public ActionResult DeleteTask(string id) {
            // Auth
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms == null) {
                HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
                return Unauthorized();  // Kick em out
            }
            
            string taskOwner = Program.Storage.GetOwnerOfTask(id);
            if (taskOwner == null) { return NotFound(); }
            if (taskOwner != perms.Id) { return Forbid(); }

            Program.Storage.RemoveTask(perms.Id, id);
            return NoContent();
        }
        
    }
    
}