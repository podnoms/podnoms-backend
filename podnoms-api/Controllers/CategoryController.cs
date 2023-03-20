using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using PodNoms.Data.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class CategoryController : BaseAuthController {
        private readonly IRepoAccessor _repo;
        private readonly IMapper _mapper;

        public CategoryController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            ILogger<CategoryController> logger, IRepoAccessor repo, IMapper mapper)
            : base(contextAccessor, userManager, logger) {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryViewModel>>> Get() {
            var response = await _repo.Categories.GetAll()
                .Include(c => c.Subcategories)
                .OrderBy(r => r.Description)
                .ToListAsync();
            return Ok(_mapper.Map<List<Category>, List<CategoryViewModel>>(response));
        }
    }
}
