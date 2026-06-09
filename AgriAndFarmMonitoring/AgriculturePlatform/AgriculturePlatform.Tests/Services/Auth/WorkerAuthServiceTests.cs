// AgriculturePlatform.Tests/Services/Auth/WorkerAuthServiceTests.cs
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

public class WorkerAuthServiceTests
{
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IFarmRepository> _farmRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly IConfiguration _configuration;
    private readonly WorkerAuthService _workerAuthService;

    public WorkerAuthServiceTests()
    {
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        
        // Use the exact 32-character key from TestHelper
        var inMemorySettings = new Dictionary<string, string>
        {
            {"JwtSettings:SecretKey", TestHelper.TestJwtSecretKey},
            {"JwtSettings:Issuer", "TestIssuer"},
            {"JwtSettings:Audience", "TestAudience"},
            {"JwtSettings:ExpiryDays", 7d}
        };
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        
        _workerAuthService = new WorkerAuthService(
            _workerRepositoryMock.Object,
            _farmRepositoryMock.Object,
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
        
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        worker.Email = loginDto.Email;
        worker.PasswordHash = TestHelper.HashPassword(loginDto.Password);
        
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = true };
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(worker);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(worker.FarmId))
            .ReturnsAsync(farm);
        _workerRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Worker>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workerAuthService.LoginAsync(loginDto, "127.0.0.1");

        // Assert
        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(worker.Email);
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new WorkerLoginDto
        {
            Email = "wrong@test.com",
            Password = "WorkerPass123"
        };
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((Worker?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.LoginAsync(loginDto, "127.0.0.1"));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new WorkerLoginDto
        {
            Email = "worker@test.com",
            Password = "WrongPassword"
        };
        
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        worker.Email = loginDto.Email;
        worker.PasswordHash = TestHelper.HashPassword("WorkerPass123");
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(worker);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.LoginAsync(loginDto, "127.0.0.1"));
    }

    [Fact]
    public async Task LoginAsync_InactiveWorker_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new WorkerLoginDto
        {
            Email = "worker@test.com",
            Password = "WorkerPass123"
        };
        
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        worker.IsActive = false;
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(worker);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.LoginAsync(loginDto, "127.0.0.1"));
    }
}