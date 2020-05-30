using System.Threading.Tasks;
using PodNoms.Data.Models;

namespace PodNoms.Common.Auth.ApiKeys {
    public interface IGetApiKeyQuery {
        Task<ApplicationUser> Execute(string providedApiKey);
    }
}
