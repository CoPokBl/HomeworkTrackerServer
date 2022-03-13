using System;
using HomeworkTrackerServer.Objects;
using HomeworkTrackerServer.Objects.ControllerClasses;
using HomeworkTrackerServer.Objects.HeaderParams;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers {
    
    [ApiController]
    [Route("auth")]
    public class LoginController : ApiController {
        
        [HttpGet]
        // Login
        public IActionResult Login([FromHeader] AuthorizationHeaderParams authorization) {
            
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
                Program.Debug(ex.ToString());
                return BadRequest();
            }

            if (!Program.Storage.AuthUser(externalUser.Username, externalUser.Password, out string id)) {
                return Unauthorized();
            }

            // do thing
            return Ok(Program.TokenHandler.GenerateToken(id));
        }
    }
}