using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PodNoms.Data.Models;

namespace PodNoms.Common.Persistence.Repositories {
    public interface IChatRepository : IRepository<ChatMessage> {
        Task<IEnumerable<ChatMessage>> GetSentChats(string fromUserId);
        Task<IEnumerable<ChatMessage>> GetReceivedChats(string fromUserId);
        Task<IEnumerable<ChatMessage>> GetChats(string fromUserId, string toUserId, int take = 10);
        Task<IEnumerable<ChatMessage>> GetAllChats(string userId);
    }


    internal class ChatRepository : GenericRepository<ChatMessage>, IChatRepository {
        public ChatRepository(PodNomsDbContext context, ILogger logger) : base(context, logger) {
        }

        public async Task<IEnumerable<ChatMessage>> GetAllChats(string userId) {
            var chats = await GetAll()
                .Where(c => (c.FromUser.Id == userId || c.ToUser.Id == userId))
                .Include(c => c.FromUser)
                .Include(c => c.ToUser)
                .ToListAsync();

            return chats;
        }

        public async Task<IEnumerable<ChatMessage>> GetChats(string fromUserId, string toUserId, int take = 10) {
            var chats = await GetAll()
                .Where(c => (c.FromUser.Id == fromUserId && c.ToUser.Id == toUserId) ||
                            (c.FromUser.Id == toUserId && c.ToUser.Id == fromUserId))
                .Include(c => c.FromUser)
                .Include(c => c.ToUser)
                .OrderByDescending(c => c.CreateDate)
                .Take(take)
                .ToListAsync();
            return chats;
        }

        public async Task<IEnumerable<ChatMessage>> GetReceivedChats(string toUserId) {
            var chats = await GetAll()
                .Where(c => c.ToUser.Id == toUserId)
                .Include(c => c.FromUser)
                .Include(c => c.ToUser)
                .ToListAsync();

            return chats;
        }

        public async Task<IEnumerable<ChatMessage>> GetSentChats(string fromUserId) {
            var chats = await GetAll()
                .Where(c => c.FromUser.Id == fromUserId)
                .Include(c => c.FromUser)
                .Include(c => c.ToUser)
                .ToListAsync();

            return chats;
        }
    }
}
