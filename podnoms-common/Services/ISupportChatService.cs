using System.Threading.Tasks;
using PodNoms.Data.Models.ViewModels;

namespace PodNoms.Common.Services {
    public interface ISupportChatService {
        Task<bool> InitiateSupportRequest(string fromUser, ChatViewModel message);
    }
}
