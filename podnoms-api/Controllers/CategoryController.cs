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

namespace PodNoms.Api.Controllers;

[Route("[controller]")]
[Authorize]
[ApiController]
public class CategoryController : BaseAuthController {
  private readonly IMapper _mapper;
  private readonly IRepoAccessor _repo;

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
