using System;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Objects.ControllerClasses;
using HomeworkTrackerServer.Objects.HeaderParams;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using RayKeys.Misc;

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
            catch (Exception ex) {
                // Invalid something
                // Logger.Debug(authorization.Authorization);
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
            try {
                patchData.ApplyTo(externalUser);
            }
            catch (Exception) {
                BadRequest();
            }

            if (originalPassword != externalUser.Password) {
                StringBuilder builder = new StringBuilder();
                foreach (byte t in SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(externalUser.Password))) {
                    builder.Append(t.ToString("x2"));
                }
                externalUser.Password = builder.ToString();
                Program.Storage.ChangePassword(id, externalUser.Password);
            }
            // do it ig
            Program.Storage.ChangeUsername(id, externalUser.Username);
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

            // do it ig
            Program.Storage.RemoveUser(id);
            return NoContent();
        }
        
        
    }
}