// Application/Services/WorkerAuthService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;  
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Services;

public class WorkerAuthService : IWorkerAuthService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _accessTokenExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public WorkerAuthService(
        IWorkerRepository workerRepository,
        IFarmRepository farmRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IAuditLogService auditLogService,
        IConfiguration configuration)
    {
        _workerRepository = workerRepository;
        _farmRepository = farmRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _auditLogService = auditLogService;
        
        _secretKey = configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-minimum-32-characters-long";
        _issuer = configuration["JwtSettings:Issuer"] ?? "AgriculturePlatform";
        _audience = configuration["JwtSettings:Audience"] ?? "AgriculturePlatformClients";
        _accessTokenExpiryMinutes = int.Parse(configuration["JwtSettings:AccessTokenExpiryMinutes"] ?? "15");
        _refreshTokenExpiryDays = int.Parse(configuration["JwtSettings:RefreshTokenExpiryDays"] ?? "7");
    }

    public async Task<WorkerAuthResponseDto> LoginAsync(WorkerLoginDto dto, string ipAddress)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            throw new BadRequestException("Email and password are required");

        // ✅ FIX: Get the worker first WITHOUT farmId
        var worker = await _workerRepository.GetByEmailAsync(dto.Email.Trim().ToLower());
        
        if (worker == null)
            throw new UnauthorizedException("Invalid email or password");

        // ✅ Check if worker is active
        if (!worker.IsActive)
            throw new UnauthorizedException("Account is deactivated. Please contact administrator.");

        // Verify password
        if (!VerifyPassword(dto.Password, worker.PasswordHash))
        {
            await _auditLogService.LogAsync(worker.FarmId, null, worker.Id, 
                "LOGIN_FAILED", "Worker", worker.Id, null, null, ipAddress, null);
            throw new UnauthorizedException("Invalid email or password");
        }

        // Get farm details
        var farm = await _farmRepository.GetByIdAsync(worker.FarmId);
        if (farm == null || !farm.IsActive)
            throw new UnauthorizedException("Farm not found or inactive");

        // Revoke all existing refresh tokens for this worker
        await _refreshTokenRepository.RevokeAllWorkerTokensAsync(worker.Id, ipAddress);

        // Record successful login
        worker.LastLoginAt = DateTime.UtcNow;
        await _workerRepository.UpdateAsync(worker);

        // Generate tokens
        var accessToken = GenerateAccessToken(worker);
        var refreshTokenValue = GenerateRefreshToken();
        var jwtId = Guid.NewGuid().ToString();

        // Store refresh token
        var refreshToken = new RefreshToken
        {
            WorkerId = worker.Id,
            Token = refreshTokenValue,
            JwtId = jwtId,
            ExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedByIp = ipAddress,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);

        await _auditLogService.LogAsync(worker.FarmId, null, worker.Id, 
            "LOGIN_SUCCESS", "Worker", worker.Id, null, null, ipAddress, null);

        return new WorkerAuthResponseDto
        {
            Id = worker.Id,
            Name = worker.Name,
            Email = worker.Email,
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            FarmId = worker.FarmId,
            FarmName = farm.FarmName,
            Role = "Worker", 
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            RefreshTokenExpiresAt = refreshToken.ExpiryDate
        };
    }

    public async Task<WorkerAuthResponseDto> RefreshTokenAsync(RefreshTokenDto dto, string ipAddress)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(dto.AccessToken) || string.IsNullOrWhiteSpace(dto.RefreshToken))
            throw new BadRequestException("Access token and refresh token are required");

        // Get principal from expired access token
        var principal = GetPrincipalFromExpiredToken(dto.AccessToken);
        if (principal == null)
            throw new UnauthorizedException("Invalid access token");

        var workerId = int.Parse(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value ?? "0");
        var jwtId = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

        if (workerId == 0 || string.IsNullOrEmpty(jwtId))
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

        if (refreshToken.WorkerId != workerId)
            throw new UnauthorizedException("Token does not match user");

        if (refreshToken.JwtId != jwtId)
            throw new UnauthorizedException("Token does not match");

        // Mark current refresh token as used
        refreshToken.IsUsed = true;
        await _refreshTokenRepository.UpdateAsync(refreshToken);

        // Get worker details - Get the worker's farmId first
        var worker = await _workerRepository.GetByIdAsync(workerId, 0, false);
        if (worker == null || !worker.IsActive)
            throw new UnauthorizedException("Worker not found or inactive");

        var farm = await _farmRepository.GetByIdAsync(worker.FarmId);
        if (farm == null || !farm.IsActive)
            throw new UnauthorizedException("Farm is not active");

        // Generate new tokens
        var newAccessToken = GenerateAccessToken(worker);
        var newRefreshTokenValue = GenerateRefreshToken();
        var newJwtId = Guid.NewGuid().ToString();

        // Store new refresh token
        var newRefreshToken = new RefreshToken
        {
            WorkerId = worker.Id,
            Token = newRefreshTokenValue,
            JwtId = newJwtId,
            ExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedByIp = ipAddress,
            IsUsed = false,
            IsRevoked = false,
            CreatedAt = DateTime.UtcNow
        };

        await _refreshTokenRepository.CreateAsync(newRefreshToken);

        return new WorkerAuthResponseDto
        {
            Id = worker.Id,
            Name = worker.Name,
            Email = worker.Email,
            AccessToken = newAccessToken,
            RefreshToken = newRefreshTokenValue,
            FarmId = worker.FarmId,
            FarmName = farm.FarmName,
            Role = "Worker", 
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
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

    public async Task<bool> RevokeAllUserTokensAsync(int workerId, string ipAddress)
    {
        var worker = await _workerRepository.GetByIdAsync(workerId, 0, false);
        if (worker == null)
            return false;

        await _refreshTokenRepository.RevokeAllWorkerTokensAsync(workerId, ipAddress);
        return true;
    }

    private string GenerateAccessToken(Worker worker)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, worker.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, worker.Email),
            new Claim(JwtRegisteredClaimNames.Name, worker.Name),
            new Claim("farmId", worker.FarmId.ToString()),
            new Claim("workerId", worker.Id.ToString()),
            new Claim("role", "Worker"),
            new Claim("userType", "Worker"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidateIssuer = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey)),
            ValidateLifetime = false,
            ValidIssuer = _issuer,
            ValidAudience = _audience,
            ClockSkew = TimeSpan.Zero
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        tokenHandler.InboundClaimTypeMap.Clear();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                return null;

            return principal;
        }
        catch
        {
            return null;
        }
    }

    private bool VerifyPassword(string password, string? passwordHash)
    {
        if (string.IsNullOrEmpty(passwordHash)) return false;
        
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashOfInput = Convert.ToBase64String(hashedBytes);
        
        return hashOfInput == passwordHash;
    }
}