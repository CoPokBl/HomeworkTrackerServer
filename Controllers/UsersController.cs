using System;
using System.Threading.Tasks;
using HomeworkTrackerServer.Objects;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {
    
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {
        
        [HttpGet]
        // Gets list of users (only for server admins)
        public async Task<ActionResult<User[]>> GetUsers() {
            // auth
            if (!Authentication.GetPermsFromToken(HttpContext).IsSysAdmin) {
                // failed authentication
                return Unauthorized();  // L, imagine failing authentication
            }
            
            // Give it to them
            return Program.Storage.GetAllUsers();

        }

        [HttpPost]
        // Registers a new user
        public async Task<ActionResult<User>> Register(ExternalUser externalUser) {
            // do da thing
            User internalUser = new User(externalUser);
            if (!Program.Storage.CreateUser(internalUser)) {
                // failed
                return Conflict();
            }
            
            // It did it
            return internalUser;
        }
        
        [HttpPatch("{id}")]

        [HttpDelete("{id}")]
        // Deletes user
        public async Task<ActionResult> DeleteUser(string id) {
            
            // auth
            if (!Authentication.GetPermsFromToken(HttpContext).IsAuthed(id)) {
                // failed authentication
                return Unauthorized();  // L, imagine failing authentication
            }

            // do it ig
            Program.Storage.RemoveUser(id);
            return NoContent();
        }
        
        
    }
}