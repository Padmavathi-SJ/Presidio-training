// AgriculturePlatform.Tests/Services/Auth/AdminServiceTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using AgriculturePlatform.Application.DTOs.Admin;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.Auth;

public class AdminServiceTests
{
    private readonly Mock<IAdminRepository> _adminRepositoryMock;
    private readonly Mock<IFarmRepository> _farmRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _adminRepositoryMock = new Mock<IAdminRepository>();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        
        // AdminService constructor takes IAdminRepository, IFarmRepository, and IJwtService (3 arguments)
        _adminService = new AdminService(
            _adminRepositoryMock.Object,
            _farmRepositoryMock.Object,
            _jwtServiceMock.Object);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@test.com",
            Password = "Password123"
        };
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.Email = loginDto.Email;
        admin.PasswordHash = TestHelper.HashPassword(loginDto.Password);
        
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = true };
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(admin);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(admin.FarmId))
            .ReturnsAsync(farm);
        _jwtServiceMock.Setup(j => j.GenerateToken(admin))
            .Returns("test-token");
        _jwtServiceMock.Setup(j => j.GetExpiryDate())
            .Returns(DateTime.UtcNow.AddDays(7));

        // Act
        var result = await _adminService.LoginAsync(loginDto);

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().Be("test-token");
        result.Email.Should().Be(admin.Email);
        result.Name.Should().Be(admin.Name);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "wrong@test.com",
            Password = "Password123"
        };
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((Admin?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@test.com",
            Password = "WrongPassword"
        };
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.Email = loginDto.Email;
        admin.PasswordHash = TestHelper.HashPassword("Password123");
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(admin);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.LoginAsync(loginDto));
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@test.com",
            Password = "Password123"
        };
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.Email = loginDto.Email;
        admin.PasswordHash = TestHelper.HashPassword(loginDto.Password);
        admin.IsActive = false;
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(admin);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.LoginAsync(loginDto));
    }
}