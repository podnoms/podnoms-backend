using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Services.Realtime;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    /// <summary>
    /// Base class for processing services 
    /// which provide realtime updates 
    /// /// </summary>
    public class RealtimeUpdatingProcessService {
        static SemaphoreSlim __lockObj = new SemaphoreSlim(1, 1);

        protected readonly ILogger _logger;

        private readonly IRealTimeUpdater _realtime;
        protected readonly IMapper _mapper;

        protected RealtimeUpdatingProcessService(ILogger logger, IRealTimeUpdater realtimeUpdater, IMapper mapper) {
            _logger = logger;
            _realtime = realtimeUpdater;
            _mapper = mapper;
        }

        protected async Task<bool> _sendProgressUpdate(string authToken, string channelName, ProcessingProgress data) {
            var result = false;
            await __lockObj.WaitAsync();
            try {
                result = await _realtime.SendProcessUpdate(
                    authToken,
                    channelName,
                    data);
            } catch (Exception e) {
                _logger.LogError($"Error in _sendProgressUpdate{Environment.NewLine}{e.Message}");
            } finally {
                __lockObj.Release();
            }
            return result;
        }
    }
}
