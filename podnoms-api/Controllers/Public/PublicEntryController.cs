using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Akismet;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Controllers.Public {
    [Route("pub/entry")]
    [EnableCors("DefaultCors")]
    [ApiController]
    public class PublicEntryController : ControllerBase {
        private readonly IEntryRepository _entryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly AkismetClient _akismet;
        private readonly IConfiguration _config;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IMapper _mapper;

        public PublicEntryController(IEntryRepository entryRepository,
                                     IUnitOfWork unitOfWork,
                                     AkismetClient akismet,
                                     IConfiguration config,
                                     IHttpContextAccessor contextAccessor,
                                     IMapper mapper) {
            _entryRepository = entryRepository;
            _unitOfWork = unitOfWork;
            _akismet = akismet;
            _config = config;
            _contextAccessor = contextAccessor;
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

        [HttpPost("postcomment/{userSlug}/{entrySlug}")]
        public async Task<ActionResult<PodcastEntryCommentViewModel>> AddComment(
                string userSlug, string entrySlug, [FromBody] PodcastEntryCommentViewModel comment) {

            var entry = await _entryRepository
                .GetAll()
                .Where(e => e.Podcast.AppUser.Slug == userSlug && e.Slug == entrySlug)
                .SingleOrDefaultAsync();

            if (entry is null) {
                return BadRequest($"Could not find entry");
            }

            var spamCheckComment = new AkismetComment {
                Blog = new Uri(_config["SpamFilterSettings:BlogUrl"]),
                CommentAuthorEmail = comment.FromEmail,
                CommentContent = comment.Comment,
                UserIp = _contextAccessor.HttpContext.Connection.RemoteIpAddress.ToString(),
                UserAgent = Request.Headers["User-Agent"]
            };

            comment.IsSpam = (await _akismet.CheckCommentAsync(spamCheckComment)).IsSpam;

            var newComment = new EntryComment() {
                CommentText = comment.Comment,
                FromUser = comment.FromName,
                FromUserEmail = comment.FromEmail,
                IsSpam = comment.IsSpam
            };

            entry.Comments.Add(newComment);
            await _unitOfWork.CompleteAsync();
            return Ok(comment);
        }

        [HttpGet("comment/{userSlug}/{entrySlug}")]
        public async Task<ActionResult<List<PodcastEntryCommentViewModel>>> GetComments(string userSlug, string entrySlug) {
            var entry = await _entryRepository
                .GetAll()
                .Include(r => r.Comments)
                .Where(e => e.Podcast.AppUser.Slug == userSlug && e.Slug == entrySlug)
                .SingleOrDefaultAsync();
            if (entry is null) {
                return BadRequest($"Could not find entry");
            }
            return _mapper.Map<List<EntryComment>, List<PodcastEntryCommentViewModel>>(
                entry.Comments
                    .OrderByDescending(c => c.CreateDate)
                    .ToList());
        }
    }
}
