using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using PodNoms.Api.Models;
using PodNoms.Api.Models.ViewModels;
using PodNoms.Api.Models.ViewModels.Resources;
using PodNoms.Api.Persistence;
using PodNoms.Api.Services.Auth;

namespace PodNoms.Api.Providers {
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
            this._options = options;

            //Domain to API Resource
            CreateMap<Podcast, PodcastViewModel>()
                .ForMember(
                    v => v.RssUrl,
                    e => e.MapFrom(m => $"{this._options.GetSection("AppSettings")["RssUrl"]}{m.AppUser.Slug}/{m.Slug}"))
                .ForMember(
                    v => v.ImageUrl,
                    e => e.MapFrom(m => m.GetImageUrl(
                        this._options.GetSection("StorageSettings")["CdnUrl"],
                        this._options.GetSection("ImageFileStorageSettings")["ContainerName"])))
                .ForMember(
                    v => v.ThumbnailUrl,
                    e => e.MapFrom(m => m.GetThumbnailUrl(
                        this._options.GetSection("StorageSettings")["CdnUrl"],
                        this._options.GetSection("ImageFileStorageSettings")["ContainerName"])));

            CreateMap<PodcastEntry, PodcastEntryViewModel>()
                .ForMember(
                    src => src.AudioUrl,
                    e => e.MapFrom(m => $"{this._options.GetSection("StorageSettings")["CdnUrl"]}{m.AudioUrl}"))
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
                    map => map.MapFrom(r => r.Options.Select(v => new NotificationOptionViewModel<string>(
                                v.Value,
                                v.Key,
                                v.Key,
                                true,
                                1,
                                "textbox"
                    )))
                );
            CreateMap<Notification, NotificationViewModel>()
                .ForMember(
                    dest => dest.Options,
                    map => map.MapFrom(r =>
                        JsonConvert.DeserializeObject<IList<NotificationOptionViewModel<string>>>(r.Config)
                            .Select(v => new NotificationOptionViewModel<string>(
                                v.Value,
                                v.Key,
                                v.Key,
                                true,
                                1,
                                "textbox")
                        )
                    )
                );

            CreateMap<ChatMessage, ChatViewModel>();

            //API Resource to Domain
            CreateMap<PodcastViewModel, Podcast>()
                .ForMember(
                    dest => dest.Category,
                    src => src.ResolveUsing<PodcastCategoryResolver, string>(s => s.Category.Id.ToString())
                );
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