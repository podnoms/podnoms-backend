
using Microsoft.EntityFrameworkCore;
using PodNoms.Data.ViewModels;

namespace PodNoms.Data.Interfaces {
    public interface IHubNotifyEntity {
        string GetHubMethodName();
        RealtimeEntityUpdateMessage SerialiseForHub();
        string UserIdForRealtime(DbContext context);
    }
}
