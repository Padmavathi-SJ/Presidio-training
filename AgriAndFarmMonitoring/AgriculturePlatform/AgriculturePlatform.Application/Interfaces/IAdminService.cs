// AgriculturePlatform.Application/Interfaces/IAdminService.cs
using AgriculturePlatform.Application.DTOs.Admin;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAdminService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto);
}