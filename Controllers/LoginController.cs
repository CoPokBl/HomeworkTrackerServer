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

            if (authorization == null) {
                return BadRequest("Authorization is missing");
            }

            if (Program.Debug) {
                Logger.Debug("Authorization: " + authorization);
            }
            
            ExternalUser externalUser;
            try {
                if (authorization.GetAuthType() != "Basic") {
                    // bad
                    return BadRequest("Invalid authorization type (It must be Basic)");
                }

                externalUser = new ExternalUser {
                    Username = authorization.GetUsername(),
                    Password = authorization.GetPassword()
                };
            }
            catch (Exception e) {
                // Invalid something
                return BadRequest(Program.Debug ? "Invalid authorization header: " + e : "Invalid authorization header: " + e.Message);
            }

            if (!Program.Storage.AuthUser(externalUser.Username, externalUser.Password, out string id)) {
                return Unauthorized();
            }

            // do thing
            return Ok(TokenHandler.GenerateToken(id));
        }
        
        [HttpOptions]
        public IActionResult Options() {
            HttpContext.Response.Headers.Add("Allow", "GET,OPTIONS");
            return Ok();
        }
        
    }
}