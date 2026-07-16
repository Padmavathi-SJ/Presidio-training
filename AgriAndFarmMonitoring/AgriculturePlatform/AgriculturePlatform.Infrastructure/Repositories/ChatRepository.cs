using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AI;
using AgriculturePlatform.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace AgriculturePlatform.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ChatSession?> GetSessionAsync(string sessionId)
        {
            return await _context.ChatSessions
                .Include(s => s.Messages)
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);
        }

        public async Task<ChatSession> CreateSessionAsync(ChatSession session)
        {
            _context.ChatSessions.Add(session);
            await _context.SaveChangesAsync();
            return session;
        }

        public async Task SaveMessageAsync(ChatMessage message)
        {
            _context.ChatMessages.Add(message);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetSessionMessagesAsync(string sessionId)
        {
            return await _context.ChatMessages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Timestamp)
                .ToListAsync();
        }
    }
}
