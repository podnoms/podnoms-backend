using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class ChatUserImageResolver : IValueResolver<ChatMessage, ChatViewModel, string> {
        private readonly IConfiguration _options;
        private readonly UserManager<ApplicationUser> _userManager;

        public ChatUserImageResolver(IConfiguration options, UserManager<ApplicationUser> userManager) {
            _options = options;
            _userManager = userManager;
        }
        public string Resolve(ChatMessage source, ChatViewModel destination, string destMember, ResolutionContext context) {
            var user = _userManager.FindByIdAsync(source.FromUser.Id.ToString()).Result;

            return user.GetThumbnailUrl(
                _options.GetSection("StorageSettings")["CdnUrl"],
                _options.GetSection("ImageFileStorageSettings")["ContainerName"]);
        }
    }
}
