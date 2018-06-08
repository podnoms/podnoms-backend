using System;
using System.Linq.Expressions;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using PodNoms.Api.Models;
using PodNoms.Api.Models.ViewModels;
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

            CreateMap<ApplicationUser, ProfileViewModel>()
                .ForMember(
                    src => src.ProfileImage,
                    map => map.MapFrom(s => s.PictureUrl));

            CreateMap<ChatMessage, ChatViewModel>();

            //API Resource to Domain
            CreateMap<PodcastViewModel, Podcast>();
            CreateMap<PodcastEntryViewModel, PodcastEntry>();
            CreateMap<RegistrationViewModel, ApplicationUser>()
                .ForMember(
                    e => e.UserName,
                    map => map.MapFrom(vm => vm.Email));
            CreateMap<ChatViewModel, ChatMessage>();
        }
    }
}
