using System;
using System.Net;
using System.Threading.Tasks;
using HomeworkTrackerServer.Objects;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {
    
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase {

        public UsersController() { }

        [HttpGet]
        // Gets list of users (only for server admins)
        public async Task<ActionResult> GetUsers() {
            throw new NotImplementedException();
        }

        [HttpPost]
        // Registers a new user
        public async Task<ActionResult> Register(User user) {
            throw new NotImplementedException();
        }

        [HttpDelete("{username}")]
        // Deletes user
        public async Task<ActionResult> DeleteUser(string username) {
            
            // auth 
            
            
            // do it ig
            Program.Storage.RemoveUser(username);
            return NoContent();
        }
        
        
    }
}