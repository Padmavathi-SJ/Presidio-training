// AgriculturePlatform.Application/Interfaces/IWorkerAuthService.cs
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerAuthService
{
    Task<WorkerAuthResponseDto> LoginAsync(WorkerLoginDto dto, string ipAddress);
}