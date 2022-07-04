using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Identity.Data;
using PodNoms.Identity.Models;

namespace PodNoms.Identity.Controllers.Auth;

[Authorize]
[Route("[controller]")]
public class AccountController : Controller {
    private readonly UserManager<ApplicationUser> _userManager;

    public AccountController(
        UserManager<ApplicationUser> userManager) {
        _userManager = userManager;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterViewModel model) {
        if (ModelState.IsValid) {
            var user = await _userManager.FindByNameAsync(model.Email);
            if (user != null) {
                return StatusCode(StatusCodes.Status409Conflict);
            }

            user = new ApplicationUser {UserName = model.Email, Email = model.Email};
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded) {
                return Ok();
            }

            AddErrors(result);
        }

        // If we got this far, something failed.
        return BadRequest(ModelState);
    }

    #region Helpers

    private void AddErrors(IdentityResult result) {
        foreach (var error in result.Errors) {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

    #endregion
}
