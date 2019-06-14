using System.Collections.Generic;
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
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {

    [Authorize (Roles = "client-admin")]
    [Authorize]
    [Route ("admin/[controller]")]
    [ApiExplorerSettings (IgnoreApi = true)]
    public class UserController : BaseAuthController {

        public PodNomsDbContext _context;
        private readonly IMapper _mapper;
        private readonly RoleManager<IdentityRole> _roleManager;
        public UserController (
                IHttpContextAccessor contextAccessor,
                UserManager<ApplicationUser> userManager,
                ILogger<UserController> logger,
                RoleManager<IdentityRole> roleManager,
                PodNomsDbContext context,
                IMapper mapper,
                IUnitOfWork unitOfWork):
            base (contextAccessor, userManager, logger) {
                _roleManager = roleManager;
                _context = context;
                _mapper = mapper;
            }

        [HttpGet]
        public async Task<ActionResult<List<UserActivityViewModel>>> GetUsers () {
            var users = await _userManager.Users
                .Include (r => r.Podcasts)
                .ThenInclude (podcast => podcast.PodcastEntries)
                .ToListAsync ();
            return _mapper.Map<List<ApplicationUser>, List<UserActivityViewModel>> (users);
        }
    }
}
