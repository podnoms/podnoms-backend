using System;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;
using PodNoms.Common.Services.Push.Data;

namespace PodNoms.Common.Services.Push {
    public interface IPushSubscriptionStore {
        Task<string> StoreSubscriptionAsync(string uid, PushSubscription subscription);
        Task ForEachSubscriptionAsync(string uid, Action<PushSubscription> action);
        Task ForEachSubscriptionAsync(Action<PushSubscription> action);
        PushSubscriptionContext GetContext();
    }
}
