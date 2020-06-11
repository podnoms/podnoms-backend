using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PodNoms.Common.Data.Resolvers;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Utils.Extensions;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;
using System;
using System.Linq;

namespace PodNoms.Common.Data {
    public class MappingProvider : Profile {
        private readonly IConfiguration _options;

        public MappingProvider() { }

        public MappingProvider(IConfiguration options) {
            _options = options;

            //Domain to API Resource
            CreateMap<Podcast, PodcastViewModel>()
                .ForMember(
                    v => v.RssUrl,
                    e => e.MapFrom(m => m.GetRssUrl(_options.GetSection("AppSettings")["RssUrl"])))
                .ForMember(
                    v => v.User,
                    e => e.MapFrom(m => m.AppUser.Slug))
                .ForMember(
                    v => v.ImageUrl,
                    e => e.MapFrom(m => m.GetImageUrl(
                       _options.GetSection("StorageSettings")["ImageUrl"],
                       _options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    v => v.Notifications,
                    e => e.MapFrom(m => m.Notifications)
                )
                .ForMember(
                    v => v.AuthPassword,
                    e => e.MapFrom(m => "**********")
                )
                .ForMember(
                    v => v.ThumbnailUrl,
                    e => e.MapFrom(m => m.GetThumbnailUrl(
                       _options.GetSection("StorageSettings")["ImageUrl"],
                       _options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    src => src.PagesUrl,
                    e => e.MapFrom(m => m.GetPagesUrl(_options.GetSection("AppSettings")["PagesUrl"])))
                .ForMember(
                    src => src.LastEntryDate,
                    e => e.MapFrom(m => m.GetLastEntryDate()))
                .ForMember(
                    src => src.PublicTitle,
                    e => e.MapFrom(m => string.IsNullOrEmpty(m.PublicTitle) ? m.Title : m.PublicTitle));

            CreateMap<PodcastEntry, PodcastEntryViewModel>()
                .ForMember(
                    src => src.PcmUrl,
                    e => e.MapFrom(m => m.GetPcmUrl(
                        _options.GetSection("StorageSettings")["CdnUrl"],
                       _options.GetSection("WaveformDataFileStorageSettings")["ContainerName"])))
                .ForMember(
                    src => src.ImageUrl,
                    e => e.MapFrom(m => m.GetImageUrl(
                       _options.GetSection("StorageSettings")["ImageUrl"],
                       _options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    src => src.ThumbnailUrl,
                    e => e.MapFrom(m => m.GetThumbnailUrl(
                       _options.GetSection("StorageSettings")["ImageUrl"],
                       _options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    src => src.AudioUrl,
                    e => e.MapFrom(m => m.GetAudioUrl(_options.GetSection("AppSettings")["AudioUrl"])))
                .ForMember(
                    src => src.ProcessingStatus,
                    e => e.MapFrom(m => m.Processed ? ProcessingStatus.Processed : m.ProcessingStatus))
                .ForMember(
                    src => src.PodcastId,
                    e => e.MapFrom(m => m.Podcast.Id))
                .ForMember(
                    src => src.PodcastSlug,
                    e => e.MapFrom(m => m.Podcast.Slug))
                .ForMember(
                    src => src.PodcastTitle,
                    e => e.MapFrom(m => m.Podcast.Title))
                .ForMember(
                    src => src.PagesUrl,
                    e => e.MapFrom(m => m.GetPagesUrl(_options.GetSection("AppSettings")["PagesUrl"])));

            CreateMap<PodcastEntry, PublicSharingViewModel>()
                .ForMember(
                    src => src.DownloadNonce,
                    e => e.MapFrom(m => System.Guid.NewGuid().ToString()))
                .ForMember(
                    src => src.StrippedDescription,
                    e => e.MapFrom(m => m.Description.RemoveUnwantedHtmlTags()))
                .ForMember(
                    src => src.Title,
                    e => e.MapFrom(m => m.Title))
                .ForMember(
                    src => src.Author,
                    e => e.MapFrom(m => m.Podcast.AppUser.GetBestGuessName()))
                .ForMember(
                    src => src.DownloadUrl,
                    e => e.MapFrom(m => m.GetDownloadUrl(_options.GetSection("AppSettings")["DownloadUrl"])))
                .ForMember(
                    src => src.AudioUrl,
                    e => e.MapFrom(m => m.GetAudioUrl(_options.GetSection("AppSettings")["AudioUrl"])))
                .ForMember(
                    src => src.LargeImageUrl,
                    e => e.MapFrom(m =>
                       m.GetImageUrl(_options.GetSection("StorageSettings")["CdnUrl"],
                           _options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    src => src.ThumbnailUrl,
                    e => e.MapFrom(m =>
                       m.GetThumbnailUrl(_options.GetSection("StorageSettings")["CdnUrl"],
                           _options.GetSection("ImageFileStorageSettings")["ContainerName"])));

            CreateMap<Playlist, PlaylistViewModel>();

            CreateMap<Category, CategoryViewModel>()
                .ForMember(
                    src => src.Children,
                    e => e.MapFrom(m => m.Subcategories)
                );

            CreateMap<Subcategory, SubcategoryViewModel>();

            CreateMap<ApplicationUser, UserActivityViewModel>()
                .ForMember(
                    src => src.Name,
                    map => map.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember(
                    src => src.PodcastCount,
                    map => map.MapFrom(s => s.Podcasts.Count))
                .ForMember(
                    src => src.EntryCount,
                    map => map.MapFrom(s => s.Podcasts.Sum(r => r.PodcastEntries.Count)));

            CreateMap<ApplicationUser, ProfileViewModel>()
                .ForMember(
                    src => src.Name,
                    map => map.MapFrom(s => $"{s.FirstName} {s.LastName}"))
                .ForMember(
                    src => src.ProfileImageUrl,
                    map => map.MapFrom(s => s.GetImageUrl(
                       _options.GetSection("StorageSettings")["ImageUrl"],
                       _options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    src => src.ThumbnailImageUrl,
                    map => map.MapFrom(s => s.GetThumbnailUrl(
                       _options.GetSection("StorageSettings")["ImageUrl"],
                       _options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    src => src.HasSubscribed,
                    map => map.MapFrom<ProfileHasSubscribedResolver>()
                )
                .ForMember(
                    src => src.SubscriptionType,
                    map => map.MapFrom<ProfileSubscriptionResolver>()
                )
                .ForMember(
                    src => src.SubscriptionValid,
                    map => map.MapFrom<ProfileSubscriptionValidResolver>()
                )
                .ForMember(
                    src => src.SubscriptionValidUntil,
                    map => map.MapFrom<ProfileSubscriptionValidUntilResolver>()
                )
                .ForMember(
                    src => src.IsFluent,
                    map => map.MapFrom<ProfileIsFluentResolver>()
                )
               .ForMember(
                   src => src.Roles,
                   map => map.MapFrom<UserRolesResolver>()
               );

            CreateMap<BaseNotificationConfig, NotificationConfigViewModel>()
                .ForMember(
                    src => src.Options,
                    map => map.MapFrom(r => r.Options.Select(
                        o => new NotificationOptionViewModel {
                            Value = o.Value.Value,
                            Key = o.Value.Key,
                            Label = o.Value.Label,
                            Description = o.Value.Description,
                            Required = o.Value.Required,
                            ControlType = o.Value.ControlType
                        }
                    ))
                );

            CreateMap<IssuedApiKey, ApiKeyViewModel>()
                .ForMember(
                    dest => dest.Id,
                    map => map.MapFrom(r => r.Id.ToString())
                )
                .ForMember(
                    dest => dest.DateIssued,
                    map => map.MapFrom(r => r.CreateDate)
                );

            CreateMap<Notification, NotificationViewModel>()
                .ForMember(
                    dest => dest.Options,
                    map => map.MapFrom<NotificationOptionsResolver>()
                );
            CreateMap<ActivityLogPodcastEntry, ActivityLogPodcastEntryViewModel>()
                .ForMember(
                    dest => dest.DateAccessed,
                    map => map.MapFrom(src => src.CreateDate)
                )
                .ForMember(
                    dest => dest.IncomingUrl,
                    map => map.MapFrom(src => src.Referrer)
                )
                .ForMember(
                    dest => dest.IncomingHost,
                    map => map.MapFrom(src => src.UserAgent)
                )
                .ForMember(
                    dest => dest.PodcastSlug,
                    map => map.MapFrom(src => src.PodcastEntry.Slug)
                )
                .ForMember(
                    dest => dest.PodcastTitle,
                    map => map.MapFrom(src => src.PodcastEntry.Title));

            CreateMap<ChatMessage, ChatViewModel>()
            .ForMember(
                    dest => dest.MessageId,
                    src => src.MapFrom(s => s.Id.ToString()))
            .ForMember(
                    dest => dest.MessageDate,
                    src => src.MapFrom(s => s.CreateDate))
            .ForMember(
                    dest => dest.FromUserId,
                    src => src.MapFrom(s => s.FromUser.Id.ToString()))
            .ForMember(
                    dest => dest.ToUserId,
                    src => src.MapFrom(s => s.ToUser.Id.ToString()))
                .ForMember(
                    dest => dest.FromUserImage,
                    src => src.MapFrom<ChatUserImageResolver>())
                .ForMember(
                    dest => dest.FromUserName,
                    src => src.MapFrom(s => s.FromUser.GetBestGuessName()));

            //API Resource to Domain

            CreateMap<PodcastViewModel, Podcast>()
                .ForMember(
                    dest => dest.Category,
                    src => src.MapFrom<PodcastCategoryResolver>())
                .ForMember(
                    dest => dest.AuthPassword,
                    src => src.MapFrom<PodcastAuthPasswordResolver>())
                .ForMember(
                    dest => dest.Slug,
                    opt => opt.Condition(src => (!string.IsNullOrEmpty(src.Slug))))
                .ForMember(
                    dest => dest.Slug,
                    map => map.MapFrom(vm => vm.Slug));

            CreateMap<PodcastEntryViewModel, PodcastEntry>()
                .ForMember(
                    e => e.AudioUrl,
                    map => map.Ignore())
                .AfterMap((src, dest) => dest.AudioUrl = dest.AudioUrl);

            CreateMap<RegistrationViewModel, ApplicationUser>()
                .ForMember(
                    e => e.Slug,
                    map => map.MapFrom(vm => vm.Username))
                .ForMember(
                    e => e.UserName,
                    map => map.MapFrom(vm => vm.Email));

            CreateMap<NotificationViewModel, Notification>()
                .ForMember(
                    dest => dest.Config,
                    map => map.MapFrom(r => JsonConvert.SerializeObject(r.Options))
                );

            CreateMap<ChatViewModel, ChatMessage>();
        }
    }
}
