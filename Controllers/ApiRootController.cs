using HomeworkTrackerServer.Objects.ControllerClasses;
using Microsoft.AspNetCore.Mvc;

namespace HomeworkTrackerServer.Controllers; 

[ApiController]
[Route("api")]
public class ApiRootController : ApiController {

    [HttpGet]
    public ActionResult GetRequest() {
        // Give them the API version
        return Ok($"Homework Tracker API, made by CoPokBl using ASP.NET. Version {Program.Ver}.");
    }
        
    [HttpOptions]
    public IActionResult Options() {
        HttpContext.Response.Headers.Add("Allow", "GET,OPTIONS");
        return Ok();
    }
        
}