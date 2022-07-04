using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using PodNoms.Identity.Data;
using PodNoms.Identity.Models;

namespace PodNoms.Identity.Controllers;

[Route("[controller]")]
public class PingController : Controller {
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() {
        return new OkObjectResult(new {
            Result = "pong"
        });
    }

    [HttpGet("auth")]
    [Authorize(AuthenticationSchemes = OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme)]
    public IActionResult AuthPingGet() {
        return new OkObjectResult(new {
            Result = "auth-ping"
        });
    }
}
