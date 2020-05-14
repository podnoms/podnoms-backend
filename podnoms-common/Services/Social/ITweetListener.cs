using System.Threading.Tasks;

namespace PodNoms.Common.Services.Social {
    public interface ITweetListener {
        Task<bool> StartAsync();
        Task StopAsync();
    }
}
