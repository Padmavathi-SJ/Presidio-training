using System.Threading.Tasks;
using AgriculturePlatform.Application.DTOs.AI;

namespace AgriculturePlatform.Application.Interfaces
{
    public interface IChatService
    {
        Task<ChatResponseDto> ProcessChatAsync(ChatRequestDto request);
    }
}
