using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Common.Data.Settings;
using PodNoms.Data.Extensions;
using PodNoms.Common.Services;
using PodNoms.Common.Services.Gravatar;
using PodNoms.Common.Services.Storage;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Common.Auth {
    public class PodNomsUserManager : UserManager<ApplicationUser> {
        private readonly GravatarHttpClient _gravatarClient;
        private readonly IFileUtilities _fileUtilities;
        private ImageFileStorageSettings _fileStorageSettings;
        private readonly IMailSender _mailSender;
        private readonly StorageSettings _storageSettings;

        public PodNomsUserManager(IUserStore<ApplicationUser> store, IOptions<IdentityOptions> optionsAccessor,
                    IPasswordHasher<ApplicationUser> passwordHasher, IEnumerable<IUserValidator<ApplicationUser>> userValidators,
                    IEnumerable<IPasswordValidator<ApplicationUser>> passwordValidators, ILookupNormalizer keyNormalizer,
                    IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<ApplicationUser>> logger,
                    [FromServices]GravatarHttpClient gravatarClient,
                    IOptions<StorageSettings> storageSettings,
                    IOptions<ImageFileStorageSettings> fileStorageSettings,
                    IFileUtilities fileUtilities,
                    IMailSender mailSender) :
            base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger) {
            _gravatarClient = gravatarClient;
            _fileUtilities = fileUtilities;
            _fileStorageSettings = fileStorageSettings.Value;
            _mailSender = mailSender;
            _storageSettings = storageSettings.Value;
        }
        public override async Task<IdentityResult> CreateAsync(ApplicationUser user) {
            _checkName(user);
            await _imageify(user);
            var result = await base.CreateAsync(user);
            if (result.Succeeded) {
                try {
                    await _mailSender.SendEmailAsync("fergal.moran@gmail.com", "New user signup", $"{user.Email}\n{user.FirstName} {user.LastName}");
                } catch (Exception) {
                }
            } else {
                Logger.LogError($"Error signing up user: {user.Email}");
                foreach (var error in result.Errors) {
                    Logger.LogError(error.Description);
                }
            }
            return result;
        }
        public override async Task<IdentityResult> UpdateAsync(ApplicationUser user) {
            _checkName(user);
            await _imageify(user);
            return await base.UpdateAsync(user);
        }
        private void _checkName(ApplicationUser user) {
            if (!string.IsNullOrEmpty(user.FirstName)) return;
            
            user.FirstName = "PodNoms";
            user.LastName = "User";
        }

        private async Task _imageify(ApplicationUser user) {
            if (string.IsNullOrEmpty(user.PictureUrl)) {
                var gravatar = await _gravatarClient.GetGravatarImage(user.Email);
                if (!string.IsNullOrEmpty(gravatar)) {
                    user.PictureUrl = gravatar;
                } else {
                    var image = ImageUtils.GetTemporaryImage("profile", 6, "svg");
                    var destImage = $"profile/{user.Id.ToString()}.svg";
                    var result = await _fileUtilities.CopyRemoteFile(
                        "static", $"images/{image}",
                        _fileStorageSettings.ContainerName, destImage);
                    user.PictureUrl = $"{_storageSettings.CdnUrl}{_fileStorageSettings.ContainerName}/{destImage}";
                }
            }
        }

        private void _slugify(ApplicationUser user) {
            if (!string.IsNullOrEmpty(user.Slug)) return;
            
            var name = $"{user.FirstName} {user.LastName}";
            var c = name ?? user.Email?.Split('@')[0] ?? string.Empty;
            if (!string.IsNullOrEmpty(c)) {
                user.Slug = c.Slugify(
                    from u in Users select u.Slug
                );
            }
        }
    }
}