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
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers {
    [Route("[controller]")]
    [Authorize]
    [ApiController]
    public class TagsController : BaseAuthController {
        private readonly ITagRepository _repository;
        private readonly IMapper _mapper;

        public TagsController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager,
            ILogger<CategoryController> logger, ITagRepository repository, IMapper mapper)
            : base(contextAccessor, userManager, logger) {
            _repository = repository;
            _mapper = mapper;
        }
        
        [HttpGet]
        public async Task<ActionResult<List<TagViewModel>>> GetTags() {
            var tags = await _repository.GetAll()
                .Take(10)
                .ToListAsync();
            return _mapper.Map<List<EntryTag>, List<TagViewModel>>(tags);
        }
    }
}
