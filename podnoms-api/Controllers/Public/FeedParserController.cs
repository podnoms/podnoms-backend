using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PodNoms.Common.Data.ViewModels.RssViewModels;
using PodNoms.Common.Services.Rss;

namespace PodNoms.Api.Controllers.Public {

    [Route("pub/podcast/feedparser")]
    public class FeedParserController : Controller {

        private readonly RssFeedParser _parser;
        public FeedParserController(RssFeedParser parser) {
            _parser = parser;
        }

        [HttpGet]
        public async Task<ActionResult<PodcastEnclosureViewModel>> GetFeed([FromQuery] string url) {
            var feed = await _parser.ParseRssFeed(url);
            return Ok(feed);
        }
    }
}
