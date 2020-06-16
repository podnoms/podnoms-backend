using System;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using CodeHollow.FeedReader.Feeds;
using CodeHollow.FeedReader.Feeds.Itunes;
using PodNoms.Common.Data.ViewModels.RssViewModels;
using PodNoms.Common.Utils.Extensions;

namespace PodNoms.Common.Services.Rss {
    public class RssFeedParser {
        public async Task<PodcastEnclosureViewModel> ParseRssFeed(string url) {
            var rss = await FeedReader.ReadAsync(url);
            if (rss.Type == FeedType.Rss_2_0) {
                var feed = (CodeHollow.FeedReader.Feeds.Rss20Feed)rss.SpecificFeed;
                var podcastFeed = rss.GetItunesChannel();

                var ret = new PodcastEnclosureViewModel {
                    Title = feed.Title,
                    Description = feed.Description,
                    Author = podcastFeed.Author,
                    Category = feed.Categories.Join("\n"),
                    Link = feed.Link,
                    Image = feed.Image.Link ?? podcastFeed.Image.Href,
                    PublishDate = feed.PublishingDateString,
                    Language = feed.Language,
                    Copyright = feed.Copyright,
                    Owner = podcastFeed.Owner.Name,
                    OwnerEmail = podcastFeed.Owner.Email,
                    Items = feed.Items
                        .Cast<Rss20FeedItem>()
                        .Select(item => new PodcastEnclosureItemViewModel {
                            Uid = item.Guid,
                            Title = item.Title,
                            Description = item.Description,
                            Author = item.Author ?? "",
                            UpdateDate = item.PublishingDateString,
                            AudioUrl = item.Enclosure.Url,
                            // Duration = item.Id,
                        }).ToList()
                };
                return ret;
            }
            throw new InvalidOperationException("This is not a podcast RSS feed.");
        }
    }
}
