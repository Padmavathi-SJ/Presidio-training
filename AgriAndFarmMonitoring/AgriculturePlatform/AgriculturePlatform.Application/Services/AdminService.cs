// AgriculturePlatform.Application/Services/AdminService.cs
using System.Security.Cryptography;
using System.Text;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Services;

public class AdminService : IAdminService
{
    private readonly IAdminRepository _adminRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IJwtService _jwtService;

    public AdminService(
        IAdminRepository adminRepository, 
        IFarmRepository farmRepository,
        IJwtService jwtService)
    {
        _adminRepository = adminRepository;
        _farmRepository = farmRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        // 1. Check if farm email already exists
        if (await _farmRepository.EmailExistsAsync(dto.FarmEmail))
            throw new BadRequestException("Farm email already registered");

        // 2. Check if admin email already exists
        if (await _adminRepository.EmailExistsAsync(dto.AdminEmail))
            throw new BadRequestException("Admin email already registered");

        // 3. Create Farm FIRST
        var farm = new Farm
        {
            FarmName = dto.FarmName,
            Email = dto.FarmEmail,
            Phone = dto.FarmPhone,
            Address = dto.FarmAddress,
            City = dto.FarmCity,
            State = dto.FarmState,
            Country = dto.FarmCountry,
            PostalCode = dto.FarmPostalCode,
            TotalLandHectares = dto.TotalLandHectares,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdFarm = await _farmRepository.CreateAsync(farm);

        // 4. Create Admin linked to the farm
        var admin = new Admin
        {
            Name = dto.AdminName,
            Email = dto.AdminEmail,
            PasswordHash = HashPassword(dto.AdminPassword),
            Phone = dto.AdminPhone,
            FarmId = createdFarm.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var createdAdmin = await _adminRepository.CreateAsync(admin);

        // 5. Generate token
        var token = _jwtService.GenerateToken(createdAdmin);

        return new AuthResponseDto
        {
            Id = createdAdmin.Id,
            Name = createdAdmin.Name,
            Email = createdAdmin.Email,
            Token = token,
            FarmId = createdAdmin.FarmId,
            FarmName = createdFarm.FarmName,
            ExpiresAt = _jwtService.GetExpiryDate()
        };
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        // Get admin by email
        var admin = await _adminRepository.GetByEmailAsync(dto.Email);
        if (admin == null)
            throw new UnauthorizedException("Invalid email or password");

        // Check if active
        if (!admin.IsActive)
            throw new UnauthorizedException("Account is deactivated");

        // Verify password
        if (!VerifyPassword(dto.Password, admin.PasswordHash))
            throw new UnauthorizedException("Invalid email or password");

        // Get farm details
        var farm = await _farmRepository.GetByIdAsync(admin.FarmId);
        var farmName = farm?.FarmName ?? string.Empty;

        // Update last login
        admin.LastLogin = DateTime.UtcNow;
        await _adminRepository.UpdateAsync(admin);

        // Generate token
        var token = _jwtService.GenerateToken(admin);

        return new AuthResponseDto
        {
            Id = admin.Id,
            Name = admin.Name,
            Email = admin.Email,
            Token = token,
            FarmId = admin.FarmId,
            FarmName = farmName,
            ExpiresAt = _jwtService.GetExpiryDate()
        };
    }

    private string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    private bool VerifyPassword(string password, string passwordHash)
    {
        var hashOfInput = HashPassword(password);
        return hashOfInput == passwordHash;
    }
}