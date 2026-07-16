using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturePlatform.Domain.Entities.AI;

namespace AgriculturePlatform.Application.Interfaces
{
    public interface IChatRepository
    {
        Task<ChatSession?> GetSessionAsync(string sessionId);
        Task<ChatSession> CreateSessionAsync(ChatSession session);
        Task SaveMessageAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(string sessionId);
    }
}
