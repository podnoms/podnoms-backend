using System.Threading.Tasks;

namespace PodNoms.Common.Persistence {
    public interface IUnitOfWork {
        Task<bool> CompleteAsync();
    }
}