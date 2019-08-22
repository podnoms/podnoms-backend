using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
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
                    e => e.MapFrom(m => $"{_options.GetSection("AppSettings")["RssUrl"]}{m.AppUser.Slug}/{m.Slug}"))
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
                    e => e.MapFrom(m => string.Empty)
                )
                .ForMember(
                    v => v.ThumbnailUrl,
                    e => e.MapFrom(m => m.GetThumbnailUrl(
                       _options.GetSection("StorageSettings")["ImageUrl"],
                       _options.GetSection("ImageFileStorageSettings")["ContainerName"])));

            CreateMap<PodcastEntry, PodcastEntryViewModel>()
                .ForMember(
                    src => src.AudioUrl,
                    e => e.MapFrom(m =>
                       $"{_options.GetSection("StorageSettings")["CdnUrl"]}{m.AudioUrl}"))
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
                    e => e.MapFrom(m => m.Podcast.Title));

            CreateMap<PodcastEntry, SharingPublicViewModel>()
                .ForMember(
                    src => src.Title,
                    e => e.MapFrom(m => m.Title))
                .ForMember(
                    src => src.AudioUrl,
                    e => e.MapFrom(m => $"{_options.GetSection("StorageSettings")["CdnUrl"]}{m.AudioUrl}"))
                .ForMember(
                    src => src.ImageUrl,
                    e => e.MapFrom(m =>
                       m.GetThumbnailUrl(_options.GetSection("StorageSettings")["CdnUrll"],
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
                    src => src.ProfileImage,
                    map => map.MapFrom(s => s.PictureUrl))
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
                );
                
//NCA3
//                .ForMember(
//                    src => src.Roles,
//                    map => map.MapFrom<UserRolesResolver>()
//                )

            CreateMap<BaseNotificationConfig, NotificationConfigViewModel>()
                .ForMember(
                    src => src.Options,
                    map => map.MapFrom(r => r.Options.Select(o => new NotificationOptionViewModel {
                        Value = o.Value.Value,
                        Key = o.Value.Key,
                        Label = o.Value.Label,
                        Description = o.Value.Description,
                        Required = o.Value.Required,
                        ControlType = o.Value.ControlType
                    })));

            CreateMap<Notification, NotificationViewModel>()
                .ForMember(
                    dest => dest.Options,
                    map => map.MapFrom<NotificationOptionsResolver>()
                );

            CreateMap<ChatMessage, ChatViewModel>();

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

            CreateMap<ChatViewModel, ChatMessage>();
            CreateMap<NotificationViewModel, Notification>()
                .ForMember(
                    dest => dest.Config,
                    map => map.MapFrom(r => JsonConvert.SerializeObject(r.Options))
                );
        }
    }
}
