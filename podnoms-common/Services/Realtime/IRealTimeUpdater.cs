using System.Threading.Tasks;

namespace PodNoms.Common.Services.Realtime {
    public interface IRealTimeUpdater {
        Task<bool> SendProcessUpdate(string authToken, string channelName, object data);
    }
}
