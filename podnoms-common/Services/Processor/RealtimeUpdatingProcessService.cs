using System;
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
        protected readonly ILogger _logger;

        private readonly IRealTimeUpdater _realtime;
        private readonly IMapper _mapper;

        protected RealtimeUpdatingProcessService(ILogger logger, IRealTimeUpdater realtimeUpdater, IMapper mapper) {
            _logger = logger;
            _realtime = realtimeUpdater;
            _mapper = mapper;
        }

        protected async Task<bool> _sendProgressUpdate(string authToken, string channelName, ProcessingProgress data) {
            return await _realtime.SendProcessUpdate(
                authToken,
                channelName,
                data);
        }
    }
}
