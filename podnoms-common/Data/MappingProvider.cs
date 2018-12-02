using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;
using PodNoms.Data.Models.Notifications;

namespace PodNoms.Common.Data {
    public static class MappingExtensions {
        public static IMappingExpression<TSource, TDestination> Ignore<TSource, TDestination>(
            this IMappingExpression<TSource, TDestination> map,
            Expression<Func<TDestination, object>> selector) {
            map.ForMember(selector, config => config.Ignore());
            return map;
        }
    }

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
                        _options.GetSection("StorageSettings")["CdnUrl"],
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
                        _options.GetSection("StorageSettings")["CdnUrl"],
                        _options.GetSection("ImageFileStorageSettings")["ContainerName"])));

            CreateMap<PodcastEntry, PodcastEntryViewModel>()
                .ForMember(
                    src => src.AudioUrl,
                    e => e.MapFrom(m => 
                        $"{_options.GetSection("StorageSettings")["CdnUrl"]}{m.AudioUrl}"))
                .ForMember(
                    src => src.ImageUrl,
                    e => e.MapFrom(m => 
                        m.ImageUrl.StartsWith("http") ? 
                            m.ImageUrl : 
                            $"{_options.GetSection("StorageSettings")["CdnUrl"]}{m.ImageUrl}"))
                .ForMember(
                    src => src.ThumbnailUrl,
                    e => e.MapFrom(m => 
                        m.ImageUrl.StartsWith("http") ? 
                            m.ImageUrl : 
                            $"{_options.GetSection("StorageSettings")["CdnUrl"]}{_options.GetSection("ImageFileStorageSettings")["ContainerName"]}/entry/cached/{m.Id.ToString()}-32x32.png"))
                .ForMember(
                    src => src.PodcastId,
                    e => e.MapFrom(m => m.Podcast.Id))
                .ForMember(
                    src => src.PodcastSlug,
                    e => e.MapFrom(m => m.Podcast.Slug))
                .ForMember(
                    src => src.PodcastTitle,
                    e => e.MapFrom(m => m.Podcast.Title));

            CreateMap<Category, CategoryViewModel>()
                .ForMember(
                    src => src.Children,
                    e => e.MapFrom(m => m.Subcategories)
                );

            CreateMap<Subcategory, SubcategoryViewModel>();

            CreateMap<ApplicationUser, ProfileViewModel>()
                .ForMember(
                    src => src.ProfileImage,
                    map => map.MapFrom(s => s.PictureUrl));

            CreateMap<BaseNotificationConfig, NotificationConfigViewModel>()
                .ForMember(
                    src => src.Options,
                    map => map.MapFrom(r => r.Options.Select(o => o.Value)));

            CreateMap<Notification, NotificationViewModel>()
                .ForMember(
                    dest => dest.Options,
                    map => map.MapFrom<NotificationOptionsResolver, string>(s =>
                        s.Config)
                );

            CreateMap<ChatMessage, ChatViewModel>();

            //API Resource to Domain

            CreateMap<PodcastViewModel, Podcast>()
                .ForMember(
                    dest => dest.Category,
                    src => src.MapFrom<PodcastCategoryResolver, string>(s => s.Category.Id.ToString()))
                .ForMember(
                    dest => dest.AuthPassword,
                    src => src.MapFrom<PodcastAuthPasswordResolver, string>(s => s.Category.Id.ToString()))
                .ForMember(
                    dest => dest.Slug,
                    opt => opt.Condition(src => (!string.IsNullOrEmpty(src.Slug))))
                .ForMember(
                    dest => dest.Slug,
                    map => map.MapFrom(vm => vm.Slug));

            CreateMap<PodcastEntryViewModel, PodcastEntry>();
            CreateMap<RegistrationViewModel, ApplicationUser>()
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