// Application/Interfaces/IWorkerAuthService.cs
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.DTOs.Worker;

namespace AgriculturePlatform.Application.Interfaces;

public interface IWorkerAuthService
{
    Task<WorkerAuthResponseDto> LoginAsync(WorkerLoginDto dto, string ipAddress);
    Task<WorkerAuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, string ipAddress);
    Task<bool> RevokeTokenAsync(RevokeTokenDto dto, string ipAddress);
    Task<bool> RevokeAllUserTokensAsync(int workerId, string ipAddress);
}