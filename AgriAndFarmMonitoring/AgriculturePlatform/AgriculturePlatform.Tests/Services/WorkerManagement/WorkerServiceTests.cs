// AgriculturePlatform.Tests/Services/WorkerManagement/WorkerServiceTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Exceptions;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.Auth;

public class WorkerServiceTests
{
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IFarmRepository> _farmRepositoryMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;  // ✅ ADD THIS
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly IConfiguration _configuration;
    private readonly WorkerAuthService _workerAuthService;

    public WorkerServiceTests()
    {
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();  
        _auditLogServiceMock = new Mock<IAuditLogService>();
        
        // Setup configuration with proper key length (at least 32 characters)
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", TestHelper.TestJwtSecretKey},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:AccessTokenExpiryMinutes", "15"},  
            {"JwtSettings:RefreshTokenExpiryDays", "7"}     
        };
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        _workerAuthService = new WorkerAuthService(
            _workerRepositoryMock.Object,
            _farmRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,  
            _auditLogServiceMock.Object,
            _configuration);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new WorkerLoginDto
        {
            Email = "worker@test.com",
            Password = "WorkerPass123"
        };
        string ipAddress = "127.0.0.1";
        
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        worker.Email = loginDto.Email;
        worker.PasswordHash = TestHelper.HashPassword(loginDto.Password);  // Use BCrypt? Consider using BCrypt
        worker.IsActive = true;
        
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = true };
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(worker);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(worker.FarmId))
            .ReturnsAsync(farm);
        _refreshTokenRepositoryMock.Setup(r => r.RevokeAllWorkerTokensAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Id = 1, Token = "refresh-token", ExpiryDate = DateTime.UtcNow.AddDays(7) });
        _workerRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Worker>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workerAuthService.LoginAsync(loginDto, ipAddress);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();  
        result.RefreshToken.Should().NotBeNullOrEmpty(); 
        result.Email.Should().Be(worker.Email);
    }
}