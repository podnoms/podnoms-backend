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
        public List<string> GoogleApiKeys { get; set; }
        public string IPStackKey { get; set; }
        public string Downloader { get; set; }

        //TODO: This should be a randomiser to cycle through our keys
        // internal string GetGoogleApiKey() => GoogleApiKeys[^1];
        internal string GetGoogleApiKey() {
            try {
                return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT").Equals(
                        Microsoft.Extensions.Hosting.Environments.Development) ?
                    GoogleApiKeys[^1] :
                    GoogleApiKeys.OrderBy(n => Guid.NewGuid()).FirstOrDefault();
            } catch (Exception) {
            }
            return GoogleApiKeys.OrderBy(n => Guid.NewGuid()).FirstOrDefault();
        }
    }
}
