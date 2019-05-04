using System.Threading.Tasks;
using PodNoms.Common.Data.ViewModels.Resources;

namespace PodNoms.Common.Services {
    public interface ISupportChatService {
        Task<bool> InitiateSupportRequest(string fromUser, ChatViewModel message);
    }
}
