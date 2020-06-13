using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    public class ServerShowcasesController : BaseAuthController {
        private readonly IRepository<ServerShowcase> _repository;
        private readonly IMapper _mapper;

        public ServerShowcasesController(
                        IHttpContextAccessor contextAccessor,
                        UserManager<ApplicationUser> userManager,
                        ILogger<ServerShowcasesController> logger,
                        IRepository<ServerShowcase> repository,
                        IMapper mapper) : base(contextAccessor, userManager, logger) {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<ServerShowcaseViewModel>> GetShowcaseForUser() {
            var candidate = await _repository.GetAll()
                .Where(r => r.StartDate <= System.DateTime.Today)
                .Where(r => r.EndDate >= System.DateTime.Today)
                .Where(r => r.IsActive)
                .SingleOrDefaultAsync();
            return _mapper.Map<ServerShowcaseViewModel>(candidate);
        }
    }
}

