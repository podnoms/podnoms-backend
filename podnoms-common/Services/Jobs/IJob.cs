using System.Threading.Tasks;
using Hangfire.Server;

namespace PodNoms.Common.Services.Jobs {
    public interface IJob {
        Task<bool> Execute();
        Task<bool> Execute(PerformContext context);
    }
}
