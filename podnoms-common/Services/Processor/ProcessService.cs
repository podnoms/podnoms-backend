using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using PodNoms.Common.Data.ViewModels;
using PodNoms.Common.Data.ViewModels.Resources;
using PodNoms.Common.Services.Realtime;
using PodNoms.Data.Models;

namespace PodNoms.Common.Services.Processor {
    public class ProcessService {
        protected readonly ILogger Logger;

        private readonly IRealTimeUpdater _realtime;
        protected ProcessService(ILoggerFactory logger, IRealTimeUpdater realtimeUpdater) {
            Logger = logger.CreateLogger<UrlProcessService>();
            _realtime = realtimeUpdater;
        }
        protected async Task<bool> _sendProcessCompleteMessage(PodcastEntry entry) {
            return await __sendProcessUpdate(entry.Podcast.AppUser.Id, entry.Id.ToString(), "info_processed", 
                new PodcastEntryViewModel());
        }
        protected async Task<bool> _sendProgressUpdate(string userId, string itemUid, ProcessProgressEvent data) {
            return await _realtime.SendProcessUpdate(userId, itemUid, "progress_update", data);
        }

        private async Task<bool> __sendProcessUpdate(string userId, string itemUid, string message, PodcastEntryViewModel data) {
            try {
                return await _realtime.SendProcessUpdate(
                    userId,
                    itemUid,
                    message,
                    data);
            } catch (Exception ex) {
                Logger.LogError(123456, ex, "Error sending realtime message");
            }
            return false;
        }
    }
}