using System;
using System.Collections.Generic;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Objects.ControllerClasses;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace HomeworkTrackerServer.Controllers; 

[ApiController]
[Route("api/tasks")]
public class TasksController : ApiController {

    [HttpGet]
    public ActionResult GetTasks() {
            
        // Auth
        Permissions perms = Authentication.GetPermsFromToken(HttpContext);
        if (perms != null) { return Ok(Program.Storage.GetTasks(perms.Id).ToArray()); }
        HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
        return Unauthorized();  // Kick em out

    }
        
    [HttpGet("{id}")]
    public ActionResult GetSpecificTask(string id) {
            
        Logger.Debug("Looking for task with id: " + id);
            
        // Auth
        Permissions perms = Authentication.GetPermsFromToken(HttpContext);
        if (perms == null) {
            HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
            return Unauthorized();  // Kick em out
        }
        
        // Rate limit
        if (!RateLimiting.CheckRequest(perms.Id)) {
            return StatusCode(429);
        }

        return Ok(Program.Storage.GetTask(id));
    }

    [HttpPut]
    public ActionResult AddTask(Dictionary<string, string> task) {

        // Auth
        Permissions perms = Authentication.GetPermsFromToken(HttpContext);
        if (perms == null) {
            HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
            return Unauthorized();  // Kick em out
        }
        
        // Rate limit
        if (!RateLimiting.CheckRequest(perms.Id)) {
            return StatusCode(429);
        }
            
        if (task == null) { return BadRequest("You must provide the task in JSON form"); }

        // Make sure all fields in task are less than 255 characters
        foreach (KeyValuePair<string, string> kvp in task) {
            if (kvp.Key.Length > 255) { return BadRequest("Fields cannot be more than 255 characters"); }
            if (kvp.Value.Length > 255) { return BadRequest("Fields cannot be more than 255 characters"); }
        }
            
        if (!Program.Storage.TryAddTask(perms.Id, task, out string id)) {
            // Failed
            // Invalid args
            return BadRequest("Failed to add task, rejected by storage");
        }

        Converter.TryConvertDicToTask(task, out HomeworkTask obj, true);
        obj.Owner = perms.Id;
        return CreatedAtAction(nameof(GetTasks), id, obj);

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
            BadRequest("Failed to apply patch, invalid patch data");
        }
            
        // make sure all fields in externalTask are less than 255 characters
        foreach (KeyValuePair<string, string> kvp in 
                 JsonConvert.DeserializeObject<Dictionary<string, string>>(JsonConvert.SerializeObject(externalTask))) {
            if (kvp.Key.Length > 255) { return BadRequest("Fields cannot be more than 255 characters"); }
            if (kvp.Value.Length > 255) { return BadRequest("Fields cannot be more than 255 characters"); }
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
            return BadRequest("Failed to edit task, rejected by storage");
        }

        HomeworkTask obj = Program.Storage.GetTask(id);
        obj.Owner = perms.Id;
        obj.Id = internalTask.Id;
        return Ok();

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
        
    [HttpOptions]
    public IActionResult Options() {
        HttpContext.Response.Headers.Add("Allow", "GET,PUT,PATCH,DELETE,OPTIONS");
        return Ok();
    }
        
    [HttpOptions("{id}")]
    public IActionResult OptionsPerTask() {
        HttpContext.Response.Headers.Add("Allow", "GET,PATCH,DELETE,OPTIONS");
        return Ok();
    }
        
}