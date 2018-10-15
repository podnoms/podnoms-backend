using System.Text;
using Microsoft.AspNetCore.Mvc;

namespace PodNoms.Api.Controllers {
    public class HomeController : Controller {
        [Route("robots.txt", Name = "GetRobotsText")]
        [Route("rss/robots.txt", Name = "GetRobotsTextRss")]
        [HttpGet]
        public ContentResult RobotsText() {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("user-agent: *");
            stringBuilder.AppendLine("disallow: /error/");
            stringBuilder.AppendLine("allow: /error/foo");

            return this.Content(stringBuilder.ToString(), "text/plain", Encoding.UTF8);
        }
    }
}
