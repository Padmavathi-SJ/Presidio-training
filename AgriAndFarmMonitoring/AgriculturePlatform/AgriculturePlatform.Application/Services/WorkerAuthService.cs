// AgriculturePlatform.Application/Services/WorkerAuthService.cs
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration; 
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Services;

public class WorkerAuthService : IWorkerAuthService
{
    private readonly IWorkerRepository _workerRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IAuditLogService _auditLogService;
    private readonly string _secretKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expiryDays;  // Keep as int

    public WorkerAuthService(
        IWorkerRepository workerRepository,
        IFarmRepository farmRepository,
        IAuditLogService auditLogService,
        IConfiguration configuration)
    {
        _workerRepository = workerRepository;
        _farmRepository = farmRepository;
        _auditLogService = auditLogService;
        _secretKey = configuration["JwtSettings:SecretKey"] ?? "your-super-secret-key-minimum-32-characters-long";
        _issuer = configuration["JwtSettings:Issuer"] ?? "AgriculturePlatform";
        _audience = configuration["JwtSettings:Audience"] ?? "AgriculturePlatformClients";
        
        // FIX: Parse the string value correctly
        var expiryDaysStr = configuration["JwtSettings:ExpiryDays"];
        _expiryDays = string.IsNullOrEmpty(expiryDaysStr) ? 7 : int.Parse(expiryDaysStr);
    }

    public async Task<WorkerAuthResponseDto> LoginAsync(WorkerLoginDto dto, string ipAddress)
    {
        // 1. Find worker by email
        var worker = await _workerRepository.GetByEmailAsync(dto.Email);
        
        if (worker == null)
        {
            throw new UnauthorizedException("Invalid email or password");
        }

        // 2. Check if worker is active
        if (!worker.IsActive)
        {
            throw new UnauthorizedException("Account is deactivated. Please contact administrator.");
        }

        // 3. Verify password
        if (!VerifyPassword(dto.Password, worker.PasswordHash))
        {
            // Record failed login attempt
            await _auditLogService.LogAsync(worker.FarmId, null, worker.Id, 
                "LOGIN_FAILED", "Worker", worker.Id, null, null, ipAddress, null);
            throw new UnauthorizedException("Invalid email or password");
        }

        // 4. Get farm details
        var farm = await _farmRepository.GetByIdAsync(worker.FarmId);
        if (farm == null || !farm.IsActive)
        {
            throw new UnauthorizedException("Farm not found or inactive");
        }

        // 5. Generate JWT token
        var token = GenerateToken(worker);

        // 6. Record successful login
        worker.LastLoginAt = DateTime.UtcNow;
        await _workerRepository.UpdateAsync(worker);
        
        await _auditLogService.LogAsync(worker.FarmId, null, worker.Id, 
            "LOGIN_SUCCESS", "Worker", worker.Id, null, null, ipAddress, null);

        return new WorkerAuthResponseDto
        {
            Id = worker.Id,
            Name = worker.Name,
            Email = worker.Email,
            Token = token,
            FarmId = worker.FarmId,
            FarmName = farm.FarmName,
            Role = worker.Role ?? "WORKER",
            ExpiresAt = DateTime.UtcNow.AddDays(_expiryDays)  // Use AddDays for days
        };
    }

    private string GenerateToken(Worker worker)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, worker.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, worker.Email),
            new Claim(JwtRegisteredClaimNames.Name, worker.Name),
            new Claim("farmId", worker.FarmId.ToString()),     // Multi-tenancy key
            new Claim("workerId", worker.Id.ToString()),
            new Claim("role", "WORKER"),
            new Claim("userType", "Worker"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(_expiryDays),  // Use AddDays for days
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
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