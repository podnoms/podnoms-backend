using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {

    [Route ("[controller]")]
    public class AccountsController : BaseController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AccountsController (UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<AccountsController> logger) : base (logger) {
            _userManager = userManager;
            _mapper = mapper;
        }
        // POST api/accounts
        [HttpPost]
        public async Task<IActionResult> Post ([FromBody] RegistrationViewModel model) {
            if (!ModelState.IsValid) {
                return BadRequest (ModelState);
            }
            var userIdentity = _mapper.Map<RegistrationViewModel, ApplicationUser> (model);
            var result = await _userManager.CreateAsync (userIdentity, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult (result);
            return Ok (model);
        }
    }
}
