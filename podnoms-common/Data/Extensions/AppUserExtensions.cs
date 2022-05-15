using System.Linq;
using System.Threading.Tasks;
using HandlebarsDotNet;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Utils;
using PodNoms.Data.Models;

namespace PodNoms.Common.Data.Extensions {
    public static class AppUserExtensions {
        public static async Task<string> GetOpmlFeed(this ApplicationUser user, IRepoAccessor repo, string rssUrl,
            string siteUrl) {
            var podcasts = await repo.Podcasts.GetAllForUserAsync(user.Id);
            var xml = await ResourceReader.ReadResource("opml.xml");

            var template = Handlebars.Compile(xml);
            var model = new {
                title = $"PodNoms OPML feed for {user.GetBestGuessName()}",
                items = podcasts.Select(p => new {
                    title = p.Title,
                    rssUrl = p.GetRssUrl(rssUrl),
                    siteUrl = siteUrl
                })
            };
            var result = template(model);
            return result;
        }
    }
}
