using AutoMapper;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data {
    internal class PodcastAuthPasswordResolver : IMemberValueResolver<PodcastViewModel, Podcast, string, byte[]> {
        public byte[] Resolve(PodcastViewModel source, Podcast destination, string sourceMember, byte[] destMember,
            ResolutionContext context) {
            return string.IsNullOrEmpty(source.AuthPassword)
                ? null
                : System.Text.Encoding.ASCII.GetBytes(source.AuthPassword);
        }
    }
}