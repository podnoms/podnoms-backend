using System;
using System.Linq;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using PodNoms.Common.Data.ViewModels.RssViewModels;

namespace PodNoms.Common.Services.Rss {
    public class RssFeedParser {
        public async Task<PodcastEnclosureViewModel> ParseRssFeed(string url) {
            var rss = await FeedReader.ReadAsync(url);
            if (rss.Type == FeedType.Rss_2_0) {
                var feed = (CodeHollow.FeedReader.Feeds.Rss20Feed)rss.SpecificFeed;
                var ret = new PodcastEnclosureViewModel {
                    Title = feed.Title,
                    Description = feed.Description,
                    Link = feed.Link,
                    // Image = feed.ImageUrl,
                    // PublishDate = feed.LastUpdatedDateString,
                    Language = feed.Language,
                    Copyright = feed.Copyright,
                    OwnerEmail = feed.Title,
                    Items = feed.Items.Select(item => new PodcastEnclosureItemViewModel {
                        // Uid = item.Id,
                        Title = item.Title,
                        // Description = item.Description,
                        // Author = item.Author,
                        // UpdateDate = item.PublishingDateString,
                        // AudioUrl = item.SpecificItem["enclosure"],
                        // Duration = item.Id,
                    }).ToList()
                };
                return ret;
            }
            throw new InvalidOperationException("This is not a podcast RSS feed.");
        }
    }
}
