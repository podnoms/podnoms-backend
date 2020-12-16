using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Hosting;

namespace PodNoms.Common.Data.Settings {
    public class AppSettings {
        public string ApiUrl { get; set; }
        public string RealtimeUrl { get; set; }
        public string SiteUrl { get; set; }
        public string DownloadUrl { get; set; }
        public string AudioUrl { get; set; }
        public string CanonicalRssUrl { get; set; }
        public string RssUrl { get; set; }
        public string PagesUrl { get; set; }
        public string JobServerUrl { get; set; }
        public string IPStackKey { get; set; }
        public string Downloader { get; set; }

        public string[] GoogleApiKeys { get; set; }
    }
}
