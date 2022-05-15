using System.Threading.Tasks;
using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Resolvers {
    internal class PodcastAuthPasswordResolver : IValueResolver<PodcastViewModel, Podcast, byte[]> {
        private readonly IRepoAccessor _repo;

        public PodcastAuthPasswordResolver(IRepoAccessor repo) {
            _repo = repo;
        }

        public byte[] Resolve(PodcastViewModel source, Podcast destination, byte[] destMember,
            ResolutionContext context) {
            byte[] ret;
            if (string.IsNullOrEmpty(source.AuthPassword) || source.AuthPassword.Equals("**********")) {
                try {
                    using var t = _repo.Podcasts.GetReadOnlyAsync(source.Id);
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
