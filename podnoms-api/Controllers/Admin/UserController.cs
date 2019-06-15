using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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

        [HttpGet ("activity")]
        public async Task<ActionResult<PagedResult>> GetUsers (
            [FromQuery] int currentPage, [FromQuery] int pageSize, [FromQuery] string sortBy, [FromQuery] bool ascending = true) {

            _logger.LogDebug ($"Paging results for: currentPage: {currentPage}, pageSize: {pageSize}, sortBy:{sortBy}");
            var query = _userManager.Users.AsQueryable ();

            if (!string.IsNullOrEmpty (sortBy)) {
                if (sortBy.Equals ("name")) {
                    sortBy = "FirstName, LastName";
                }
                var extra = ascending ? string.Empty : " descending";
                query = query
                    .OrderBy ($"{sortBy}{extra}");
            }
            query = query
                .Skip (currentPage - 1)
                .Take (pageSize);

            var results = await query.ToListAsync ();
            // .ThenInclude (podcast => podcast.PodcastEntries)

            var source = _mapper.Map<List<ApplicationUser>, List<UserActivityViewModel>> (results);
            var response = source.AsQueryable ().PageResult (currentPage, pageSize);

            return response;
        }
    }
}
