using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using PodNoms.Api.Models;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;
using System.Threading.Tasks;
using System.Collections.Generic;
using PodNoms.Api.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using PodNoms.Api.Models.ViewModels.Resources;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class CategoryController : BaseAuthController {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            ILogger<CategoryController> logger, ICategoryRepository categoryRepository, IMapper mapper)
            : base(contextAccessor, userManager, logger) {
            _categoryRepository = categoryRepository;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<CategoryViewModel>>> Get() {
            var response = await this._categoryRepository.GetAll()
                .Include(c => c.Subcategories)
                .OrderBy(r => r.Description)
                .ToListAsync();
            return Ok(_mapper.Map<List<Category>, List<CategoryViewModel>>(response));
        }
    }
}
