using System.Threading.Tasks;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class PodcastAuthPasswordResolver : IValueResolver<PodcastViewModel, Podcast, byte[]> {
        private readonly IPodcastRepository _podcastRepository;

        public PodcastAuthPasswordResolver(IPodcastRepository podcastRepository) {
            _podcastRepository = podcastRepository;
        }
        public byte[] Resolve(PodcastViewModel source, Podcast destination, byte[] destMember, ResolutionContext context) {
            byte[] ret;
            if (string.IsNullOrEmpty(source.AuthPassword) || source.AuthPassword.Equals("**********")) {
                var t = _podcastRepository.GetAsync(source.Id);
                Task.WhenAll(t);
                ret = t.Result.AuthPassword;
            } else {
                ret = System.Text.Encoding.ASCII.GetBytes(source.AuthPassword);
            }
            return ret;
        }
    }
}
