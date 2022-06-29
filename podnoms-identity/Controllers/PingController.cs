using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Identity.Data;
using PodNoms.Identity.Models;

namespace PodNoms.Identity.Controllers;

[Authorize]
[Route("[controller]")]
public class PingController : Controller {

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() {
        return new OkObjectResult(new {
            Result = "pong"
        });
    }
}
