#region imports
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PodNoms.Api.Models;
using PodNoms.Api.Models.ViewModels;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;
using PodNoms.Api.Services.Processor;
using PodNoms.Api.Utils.Extensions;
#endregion
namespace PodNoms.Api.Controllers {
    [Authorize]
    [Route("[controller]")]
    public class PodcastController : BaseAuthController {
        private readonly IPodcastRepository _repository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PodcastController(IPodcastRepository repository, IMapper mapper, IUnitOfWork unitOfWork, ILogger<PodcastController> logger,
                    UserManager<ApplicationUser> userManager, IHttpContextAccessor contextAccessor)
            : base(contextAccessor, userManager, logger) {
            this._uow = unitOfWork;
            this._repository = repository;
            this._mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<PodcastViewModel>> Get() {
            var podcasts = await _repository.GetAllForUserAsync(_applicationUser.Id);
            var ret = _mapper.Map<List<Podcast>, List<PodcastViewModel>>(podcasts.ToList());
            return Ok(ret);
        }

        [HttpGet("{slug}")]
        public async Task<IActionResult> GetBySlug(string slug) {
            var podcast = await _repository.GetAsync(_applicationUser.Id, slug);
            if (podcast == null)
                return NotFound();
            return Ok(_mapper.Map<Podcast, PodcastViewModel>(podcast));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PodcastViewModel vm) {
            if (ModelState.IsValid) {
                var item = _mapper.Map<PodcastViewModel, Podcast>(vm);
                item.AppUser = _applicationUser;
                var ret = _repository.AddOrUpdate(item);
                await _uow.CompleteAsync();
                return Ok(_mapper.Map<Podcast, PodcastViewModel>(ret));
            }
            return BadRequest("Invalid podcast model");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(string id, [FromBody] PodcastViewModel vm) {
            if (ModelState.IsValid) {
                var podcast = _mapper.Map<PodcastViewModel, Podcast>(vm);
                if (podcast.AppUser is null)
                    podcast.AppUser = _applicationUser;

                _repository.AddOrUpdate(podcast);
                await _uow.CompleteAsync();
                return Ok(_mapper.Map<Podcast, PodcastViewModel>(podcast));
            }
            return BadRequest("Invalid request data");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id) {
            await this._repository.DeleteAsync(id);
            await _uow.CompleteAsync();
            return Ok();
        }
    }
}
