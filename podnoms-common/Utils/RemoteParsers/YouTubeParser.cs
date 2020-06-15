#nullable enable

namespace PodNoms.Common.Utils.RemoteParsers {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::PodNoms.Common.Data.Settings;
    using Google.Apis.Services;
    using Google.Apis.YouTube.v3;
    using Microsoft.Extensions.Options;

    namespace PodNoms.Common.Utils.RemoteParsers {
        public partial class YouTubeParser : IYouTubeParser {
            const string URL_REGEX = @"^(http(s)?:\/\/)?((w){3}.)?youtu(be|.be)?(\.com)?\/.+";
            private readonly AppSettings _settings;
            private YouTubeService youtube;

            public YouTubeParser(IOptions<AppSettings> options) {
                _settings = options.Value;
                youtube = _getYouTubeService();
            }

            private YouTubeService _getYouTubeService() {
                return new YouTubeService(new BaseClientService.Initializer() {
                    ApiKey = _settings.GoogleApiKey,
                    ApplicationName = GetType().ToString()
                });
            }

            public static bool ValidateVideoId(string videoId) {
                if (string.IsNullOrWhiteSpace(videoId))
                    return false;

                // Video IDs are always 11 characters
                if (videoId.Length != 11)
                    return false;

                return !Regex.IsMatch(videoId, @"[^0-9a-zA-Z_\-]");
            }

            public static bool TryParseVideoId(string videoUrl, out string? videoId) {
                videoId = default;

                if (string.IsNullOrWhiteSpace(videoUrl))
                    return false;

                // https://www.youtube.com/watch?v=yIVRs6YSbOM
                var regularMatch = Regex.Match(videoUrl, @"youtube\..+?/watch.*?v=(.*?)(?:&|/|$)").Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(regularMatch) && ValidateVideoId(regularMatch)) {
                    videoId = regularMatch;
                    return true;
                }

                // https://youtu.be/yIVRs6YSbOM
                var shortMatch = Regex.Match(videoUrl, @"youtu\.be/(.*?)(?:\?|&|/|$)").Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(shortMatch) && ValidateVideoId(shortMatch)) {
                    videoId = shortMatch;
                    return true;
                }

                // https://www.youtube.com/embed/yIVRs6YSbOM
                var embedMatch = Regex.Match(videoUrl, @"youtube\..+?/embed/(.*?)(?:\?|&|/|$)").Groups[1].Value;
                if (!string.IsNullOrWhiteSpace(embedMatch) && ValidateVideoId(embedMatch)) {
                    videoId = embedMatch;
                    return true;
                }

                return false;
            }

            public bool ValidateUrl(string url) {
                var regex = new Regex(URL_REGEX);
                var result = regex.Match(url);
                return result.Success;
            }

            public async Task<string> GetVideoId(string url) {
                return await Task.Run(() => {
                    if (TryParseVideoId(url, out var id)) {
                        return id ?? string.Empty;
                    }

                    return string.Empty;
                });
            }

            public Task<string> GetChannelId(string channelName) {
                throw new NotImplementedException();
            }

            public Task<string> GetChannelIdentifier(string url) {
                throw new NotImplementedException();
            }

            public async Task<RemoteVideoInfo?> GetInformation(string url) {
                var videoId = await GetVideoId(url);
                if (!string.IsNullOrEmpty(url)) {
                    var request = this.youtube.Videos.List("snippet");
                    request.Id = videoId;
                    var response = await request.ExecuteAsync();
                    if (response != null) {
                        var snippet = response.Items
                            .Select(r => r.Snippet)
                            .FirstOrDefault();
                        if (snippet != null) {
                            return new RemoteVideoInfo {
                                VideoId = videoId,
                                Title = snippet.Title,
                                Description = snippet.Description,
                                Thumbnail = snippet.Thumbnails.High.Url,
                                Uploader = snippet.ChannelTitle,
                                UploadDate = DateTime.Parse(snippet.PublishedAt, null,
                                    System.Globalization.DateTimeStyles.RoundtripKind)
                            };
                        }
                    }
                }

                return null;
            }

            public RemoteUrlType GetUrlType(string url) {
                throw new NotImplementedException();
            }

            public Task<List<ParsedItemResult>> GetPlaylistItems(string url, DateTime cutoffDate, int count = 10) {
                throw new NotImplementedException();
            }

            public async Task<List<ParsedItemResult>> GetPlaylistEntriesForId(string id, int count = 10) {
                var playlistRequest = youtube.PlaylistItems.List("contentDetails");
                playlistRequest.PlaylistId = id;
                playlistRequest.MaxResults = count;
                var playlists = await playlistRequest.ExecuteAsync();
                return playlists.Items
                    .Select(p => new ParsedItemResult {
                        Id = p.ContentDetails.VideoId,
                        Title = p.Snippet.Title,
                        VideoType = "youtube",
                        UploadDate = DateTime.Parse(p.ContentDetails.VideoPublishedAt, null,
                            System.Globalization.DateTimeStyles.RoundtripKind)
                    }).ToList();
            }

            public async Task<List<ParsedItemResult>> GetPlaylistEntriesForChannelName(string channelName,
                string searchType, int nCount = 10) {
                var request = youtube.Channels.List("contentDetails");
                if (searchType.Equals("id"))
                    request.Id = channelName;
                else
                    request.ForUsername = channelName;
                request.MaxResults = 1;
                var resp = await request.ExecuteAsync();
                if (resp.Items.Count == 1) {
                    var uploadListId = resp.Items[0].ContentDetails.RelatedPlaylists.Uploads;
                    if (!string.IsNullOrEmpty(uploadListId)) {
                        return await GetPlaylistEntriesForId(uploadListId, nCount);
                    }
                }

                return new List<ParsedItemResult>();
            }
        }
    }
}
