using AgriculturePlatform.Application.DTOs.Admin;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAdminService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress);
    Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, string ipAddress);
    Task<bool> RevokeTokenAsync(RevokeTokenDto dto, string ipAddress);
    Task<bool> RevokeAllUserTokensAsync(int adminId, string ipAddress);
    Task<bool> ChangePasswordAsync(int adminId, ChangePasswordDto dto, string ipAddress);
    Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto);
    Task<bool> VerifyOtpAsync(VerifyOtpDto dto);
    Task<bool> ResetPasswordAsync(ResetPasswordDto dto);
    Task<AgriculturePlatform.Application.Common.ApiResponse<AdminProfileDto>> GetProfileAsync(int adminId);
    Task<AgriculturePlatform.Application.Common.ApiResponse<AdminProfileDto>> UpdateProfileAsync(int adminId, UpdateAdminProfileDto dto);
}