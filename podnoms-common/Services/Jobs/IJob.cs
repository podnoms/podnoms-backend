using System.Threading.Tasks;

namespace PodNoms.Common.Services.Jobs {
    public interface IJob {
        Task<bool> Execute();
    }
}