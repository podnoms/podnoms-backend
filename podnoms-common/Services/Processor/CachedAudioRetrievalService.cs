using System;
using System.IO;
using AutoMapper;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Threading.Tasks;
using PodNoms.Common.Persistence;
using PodNoms.Common.Persistence.Repositories;
using PodNoms.Common.Services.Realtime;
using PodNoms.Data.Enums;
using PodNoms.Common.Data.ViewModels;

namespace PodNoms.Common.Services.Processor {
    public class CachedAudioRetrievalService : RealtimeUpdatingProcessService {
        private readonly IEntryRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;

        public CachedAudioRetrievalService(IEntryRepository repository, IUnitOfWork unitOfWork,
            ILogger<AudioUploadProcessService> logger,
            IHttpClientFactory httpClientFactory,
            IRealTimeUpdater realtimeUpdater, IMapper mapper)
            : base(logger, realtimeUpdater, mapper) {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<string> RetrieveAudio(string authToken, Guid entryId, string remoteUrl, string extension) {
            var entry = await _repository.GetAsync(entryId);
            await _sendProgressUpdate(
                authToken,
                entry.Id.ToString(),
                new ProcessingProgress(entry) {
                    ProcessingStatus = ProcessingStatus.Converting,
                    Progress = "Retrieving cached file"
                });

            string cacheFile = Path.Combine(
                    Path.GetTempPath(),
                    $"{System.Guid.NewGuid().ToString()}.{extension}"
                );
            using (var client = _httpClientFactory.CreateClient("podnoms")) {
                using (HttpResponseMessage response = await client.GetAsync(remoteUrl)) {
                    using (Stream streamToReadFrom = await response.Content.ReadAsStreamAsync()) {
                        using (Stream streamToWriteTo = File.Open(cacheFile, FileMode.Create)) {
                            await streamToReadFrom.CopyToAsync(streamToWriteTo);
                        }
                        response.Content = null;
                    }
                }
            }
            await _sendProgressUpdate(
            authToken,
            entry.Id.ToString(),
            new ProcessingProgress(entry) {
                ProcessingStatus = ProcessingStatus.Converting,
                Progress = "Retrieved cached file"
            });
            return cacheFile;
        }
    }
}
