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
                try {
                    using var t = _podcastRepository.GetReadOnlyAsync(source.Id);
                    Task.WhenAll(t);
                    ret = t.Result.AuthPassword;
                } catch (System.AggregateException) {
                    ret = null;
                }
            } else {
                ret = System.Text.Encoding.ASCII.GetBytes(source.AuthPassword);
            }
            return ret;
        }
    }
}
