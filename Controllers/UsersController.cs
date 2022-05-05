using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Objects.ControllerClasses;
using HomeworkTrackerServer.Objects.HeaderParams;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {

    [ApiController]
    [Route("api/users")]
    public class UsersController : ApiController {
        
        [HttpGet]
        // Gets list of users (only for server admins)
        public async Task<ActionResult> GetUser([FromQuery] string username) {
            
            // auth
            string id = Program.Storage.GetUserId(username);
            if (id == null) {
                // user doesn't exist
                return NoContent();
            }
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            
            // Good auth
            if (perms != null && (perms.IsAuthed(id) || perms.IsSysAdmin)) return Ok(Program.Storage.GetUser(id));
            
            // Bad auth
            HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
            return Unauthorized();

        }

        [HttpPost]
        // Registers a new user
        public async Task<IActionResult> Register([FromHeader] AuthorizationHeaderParams authorization) {

            ExternalUser externalUser;
            try {
                if (authorization.GetAuthType() != "Basic") {
                    // bad
                    return BadRequest();
                }

                externalUser = new ExternalUser {
                    Username = authorization.GetUsername(),
                    Password = authorization.GetPassword()
                };
            }
            catch (Exception) {
                // Invalid something
                return BadRequest();
            }
            
            // do da thing
            User internalUser = new User(externalUser);
            if (!Program.Storage.CreateUser(internalUser)) {
                // failed
                return Conflict();
            }
            
            // It did it
            return CreatedAtAction(nameof(GetUser), new { id = internalUser.Guid }, internalUser);
        }

        [HttpPatch("{id}")]
        public async Task<ActionResult> ChangePassword(string id, [FromBody] JsonPatchDocument<ExternalUser> patchData) {
            
            // auth
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms == null || !perms.IsAuthed(id)) {
                HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
                return Unauthorized();
            }

            User internalUser = Program.Storage.GetUser(id);
            ExternalUser externalUser = internalUser.ToExternal();
            string originalPassword = externalUser.Password;
            string originalUsername = externalUser.Username;
            try {
                patchData.ApplyTo(externalUser);
            }
            catch (Exception) {
                BadRequest();
            }

            // do it ig
            if (originalPassword != externalUser.Password) {
                StringBuilder builder = new StringBuilder();
                foreach (byte t in SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(externalUser.Password))) {
                    builder.Append(t.ToString("x2"));
                }
                externalUser.Password = builder.ToString();
                Program.Storage.ChangePassword(id, externalUser.Password);
            }

            if (originalUsername != externalUser.Username) {
                // check to make sure the username isn't taken
                if (Program.Storage.GetUserId(externalUser.Username) != null) {
                    return Conflict("Username taken");
                }
                Program.Storage.ChangeUsername(id, externalUser.Username);
            }
            
            return Ok(externalUser);
        }

        [HttpDelete("{id}")]
        // Deletes user
        public async Task<ActionResult> DeleteUser(string id) {
            
            // auth
            Permissions perms = Authentication.GetPermsFromToken(HttpContext);
            if (perms == null || !perms.IsAuthed(id)) {
                HttpContext.Response.Headers.Add("WWW-Authenticate", Program.WwwAuthHeader);
                return Unauthorized();
            }
            
            // remove all of that users tasks
            List<Dictionary<string, string>> tasks = Program.Storage.GetTasks(id);
            foreach (Dictionary<string, string> task in tasks) {
                Program.Storage.RemoveTask(id, task["id"]);
            }

            // do it ig
            Program.Storage.RemoveUser(id);
            return NoContent();
        }
        
        [HttpOptions]
        public IActionResult Options() {
            HttpContext.Response.Headers.Add("Allow", "GET,DELETE,PATCH,POST,OPTIONS");
            return Ok();
        }
        
        [HttpOptions("{id}")]
        public IActionResult OptionsPerUser() {
            HttpContext.Response.Headers.Add("Allow", "PATCH,DELETE,OPTIONS");
            return Ok();
        }
        
        
    }
}