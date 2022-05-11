using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using k8s.Models;
using Microsoft.Extensions.Logging;
using PodNoms.AudioParsing.Downloaders;
using PodNoms.AudioParsing.Models;
using PodNoms.AudioParsing.UrlParsers;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Downloader;
using PodNoms.Common.Services.PageParser;
using PodNoms.Common.Services.Realtime;
using PodNoms.Common.Utils;
using PodNoms.Common.Utils.RemoteParsers;
using PodNoms.Data.Enums;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public class UrlParseException : Exception {
        public UrlParseException(string message) : base(message) {
        }
    }

    public class UrlProcessService : RealtimeUpdatingProcessService, IUrlProcessService {
        private readonly IRepoAccessor _repoAccessor;
        private readonly AudioDownloader _downloader;
        private readonly IPageParser _parser;
        private readonly IYouTubeParser _youTubeParser;
        private readonly IEntryRepository _repository;

        public UrlProcessService(
            IEntryRepository repository, IRepoAccessor repoAccessor,
            AudioDownloader downloader,
            IPageParser parser,
            IYouTubeParser youTubeParser,
            ILogger<UrlProcessService> logger, IRealTimeUpdater realtimeUpdater,
            IMapper mapper) : base(logger, realtimeUpdater, mapper) {
            _repository = repository;
            _repoAccessor = repoAccessor;
            _downloader = downloader;
            _parser = parser;
            _youTubeParser = youTubeParser;
        }

        private async Task __downloader_progress(string userId, string uid, ProcessingProgress e) {
            if (!string.IsNullOrEmpty(userId)) {
                await _sendProgressUpdate(
                    userId,
                    uid,
                    e);
            }
        }

        public async Task<RemoteUrlStatus> ValidateUrl(string url, string requesterId, bool deepParse) {
            var urlType = await new UrlTypeParser().GetUrlType(url.Trim());
            if (urlType != UrlType.Invalid) {
                if (urlType.Equals(UrlType.PageParser)) {
                    //urlType will be YtDl even if it's not a YtDl parseable link
                    //as the parser is greedy - so if we didn't get any info above we can pass
                    //the request off to the remote page parser
                    await _parser.Initialise(url);
                    var links = await _parser.GetAllAudioLinks(true);
                    var title = await _parser.GetPageTitle();
                    var image = await _parser.GetHeadTag("og:image");
                    var description = await _parser.GetHeadTag("og:description");

                    if (links.Count != 0) {
                        return new RemoteUrlStatus {
                            Type = RemoteUrlType.ParsedLinks.ToString(),
                            Title = title,
                            Image = image,
                            Description = description,
                            Links = links.GroupBy(r => r.Key) // note to future me
                                .Select(g => g.First()) // these lines dedupe on key - neato!!
                                .Select(r => new RemoteLinkInfo {
                                    Title = r.Value,
                                    Key = r.Key,
                                    Value = r.Value
                                }).ToList()
                        };
                    }
                }
                var downloader = await new UrlTypeParser().GetDownloader(url);
                var info = await downloader.GetVideoInformation(url);
                return new RemoteUrlStatus {
                    Type = urlType switch {
                        UrlType.Direct => RemoteUrlType.SingleItem.ToString(),
                        UrlType.YouTube => RemoteUrlType.SingleItem.ToString(),
                        UrlType.YtDl => RemoteUrlType.SingleItem.ToString(),
                        UrlType.Invalid => RemoteUrlType.Invalid.ToString(),
                        _ => RemoteUrlType.Invalid.ToString()
                    },
                    Title = info?.Title,
                    Image = info?.Thumbnail,
                    Description = info?.Description,
                    Links = new[] {
                        new RemoteLinkInfo {
                            Title = "New Audio link",
                            Key = url,
                            Value = url
                        }
                    }.ToList()
                };
            }

            throw new UrlParseException("Unable to find any audio here");
        }

        public async Task<RemoteUrlStatus> __ValidateUrl(string url, string requesterId, bool deepParse) {
            url = url.Trim();
            if (string.IsNullOrEmpty(url) || !url.ValidateAsUrl()) {
                throw new UrlParseException($"Unable to validate url: {url}");
            }

            var firstPass = await new UrlTypeParser().GetUrlType(url);
            if (firstPass == UrlType.Direct) {
                return new RemoteUrlStatus {
                    Type = RemoteUrlType.SingleItem.ToString(),
                    Title = "",
                    Image = "",
                    Description = "",
                    Links = new[] {
                        new RemoteLinkInfo {
                            Title = "(changeme) New Audio link",
                            Key = url,
                            Value = url
                        }
                    }.ToList()
                };
            }

            var fileType = await _youTubeParser.GetUrlType(url);
            // so at this point - it will be a playlist whether it's a channel, user or a playlist
            _logger.LogInformation($"Validating Url: {url}");
            if (firstPass == UrlType.YtDl && fileType == RemoteUrlType.Invalid) {
                //we have a ytdl URL that can't be parsed using youtube
                fileType = RemoteUrlType.SingleItem;
            } else if (fileType == RemoteUrlType.Invalid) {
                //call on the audio downloader to validate the URL
                //this is kind of a last resort as it spawns a youtube-dl process 
                //and we don't want to call it too often
                fileType = await _downloader.GetInfo(url, requesterId);
            }

            switch (fileType) {
                case RemoteUrlType.Playlist:
                    return new RemoteUrlStatus {
                        Type = fileType.ToString(),
                        Title = "",
                        Image = "",
                        Description = "",
                        Links = new[] {
                            new RemoteLinkInfo {
                                Title = _downloader.Properties?.Title,
                                Key = url,
                                Value = url
                            }
                        }.ToList()
                    };
                case RemoteUrlType.SingleItem: {
                    var videoInfo = await _youTubeParser.GetVideoInformation(url, requesterId);
                    if (videoInfo != null) {
                        return new RemoteUrlStatus {
                            Type = fileType.ToString(),
                            Title = "",
                            Image = "",
                            Description = "",
                            Links = new[] {
                                new RemoteLinkInfo {
                                    Title = videoInfo?.Title ?? "Audio link",
                                    Key = url,
                                    Value = url
                                }
                            }.ToList()
                        };
                    }

                    if (await _downloader.GetInfo(url, requesterId) == RemoteUrlType.SingleItem) {
                        videoInfo = _downloader.Properties;
                    }

                    return new RemoteUrlStatus {
                        Type = fileType.ToString(),
                        Title = "",
                        Image = "",
                        Description = "",
                        Links = new[] {
                            new RemoteLinkInfo {
                                Title = videoInfo?.Title ?? "Audio link",
                                Key = url,
                                Value = url
                            }
                        }.ToList()
                    };
                }
            }

            if (fileType != RemoteUrlType.Invalid || _youTubeParser.ValidateUrl(url)) {
                return new RemoteUrlStatus {
                    Type = fileType.ToString(),
                    Title = _downloader.Properties?.Title,
                    Image = _downloader.Properties?.Thumbnail,
                    Description = _downloader.Properties?.Description,
                    Links = new[] {
                        new RemoteLinkInfo {
                            Title = _downloader.Properties?.Title,
                            Key = url,
                            Value = url
                        }
                    }.ToList()
                };
            }

            if (await _parser.Initialise(url)) {
                try {
                    var title = await _parser.GetPageTitle();
                    var image = await _parser.GetHeadTag("og:image");
                    var description = _parser.GetHeadTag("og:description");

                    var links = await _parser.GetAllAudioLinks(deepParse);
                    if (links.Count == 0 && !deepParse) {
                        links = await _parser.GetAllAudioLinks(true);
                    }


                    if (links.Count > 0) {
                        return new RemoteUrlStatus {
                            Type = RemoteUrlType.ParsedLinks.ToString(),
                            Title = title,
                            Image = image,
                            Description = "",
                            Links = links
                                .GroupBy(r => r.Key) // note to future me
                                .Select(g => g.First()) // these lines dedupe on key - neato!!
                                .Select(r => new RemoteLinkInfo {
                                    Title = "",
                                    Key = r.Key,
                                    Value = r.Value
                                }).ToList()
                        };
                    }
                } catch (TaskCanceledException) {
                    //we timed out scraping
                    throw new UrlParseException($"Timed out scraping {url}");
                }
            } else {
                throw new UrlParseException($"Unable to initialise parser for {url}");
            }

            return new RemoteUrlStatus {
                Type = fileType.ToString(),
                Title = _downloader.Properties?.Title,
                Image = _downloader.Properties?.Thumbnail,
                Description = _downloader.Properties?.Description,
                Links = new[] {
                    new RemoteLinkInfo {
                        Title = _downloader.Properties?.Title,
                        Key = url,
                        Value = url
                    }
                }.ToList()
            };
        }

        public async Task<RemoteUrlType> GetInformation(string entryId, string requesterId) {
            var entry = await _repository.GetAsync(entryId);
            if (entry is null || string.IsNullOrEmpty(entry.SourceUrl)) {
                _logger.LogError("Unable to process item");
                return RemoteUrlType.Invalid;
            }

            if (entry.SourceUrl.EndsWith(".mp3") ||
                entry.SourceUrl.EndsWith(".wav") ||
                entry.SourceUrl.EndsWith(".aiff") ||
                entry.SourceUrl.EndsWith(".aif")) {
                return RemoteUrlType.SingleItem;
            }

            return await GetInformation(entry, requesterId);
        }

        public async Task<RemoteUrlType> GetInformation(PodcastEntry entry, string requesterId) {
            var info = await _downloader.GetInfo(entry.SourceUrl, requesterId);
            if (!string.IsNullOrEmpty(_downloader.Properties?.Title) &&
                string.IsNullOrEmpty(entry.Title)) {
                entry.Title = _downloader.Properties?.Title;
            }

            entry.Description = _downloader.Properties?.Description ?? entry.Description;
            entry.ImageUrl = _downloader.Properties?.Thumbnail ?? entry.ImageUrl;
            entry.ProcessingStatus = ProcessingStatus.Processing;
            try {
                entry.Author = _downloader.Properties?.Uploader;
            } catch (Exception) {
                _logger.LogWarning($"Unable to extract downloader info for: {entry.SourceUrl}");
            }

            return info;
        }

        public async Task<bool> DownloadAudio(Guid entryId, string outputFile) {
            var entry = await _repository.GetAsync(entryId);

            if (entry is null)
                return false;

            try {
                await __downloader_progress(
                    entry.Podcast.AppUser.Id.ToString(),
                    entry.Id.ToString(),
                    new ProcessingProgress(_mapper.Map<PodcastEntry, PodcastEntryViewModel>(entry)) {
                        ProcessingStatus = ProcessingStatus.Processing.ToString()
                    }
                );

                _downloader.DownloadProgress += async (s, e) => {
                    try {
                        await __downloader_progress(
                            entry.Podcast.AppUser.Id.ToString(),
                            entry.Id.ToString(),
                            e
                        );
                    } catch (NullReferenceException nre) {
                        _logger.LogError(nre.Message);
                    }
                };

                var sourceFile = await _downloader.DownloadAudio(
                    entry.Id.ToString(),
                    entry.SourceUrl,
                    entry.Podcast.AppUserId,
                    outputFile);

                if (string.IsNullOrEmpty(sourceFile)) return false;

                entry.ProcessingStatus = ProcessingStatus.Parsing;
                await _repoAccessor.CompleteAsync();

                return true;
            } catch (Exception ex) {
                _logger.LogError($"Entry: {entryId}\n{ex.Message}\n\n\n{ex.StackTrace}");
                entry.ProcessingStatus = ProcessingStatus.Failed;
                entry.ProcessingPayload = ex.Message;
                await _repoAccessor.CompleteAsync();
                await _sendProgressUpdate(
                    entry.Podcast.AppUser.Id,
                    entry.Id.ToString(),
                    new ProcessingProgress(entry) {
                        ProcessingStatus = ProcessingStatus.Failed.ToString()
                    }
                );
            }

            return false;
        }

        public async Task<bool> DownloadAudioV2(string outputId, string url, string outputFile,
            Func<ProcessingProgress, bool> progressCallback) {
            _downloader.DownloadProgress += (s, e) => {
                progressCallback(e);
            };
            var sourceFile = await _downloader.DownloadAudio(outputId, url, string.Empty, outputFile);
            return true;
        }
    }
}
