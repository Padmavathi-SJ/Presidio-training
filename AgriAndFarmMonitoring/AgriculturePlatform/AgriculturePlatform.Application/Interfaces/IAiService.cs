using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturePlatform.Domain.Entities.AI;

namespace AgriculturePlatform.Application.Interfaces
{
    public interface IAiService
    {
        Task<string> GetChatCompletionAsync(string message, IEnumerable<ChatMessage> history, string systemPrompt);
        Task<string> AnalyzePlantImageAsync(string base64Image, string cropContext);
        Task<string> AnalyzePlantImageAsTextAsync(string base64Image, string cropContext);
    }
}
