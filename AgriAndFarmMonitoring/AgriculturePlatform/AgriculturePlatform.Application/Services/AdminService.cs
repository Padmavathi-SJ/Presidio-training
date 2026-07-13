using System.IdentityModel.Tokens.Jwt;
using System.Text;    
using System.Security.Cryptography;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    public AdminService(
        IAdminRepository adminRepository,
        IFarmRepository farmRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IJwtService jwtService,
        IEmailService emailService
        )
    {
        _adminRepository = adminRepository;
        _farmRepository = farmRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _jwtService = jwtService;
        _emailService = emailService;
       
    }

// Application/Services/AdminService.cs
// Update the RegisterAsync method

public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(dto.FarmName))
        throw new BadRequestException("Farm name is required");

    if (string.IsNullOrWhiteSpace(dto.AdminEmail) || string.IsNullOrWhiteSpace(dto.AdminPassword))
        throw new BadRequestException("Admin email and password are required");

    // Check if farm email already exists
    if (await _farmRepository.EmailExistsAsync(dto.FarmEmail))
        throw new BadRequestException("Farm email already registered");

    // Check if admin email already exists
    if (await _adminRepository.EmailExistsAsync(dto.AdminEmail))
        throw new BadRequestException("Admin email already registered");

    // Create Farm
    var farm = new Farm
    {
        FarmName = dto.FarmName.Trim(),
        Email = dto.FarmEmail.Trim().ToLower(),
        Phone = dto.FarmPhone?.Trim(),
        Address = dto.FarmAddress?.Trim(),
        City = dto.FarmCity?.Trim(),
        State = dto.FarmState?.Trim(),
        Country = dto.FarmCountry?.Trim(),
        PostalCode = dto.FarmPostalCode?.Trim(),
        TotalLandHectares = dto.TotalLandHectares,
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    var createdFarm = await _farmRepository.CreateAsync(farm);

    // Create Admin - ✅ Set Role = "Admin"
    var admin = new Admin
    {
        Name = dto.AdminName.Trim(),
        Email = dto.AdminEmail.Trim().ToLower(),
        PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.AdminPassword),
        Phone = dto.AdminPhone?.Trim(),
        FarmId = createdFarm.Id,
        Role = "Admin",  // ✅ Set role explicitly
        IsActive = true,
        CreatedAt = DateTime.UtcNow
    };

    var createdAdmin = await _adminRepository.CreateAsync(admin);

    // Generate tokens
    var accessToken = _jwtService.GenerateAccessToken(createdAdmin);
    var refreshTokenValue = _jwtService.GenerateRefreshToken();
    var jwtId = Guid.NewGuid().ToString();

    // Store refresh token
    var refreshToken = new RefreshToken
    {
        AdminId = createdAdmin.Id,
        Token = refreshTokenValue,
        JwtId = jwtId,
        ExpiryDate = DateTime.UtcNow.AddDays(7),
        CreatedByIp = "127.0.0.1",
        IsUsed = false,
        IsRevoked = false
    };

    await _refreshTokenRepository.CreateAsync(refreshToken);

    return new AuthResponseDto
    {
        Id = createdAdmin.Id,
        Name = createdAdmin.Name,
        Email = createdAdmin.Email,
        AccessToken = accessToken,
        RefreshToken = refreshTokenValue,
        FarmId = createdAdmin.FarmId,
        FarmName = createdFarm.FarmName,
        Role = createdAdmin.Role ?? "Admin",
        AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiryDate(),
        RefreshTokenExpiresAt = refreshToken.ExpiryDate
    };
}
    public async Task<AuthResponseDto> LoginAsync(LoginDto dto, string ipAddress)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new BadRequestException("Email and password are required");

        // Get admin by email
        var admin = await _adminRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (admin == null)
            throw new UnauthorizedException("Invalid email or password");

        // Check if active
        if (!admin.IsActive)
            throw new UnauthorizedException("Account is deactivated. Please contact support.");

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(dto.Password, admin.PasswordHash))
            throw new UnauthorizedException("Invalid email or password");

        // Get farm details
        var farm = await _farmRepository.GetByIdAsync(admin.FarmId);
        if (farm == null || !farm.IsActive)
            throw new UnauthorizedException("Farm is not active. Please contact support.");

        // Revoke all existing refresh tokens for this admin
        await _refreshTokenRepository.RevokeAllUserTokensAsync(admin.Id, ipAddress);

        // Update last login
        admin.LastLogin = DateTime.UtcNow;
        await _adminRepository.UpdateAsync(admin);

        // Generate new tokens
        var accessToken = _jwtService.GenerateAccessToken(admin);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();
        var jwtId = Guid.NewGuid().ToString();

        // Store new refresh token
        var refreshToken = new RefreshToken
        {
            AdminId = admin.Id,
            Token = refreshTokenValue,
            JwtId = jwtId,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress,
            IsUsed = false,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);



        return new AuthResponseDto
        {
            Id = admin.Id,
            Name = admin.Name,
            Email = admin.Email,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            FarmId = admin.FarmId,
            FarmName = farm.FarmName,
            Role = admin.Role ?? "Admin", 
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiryDate(),
            RefreshTokenExpiresAt = refreshToken.ExpiryDate
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, string ipAddress)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.AccessToken) || string.IsNullOrWhiteSpace(dto.RefreshToken))
            throw new BadRequestException("Access token and refresh token are required");

        // Get principal from expired access token
        var principal = _jwtService.GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal == null)
            throw new UnauthorizedException("Invalid access token");

        // FIXED: Add using System.IdentityModel.Tokens.Jwt at the top
        var adminId = int.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
        var jwtId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (adminId == 0 || string.IsNullOrEmpty(jwtId))
            throw new UnauthorizedException("Invalid token claims");

        // Get refresh token from database
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);
        if (refreshToken == null)
            throw new UnauthorizedException("Invalid refresh token");

        // Validate refresh token
        if (refreshToken.IsRevoked)
            throw new UnauthorizedException("Refresh token has been revoked");

        if (refreshToken.IsUsed)
            throw new UnauthorizedException("Refresh token has already been used");

        if (refreshToken.ExpiryDate < DateTime.UtcNow)
            throw new UnauthorizedException("Refresh token has expired");

        if (refreshToken.AdminId != adminId)
            throw new UnauthorizedException("Token does not match user");

        if (refreshToken.JwtId != jwtId)
            throw new UnauthorizedException("Token does not match");

        // Mark current refresh token as used
        refreshToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        // Get admin details
        var admin = await _adminRepository.GetByIdAsync(adminId);
        if (admin == null || !admin.IsActive)
            throw new UnauthorizedException("Admin not found or inactive");

        var farm = await _farmRepository.GetByIdAsync(admin.FarmId);
        if (farm == null || !farm.IsActive)
            throw new UnauthorizedException("Farm is not active");

        // Generate new tokens
        var newAccessToken = _jwtService.GenerateAccessToken(admin);
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();
        var newJwtId = Guid.NewGuid().ToString();

        // Store new refresh token
        var newRefreshToken = new RefreshToken
        {
            AdminId = admin.Id,
            Token = newRefreshTokenValue,
            JwtId = newJwtId,
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            CreatedByIp = ipAddress,
            IsUsed = false,
            IsRevoked = false
        };

        await _refreshTokenRepository.CreateAsync(newRefreshToken);

       
        return new AuthResponseDto
        {
            Id = admin.Id,
            Name = admin.Name,
            Email = admin.Email,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            FarmId = admin.FarmId,
            FarmName = farm.FarmName,
            Role = admin.Role ?? "Admin", 
            AccessTokenExpiresAt = _jwtService.GetAccessTokenExpiryDate(),
            RefreshTokenExpiresAt = newRefreshToken.ExpiryDate
        };
    }

    public async Task<bool> RevokeTokenAsync(RevokeTokenDto dto, string ipAddress)
    {
        if (string.IsNullOrWhiteSpace(dto.RefreshToken))
            throw new BadRequestException("Refresh token is required");

        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(dto.RefreshToken);
        if (refreshToken == null)
            return false;

        if (refreshToken.IsRevoked)
            return false;

        refreshToken.IsRevoked = true;
        refreshToken.RevokedByIp = ipAddress;
        refreshToken.RevokedAt = DateTime.UtcNow;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        return true;
    }

    public async Task<bool> RevokeAllUserTokensAsync(int adminId, string ipAddress)
    {
        var admin = await _adminRepository.GetByIdAsync(adminId);
        if (admin == null)
            return false;

        await _refreshTokenRepository.RevokeAllUserTokensAsync(adminId, ipAddress);
        return true;
    }


    public async Task<bool> ChangePasswordAsync(int adminId, ChangePasswordDto dto, string ipAddress)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword))
            throw new BadRequestException("Current password is required");
        
        if (string.IsNullOrWhiteSpace(dto.NewPassword))
            throw new BadRequestException("New password is required");
        
        if (dto.NewPassword != dto.ConfirmNewPassword)
            throw new BadRequestException("New password and confirmation do not match");
        
        // Get admin by ID
        var admin = await _adminRepository.GetByIdAsync(adminId);
        if (admin == null)
            throw new NotFoundException("Admin not found");
        
        // Verify current password
        if (!VerifyPassword(dto.CurrentPassword, admin.PasswordHash))
            throw new UnauthorizedException("Current password is incorrect");
        
        // Hash new password with BCrypt
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        admin.UpdatedAt = DateTime.UtcNow;
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        
        await _adminRepository.UpdateAsync(admin);
        
        // Revoke all tokens for security
        await _refreshTokenRepository.RevokeAllUserTokensAsync(adminId, ipAddress);

        return true;
    }

    // ==========================================
    // FORGOT PASSWORD
    // ==========================================
    
    public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto dto)
    {
        var admin = await _adminRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (admin == null || !admin.IsActive)
        {
            // Always return true to prevent email enumeration
            return true;
        }

        // Generate 6-char alphanumeric OTP
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var otp = new string(Enumerable.Repeat(chars, 6)
            .Select(s => s[random.Next(s.Length)]).ToArray());

        // Save OTP
        admin.PasswordResetToken = otp;
        admin.PasswordResetExpires = DateTime.UtcNow.AddMinutes(10);
        await _adminRepository.UpdateAsync(admin);

        // Send Email
        var emailDto = new AgriculturePlatform.Application.DTOs.Email.EmailDto
        {
            To = admin.Email,
            ToName = admin.Name,
            Subject = "Password Reset OTP",
            Body = $"<p>Your OTP code is: <strong>{otp}</strong>. It expires in 10 minutes.</p>",
            IsHtml = true
        };

        await _emailService.SendEmailAsync(emailDto);
        return true;
    }

    public async Task<bool> VerifyOtpAsync(VerifyOtpDto dto)
    {
        var admin = await _adminRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (admin == null) return false;

        if (admin.PasswordResetToken != dto.Otp || 
            admin.PasswordResetExpires == null || 
            admin.PasswordResetExpires < DateTime.UtcNow)
        {
            return false;
        }

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordDto dto)
    {
        var admin = await _adminRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
        if (admin == null) throw new BadRequestException("Invalid request");

        if (admin.PasswordResetToken != dto.Otp || 
            admin.PasswordResetExpires == null || 
            admin.PasswordResetExpires < DateTime.UtcNow)
        {
            throw new BadRequestException("Invalid or expired OTP");
        }

        // Update password
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        admin.PasswordResetToken = null;
        admin.PasswordResetExpires = null;
        
        await _adminRepository.UpdateAsync(admin);

        return true;
    }


    private bool VerifyPassword(string password, string? passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash)) return false;
        
        // Check if it's a BCrypt hash (starts with $2a$, $2b$, or $2y$)
        if (passwordHash.StartsWith("$2"))
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch (BCrypt.Net.SaltParseException)
            {
                // If BCrypt fails, try SHA256 as fallback
                return VerifySha256Password(password, passwordHash);
            }
        }
        
        // Try SHA256
        return VerifySha256Password(password, passwordHash);
    }

    private bool VerifySha256Password(string password, string passwordHash)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashOfInput = Convert.ToBase64String(hashedBytes);
        return hashOfInput == passwordHash;
    }


}