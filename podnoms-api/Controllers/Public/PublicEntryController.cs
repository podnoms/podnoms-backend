using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/entry")]
    [EnableCors("PublicApiPolicy")]
    public class PublicEntryController : Controller {
        private readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PublicEntryController(IEntryRepository entryRepository,
                                     IUnitOfWork unitOfWork,
                                     IMapper mapper) {
            _entryRepository = entryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet("top100")]
        public async Task<ActionResult<List<PodcastEntryViewModel>>> Top100(string user, string podcast, string entry) {
            var results = await _entryRepository
                .GetAll()
                .Include(e => e.Podcast)
                .OrderByDescending(r => r.CreateDate)
                .Take(100)
                .ToListAsync();

            return _mapper.Map<List<PodcastEntry>, List<PodcastEntryViewModel>>(results);
        }
        [HttpGet("{user}/{podcast}/{entry}")]
        public async Task<ActionResult<PodcastEntryViewModel>> Get(string user, string podcast, string entry) {
            var result = await _entryRepository.GetForUserAndPodcast(user, podcast, entry);

            if (result is null) return NotFound();

            return _mapper.Map<PodcastEntry, PodcastEntryViewModel>(result);
        }

        [HttpPost("comment/{userSlug}/{entrySlug}")]
        public async Task<ActionResult<PodcastEntryCommentViewModel>> AddComment(
                string userSlug, string entrySlug, [FromBody] PodcastEntryCommentViewModel comment) {
            var entry = await _entryRepository
                .GetAll()
                .Where(e => e.Podcast.AppUser.Slug == userSlug && e.Slug == entrySlug)
                .SingleOrDefaultAsync();

            if (entry is null) {
                return BadRequest($"Could not find entry");
            }

            var newComment = new EntryComment() {
                CommentText = comment.Comment,
                FromUser = comment.FromName,
                FromUserEmail = comment.FromEmail
            };
            entry.Comments.Add(newComment);
            await _unitOfWork.CompleteAsync();
            return Ok(comment);
        }
    }
}
