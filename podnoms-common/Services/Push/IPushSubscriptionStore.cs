using System;
using System.Threading.Tasks;
using Lib.Net.Http.WebPush;

namespace PodNoms.Common.Services.Push {
    public interface IPushSubscriptionStore {
        Task StoreSubscriptionAsync(string uid, PushSubscription subscription);
        Task ForEachSubscriptionAsync(string uid, Action<PushSubscription> action);
        Task ForEachSubscriptionAsync(Action<PushSubscription> action);
    }
}