using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PodNoms.Api.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;

namespace PodNoms.Api.Controllers {

    [Route("[controller]")]
    public class AccountsController : BaseController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public AccountsController(UserManager<ApplicationUser> userManager, IMapper mapper, ILogger<AccountsController> logger) : base(logger) {
            this._userManager = userManager;
            this._mapper = mapper;
        }
        // POST api/accounts
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RegistrationViewModel model) {
            if (!ModelState.IsValid) {
                return BadRequest(ModelState);
            }
            var userIdentity = _mapper.Map<RegistrationViewModel, ApplicationUser>(model);
            var result = await _userManager.CreateAsync(userIdentity, model.Password);
            // var result = await _userRepository.AddOrUpdate(userIdentity, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(result);
            return Ok(model);
        }
    }
}