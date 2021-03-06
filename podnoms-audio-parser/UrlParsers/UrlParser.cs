﻿using System;
using System.Threading.Tasks;
using PodNoms.AudioParsing.Downloaders;

namespace PodNoms.AudioParsing.UrlParsers {
    /// <summary>
    /// Takes a URL and returns our best guess at how to process the URL
    /// </summary>
    public class UrlTypeParser {
        /// <summary>
        /// Takes a URL and determines what type it is is
        /// Currently this will be in descending order of preference
        ///     1. Direct (a URL we can handle directly without relying on other services i.e. link to mp3 file  
        ///     2. YouTube (a YouTube URL)   
        ///     3. YtDl (a non-YouTube URL which can be handled by youtube-dl (this check currently requires a HTTP call))   
        ///     3. PageParser (a URL we need to pass to the page parser to find links on)   
        ///     3. Invalid (Shit out of luck my friend)   
        /// </summary>
        public async Task<UrlType> GetUrlType(string url) {
            //check if direct url
            if (await new DirectAudioParser().IsMatch(url))
                return UrlType.Direct;

            //check if native YouTube URL
            if (await new YouTubeUrlParser().IsMatch(url))
                return UrlType.YouTube;

            //check if youtube-dl compliant URL 
            if (await new YTDLParser().IsMatch(url))
                return UrlType.YtDl;

            return UrlType.PageParser;
        }

        public async Task<IDownloader> GetDownloader(string url) {
            var type = await GetUrlType(url);
            switch (type) {
                case UrlType.Direct:
                    return new DirectDownloader();
                case UrlType.YouTube:
                    return new YouTubeDownloader();
                case UrlType.YtDl:
                    return new YtDlDownloader();
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
