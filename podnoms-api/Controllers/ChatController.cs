using System.Threading.Tasks;
using AutoMapper;
using Lib.Net.Http.WebPush;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;
using PodNoms.Common;
using PodNoms.Common.Auth;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services;
using WebPush = Lib.Net.Http.WebPush;

namespace PodNoms.Api.Controllers {
    [Authorize]
    public class ChatController : BaseAuthController {
        private readonly ISupportChatService _supportChatService;
        private readonly IChatRepository _chatRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatController(IHttpContextAccessor contextAccessor, UserManager<ApplicationUser> userManager, ILogger<ChatController> logger,
                            IMapper mapper, IUnitOfWork unitOfWork, IChatRepository chatRepository, ISupportChatService supportChatService) :
            base(contextAccessor, userManager, logger) {
            _chatRepository = chatRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _supportChatService = supportChatService;
        }

        [HttpGet]
        public async Task<ActionResult<ChatViewModel>> Get() {
            var chats = await _chatRepository.GetAllChats(_userId);
            var response = _mapper.Map<ChatViewModel>(chats);
            return Ok(response);
        }
        [HttpPost]
        public async Task<ActionResult<ChatViewModel>> Post([FromBody]ChatViewModel message) {
            //need to lookup the current support host and notify them
            message.FromUserName = _applicationUser.GetBestGuessName();
            message.FromUserId = _applicationUser.Id;
            var chat = _mapper.Map<ChatMessage>(message);
            _chatRepository.AddOrUpdate(chat);
            await _unitOfWork.CompleteAsync();

            if (await _supportChatService.InitiateSupportRequest(_userId, message)) {
                return Ok(message);
            }
            return Accepted(message);
        }
    }
}
