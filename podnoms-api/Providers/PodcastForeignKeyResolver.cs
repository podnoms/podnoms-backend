using System.Threading.Tasks;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Api.Providers {
    internal class PodcastForeignKeyResolver : IValueResolver<PodcastEntryViewModel, PodcastEntry, Podcast> {
        private IPodcastRepository _sourceRepository;

        public PodcastForeignKeyResolver(IPodcastRepository sourceRepository) {
            _sourceRepository = sourceRepository;
        }

        public Podcast Resolve(PodcastEntryViewModel source, PodcastEntry destination, Podcast destMember, ResolutionContext context) {
            return Task.Run(async () => await _sourceRepository.GetAsync(source.PodcastId)).Result;
        }
    }
}