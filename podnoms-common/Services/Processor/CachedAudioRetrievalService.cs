using System;
using System.IO;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Common.Persistence;
using PodNoms.Common.Services.Realtime;
using PodNoms.Data.Enums;
using Microsoft.Extensions.Options;
using PodNoms.AudioParsing.Helpers;
using PodNoms.AudioParsing.Models;
using PodNoms.Common.Data.Settings;
using PodNoms.Common.Utils.Extensions;

namespace PodNoms.Common.Services.Processor {
    public class CachedAudioRetrievalService : RealtimeUpdatingProcessService {
        private readonly IRepoAccessor _repo;
        private readonly AppSettings _appSettings;
        private readonly HttpClient _httpClient;

        public CachedAudioRetrievalService(IRepoAccessor repo,
            ILogger<CachedAudioRetrievalService> logger,
            IOptions<AppSettings> appSettings,
            IHttpClientFactory httpClientFactory,
            IRealTimeUpdater realtimeUpdater, IMapper mapper)
            : base(logger, realtimeUpdater, mapper) {
            _repo = repo;
            _appSettings = appSettings.Value;
            this._httpClient = httpClientFactory.CreateClient("CachedAudio");
        }


        public async Task<string> RetrieveAudio(string authToken, Guid entryId, string remoteUrl, string extension) {
            var entry = await _repo.Entries.GetAsync(entryId);
            if (entry == null) {
                return string.Empty;
            }

            await _sendProgressUpdate(
                authToken,
                entry.Id.ToString(),
                new ProcessingProgress(entry) {
                    ProcessingStatus = ProcessingStatus.Converting.ToString(),
                    Progress = "Retrieving cached file"
                });
            string cacheFile = PathUtils.GetScopedTempFile(extension);
            _logger.LogInformation("Starting cache of {RemoteUrl} to {CacheFile}", remoteUrl, cacheFile);

            var totalSize = await _httpClient.GetContentSizeAsync(remoteUrl);
            using (HttpResponseMessage response =
                   _httpClient.GetAsync(remoteUrl, HttpCompletionOption.ResponseHeadersRead).Result) {
                response.EnsureSuccessStatusCode();

                await using Stream contentStream = await response.Content.ReadAsStreamAsync(),
                    fileStream = new FileStream(cacheFile, FileMode.Create,
                        FileAccess.Write, FileShare.None, 8192, true);
                var totalRead = 0L;
                var totalReads = 0L;
                var buffer = new byte[8192];
                var isMoreToRead = true;

                do {
                    var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
                    if (read == 0) {
                        isMoreToRead = false;
                    } else {
                        await fileStream.WriteAsync(buffer, 0, read);

                        totalRead += read;
                        totalReads += 1;

                        if (totalReads % 2000 == 0) {
                            Console.WriteLine($"total bytes downloaded so far: {totalRead:n0}");
                            var percentDone = Math.Round(((double)totalRead / (double)totalSize) * 100, 2);
                            await _sendProgressUpdate(
                                authToken,
                                entry.Id.ToString(),
                                new ProcessingProgress(entry) {
                                    ProcessingStatus = ProcessingStatus.Caching.ToString(),
                                    Progress = "Retrieving cached file",
                                    Payload = new TransferProgress {
                                        Percentage = percentDone,
                                        TotalSize = totalSize.ToString()
                                    }
                                });
                            _logger.LogInformation("{PercentDone}% done", percentDone);
                        }
                    }
                } while (isMoreToRead);
            }

            await _sendProgressUpdate(
                authToken,
                entry.Id.ToString(),
                new ProcessingProgress(entry) {
                    ProcessingStatus = ProcessingStatus.Processing.ToString(),
                    Progress = "Retrieved cached file"
                });
            return cacheFile;
        }
    }
}
