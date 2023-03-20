using System.Threading.Tasks;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Data.Models;

namespace PodNoms.Api.Providers {
    internal class PodcastForeignKeyResolver : IValueResolver<PodcastEntryViewModel, PodcastEntry, Podcast> {
        private readonly IRepoAccessor _repo;

        public PodcastForeignKeyResolver(IRepoAccessor repo) {
            _repo = repo;
        }

        public Podcast Resolve(PodcastEntryViewModel source, PodcastEntry destination, Podcast destMember,
            ResolutionContext context) {
            return Task.Run(async () => await _repo.Podcasts.GetAsync(source.PodcastId)).Result;
        }
    }
}
