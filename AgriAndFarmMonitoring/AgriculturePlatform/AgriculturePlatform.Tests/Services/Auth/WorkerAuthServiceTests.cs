// AgriculturePlatform.Tests/Services/Auth/WorkerAuthServiceTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using AgriculturePlatform.Application.DTOs.Admin;
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
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly IConfiguration _configuration;
    private readonly WorkerAuthService _workerAuthService;

    public WorkerAuthServiceTests()
    {
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        
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

    // =============================================
    // LOGIN TESTS
    // =============================================

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
        worker.PasswordHash = TestHelper.HashPassword(loginDto.Password);
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
        result.Name.Should().Be(worker.Name);
        result.FarmId.Should().Be(1);
        result.FarmName.Should().Be("Test Farm");
        result.Role.Should().Be("Worker");
    }

    [Fact]
    public async Task LoginAsync_EmptyEmail_ThrowsBadRequestException()
    {
        // Arrange
        var loginDto = new WorkerLoginDto { Email = "", Password = "WorkerPass123" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _workerAuthService.LoginAsync(loginDto, ipAddress));
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ThrowsBadRequestException()
    {
        // Arrange
        var loginDto = new WorkerLoginDto { Email = "worker@test.com", Password = "" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _workerAuthService.LoginAsync(loginDto, ipAddress));
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
        string ipAddress = "127.0.0.1";
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((Worker?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.LoginAsync(loginDto, ipAddress));
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
        string ipAddress = "127.0.0.1";
        
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        worker.Email = loginDto.Email;
        worker.PasswordHash = TestHelper.HashPassword("WorkerPass123");
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(worker);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.LoginAsync(loginDto, ipAddress));
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
        string ipAddress = "127.0.0.1";
        
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        worker.Email = loginDto.Email;
        worker.PasswordHash = TestHelper.HashPassword(loginDto.Password);
        worker.IsActive = false;
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(worker);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.LoginAsync(loginDto, ipAddress));
    }

    [Fact]
    public async Task LoginAsync_InactiveFarm_ThrowsUnauthorizedException()
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
        worker.PasswordHash = TestHelper.HashPassword(loginDto.Password);
        worker.IsActive = true;
        
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = false };
        
        _workerRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(worker);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(worker.FarmId))
            .ReturnsAsync(farm);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.LoginAsync(loginDto, ipAddress));
    }

    // =============================================
    // REFRESH TOKEN TESTS
    // =============================================

    [Fact]
    public async Task RefreshTokenAsync_ValidTokens_ReturnsNewAuthResponse()
    {
        // Arrange
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        worker.IsActive = true;
        var token = GenerateTestToken(worker, "test-jti");
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = token,
            RefreshToken = "valid-refresh-token"
        };
        string ipAddress = "127.0.0.1";
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = true };
        
        // Mock principal from expired token
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, "1"),
            new Claim(JwtRegisteredClaimNames.Jti, "test-jti")
        };
        var identity = new ClaimsIdentity(claims);
        var principal = new ClaimsPrincipal(identity);
        
        var existingRefreshToken = new RefreshToken
        {
            Id = 1,
            WorkerId = 1,
            Token = "valid-refresh-token",
            JwtId = "test-jti",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(existingRefreshToken);
        // FIX: Add farmId parameter (0 means ignore farm check)
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(1, 0, false))
            .ReturnsAsync(worker);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(worker.FarmId))
            .ReturnsAsync(farm);
        _refreshTokenRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Id = 2, Token = "new-refresh-token" });

        // Act
        var result = await _workerAuthService.RefreshTokenAsync(refreshTokenDto, ipAddress);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.Email.Should().Be(worker.Email);
    }

    [Fact]
    public async Task RefreshTokenAsync_MissingAccessToken_ThrowsBadRequestException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto { AccessToken = "", RefreshToken = "token" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _workerAuthService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_MissingRefreshToken_ThrowsBadRequestException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto { AccessToken = "token", RefreshToken = "" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _workerAuthService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "expired-token",
            RefreshToken = "invalid-refresh-token"
        };
        string ipAddress = "127.0.0.1";
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_RevokedRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "expired-token",
            RefreshToken = "revoked-refresh-token"
        };
        string ipAddress = "127.0.0.1";
        
        var existingRefreshToken = new RefreshToken
        {
            Id = 1,
            WorkerId = 1,
            Token = "revoked-refresh-token",
            JwtId = "test-jti",
            IsRevoked = true
        };
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(existingRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_ExpiredRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "expired-token",
            RefreshToken = "expired-refresh-token"
        };
        string ipAddress = "127.0.0.1";
        
        var existingRefreshToken = new RefreshToken
        {
            Id = 1,
            WorkerId = 1,
            Token = "expired-refresh-token",
            JwtId = "test-jti",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsUsed = false,
            IsRevoked = false
        };
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(existingRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_AlreadyUsedRefreshToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "expired-token",
            RefreshToken = "used-refresh-token"
        };
        string ipAddress = "127.0.0.1";
        
        var existingRefreshToken = new RefreshToken
        {
            Id = 1,
            WorkerId = 1,
            Token = "used-refresh-token",
            JwtId = "test-jti",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = true,
            IsRevoked = false
        };
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(existingRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _workerAuthService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    // =============================================
    // REVOKE TOKEN TESTS
    // =============================================

    [Fact]
    public async Task RevokeTokenAsync_ValidToken_ReturnsTrue()
    {
        // Arrange
        var revokeDto = new RevokeTokenDto { RefreshToken = "valid-token" };
        string ipAddress = "127.0.0.1";
        
        var refreshToken = new RefreshToken
        {
            Id = 1,
            Token = "valid-token",
            IsRevoked = false
        };
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(revokeDto.RefreshToken))
            .ReturnsAsync(refreshToken);
        _refreshTokenRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workerAuthService.RevokeTokenAsync(revokeDto, ipAddress);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeTokenAsync_MissingToken_ThrowsBadRequestException()
    {
        // Arrange
        var revokeDto = new RevokeTokenDto { RefreshToken = "" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _workerAuthService.RevokeTokenAsync(revokeDto, ipAddress));
    }

    [Fact]
    public async Task RevokeTokenAsync_TokenNotFound_ReturnsFalse()
    {
        // Arrange
        var revokeDto = new RevokeTokenDto { RefreshToken = "not-found" };
        string ipAddress = "127.0.0.1";
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(revokeDto.RefreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _workerAuthService.RevokeTokenAsync(revokeDto, ipAddress);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task RevokeTokenAsync_AlreadyRevoked_ReturnsFalse()
    {
        // Arrange
        var revokeDto = new RevokeTokenDto { RefreshToken = "already-revoked" };
        string ipAddress = "127.0.0.1";
        
        var refreshToken = new RefreshToken
        {
            Id = 1,
            Token = "already-revoked",
            IsRevoked = true
        };
        
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(revokeDto.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _workerAuthService.RevokeTokenAsync(revokeDto, ipAddress);

        // Assert
        result.Should().BeFalse();
    }

    // =============================================
    // REVOKE ALL USER TOKENS TESTS
    // =============================================

    [Fact]
    public async Task RevokeAllUserTokensAsync_ValidWorker_ReturnsTrue()
    {
        // Arrange
        int workerId = 1;
        string ipAddress = "127.0.0.1";
        
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        
        // FIX: Add farmId parameter
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(workerId, 0, false))
            .ReturnsAsync(worker);
        _refreshTokenRepositoryMock.Setup(r => r.RevokeAllWorkerTokensAsync(workerId, ipAddress))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workerAuthService.RevokeAllUserTokensAsync(workerId, ipAddress);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_WorkerNotFound_ReturnsFalse()
    {
        // Arrange
        int workerId = 999;
        string ipAddress = "127.0.0.1";
        
        // FIX: Add farmId parameter
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(workerId, 0, false))
            .ReturnsAsync((Worker?)null);

        // Act
        var result = await _workerAuthService.RevokeAllUserTokensAsync(workerId, ipAddress);

        // Assert
        result.Should().BeFalse();
    }

    private string GenerateTestToken(Worker worker, string jti = "test-jti")
    {
        var securityKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(TestHelper.TestJwtSecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, worker.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, worker.Email),
            new Claim(JwtRegisteredClaimNames.Name, worker.Name),
            new Claim("farmId", worker.FarmId.ToString()),
            new Claim("workerId", worker.Id.ToString()),
            new Claim("role", worker.Role ?? "WORKER"),
            new Claim("userType", "Worker"),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var token = new JwtSecurityToken(
            issuer: "TestIssuer",
            audience: "TestAudience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}