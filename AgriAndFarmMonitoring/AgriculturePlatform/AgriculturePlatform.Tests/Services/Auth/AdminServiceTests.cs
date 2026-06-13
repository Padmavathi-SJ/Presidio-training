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
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositoryMock;
    private readonly Mock<IJwtService> _jwtServiceMock;
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _adminRepositoryMock = new Mock<IAdminRepository>();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _refreshTokenRepositoryMock = new Mock<IRefreshTokenRepository>();
        _jwtServiceMock = new Mock<IJwtService>();
        
        _adminService = new AdminService(
            _adminRepositoryMock.Object,
            _farmRepositoryMock.Object,
            _refreshTokenRepositoryMock.Object,
            _jwtServiceMock.Object);
    }

    // =============================================
    // REGISTER TESTS
    // =============================================

    [Fact]
    public async Task RegisterAsync_ValidInput_ReturnsAuthResponse()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FarmName = "Test Farm",
            FarmEmail = "farm@test.com",
            AdminName = "Test Admin",
            AdminEmail = "admin@test.com",
            AdminPassword = "Password123!",
            FarmPhone = "1234567890",
            TotalLandHectares = 100
        };

        var createdFarm = new Farm { Id = 1, FarmName = "Test Farm", Email = "farm@test.com" };
        var createdAdmin = new Admin { Id = 1, Name = "Test Admin", Email = "admin@test.com", FarmId = 1 };
        var accessToken = "test-access-token";
        var refreshToken = "test-refresh-token";

        _farmRepositoryMock.Setup(r => r.EmailExistsAsync(registerDto.FarmEmail))
            .ReturnsAsync(false);
        _adminRepositoryMock.Setup(r => r.EmailExistsAsync(registerDto.AdminEmail))
            .ReturnsAsync(false);
        _farmRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Farm>()))
            .ReturnsAsync(createdFarm);
        _adminRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Admin>()))
            .ReturnsAsync(createdAdmin);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(It.IsAny<Admin>()))
            .Returns(accessToken);
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns(refreshToken);
        _jwtServiceMock.Setup(j => j.GetAccessTokenExpiryDate())
            .Returns(DateTime.UtcNow.AddMinutes(15));
        _refreshTokenRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Id = 1, Token = refreshToken, ExpiryDate = DateTime.UtcNow.AddDays(7) });

        // Act
        var result = await _adminService.RegisterAsync(registerDto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(1);
        result.Name.Should().Be("Test Admin");
        result.Email.Should().Be("admin@test.com");
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.FarmId.Should().Be(1);
        result.FarmName.Should().Be("Test Farm");
    }

    [Fact]
    public async Task RegisterAsync_MissingFarmName_ThrowsBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FarmName = "",
            AdminEmail = "admin@test.com",
            AdminPassword = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task RegisterAsync_MissingAdminEmail_ThrowsBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FarmName = "Test Farm",
            AdminEmail = "",
            AdminPassword = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task RegisterAsync_FarmEmailAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FarmName = "Test Farm",
            FarmEmail = "existing@test.com",
            AdminEmail = "admin@test.com",
            AdminPassword = "Password123!"
        };

        _farmRepositoryMock.Setup(r => r.EmailExistsAsync(registerDto.FarmEmail))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.RegisterAsync(registerDto));
    }

    [Fact]
    public async Task RegisterAsync_AdminEmailAlreadyExists_ThrowsBadRequestException()
    {
        // Arrange
        var registerDto = new RegisterDto
        {
            FarmName = "Test Farm",
            FarmEmail = "farm@test.com",
            AdminEmail = "existing@test.com",
            AdminPassword = "Password123!"
        };

        _farmRepositoryMock.Setup(r => r.EmailExistsAsync(registerDto.FarmEmail))
            .ReturnsAsync(false);
        _adminRepositoryMock.Setup(r => r.EmailExistsAsync(registerDto.AdminEmail))
            .ReturnsAsync(true);

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.RegisterAsync(registerDto));
    }

    // =============================================
    // LOGIN TESTS
    // =============================================

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAuthResponse()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "admin@test.com",
            Password = "Password123!"
        };
        string ipAddress = "127.0.0.1";
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.Email = loginDto.Email;
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        admin.IsActive = true;
        
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = true };
        var accessToken = "test-access-token";
        var refreshToken = "test-refresh-token";
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(admin);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(admin.FarmId))
            .ReturnsAsync(farm);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(admin))
            .Returns(accessToken);
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns(refreshToken);
        _jwtServiceMock.Setup(j => j.GetAccessTokenExpiryDate())
            .Returns(DateTime.UtcNow.AddMinutes(15));
        _refreshTokenRepositoryMock.Setup(r => r.RevokeAllUserTokensAsync(It.IsAny<int>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Id = 1, Token = refreshToken, ExpiryDate = DateTime.UtcNow.AddDays(7) });
        _adminRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Admin>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _adminService.LoginAsync(loginDto, ipAddress);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(accessToken);
        result.RefreshToken.Should().Be(refreshToken);
        result.Email.Should().Be(admin.Email);
        result.Name.Should().Be(admin.Name);
        result.FarmId.Should().Be(1);
        result.FarmName.Should().Be("Test Farm");
    }

    [Fact]
    public async Task LoginAsync_EmptyEmail_ThrowsBadRequestException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "", Password = "Password123!" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.LoginAsync(loginDto, ipAddress));
    }

    [Fact]
    public async Task LoginAsync_EmptyPassword_ThrowsBadRequestException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "admin@test.com", Password = "" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => _adminService.LoginAsync(loginDto, ipAddress));
    }

    [Fact]
    public async Task LoginAsync_InvalidEmail_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "wrong@test.com", Password = "Password123!" };
        string ipAddress = "127.0.0.1";
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync((Admin?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.LoginAsync(loginDto, ipAddress));
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "admin@test.com", Password = "WrongPassword" };
        string ipAddress = "127.0.0.1";
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.Email = loginDto.Email;
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword("CorrectPassword123!");
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(admin);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.LoginAsync(loginDto, ipAddress));
    }

    [Fact]
    public async Task LoginAsync_InactiveAccount_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "admin@test.com", Password = "Password123!" };
        string ipAddress = "127.0.0.1";
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.Email = loginDto.Email;
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        admin.IsActive = false;
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(admin);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.LoginAsync(loginDto, ipAddress));
    }

    [Fact]
    public async Task LoginAsync_InactiveFarm_ThrowsUnauthorizedException()
    {
        // Arrange
        var loginDto = new LoginDto { Email = "admin@test.com", Password = "Password123!" };
        string ipAddress = "127.0.0.1";
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.Email = loginDto.Email;
        admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(loginDto.Password);
        admin.IsActive = true;
        
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = false };
        
        _adminRepositoryMock.Setup(r => r.GetByEmailAsync(loginDto.Email))
            .ReturnsAsync(admin);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(admin.FarmId))
            .ReturnsAsync(farm);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.LoginAsync(loginDto, ipAddress));
    }

    // =============================================
    // REFRESH TOKEN TESTS
    // =============================================

    [Fact]
    public async Task RefreshTokenAsync_ValidTokens_ReturnsNewAuthResponse()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "expired-access-token",
            RefreshToken = "valid-refresh-token"
        };
        string ipAddress = "127.0.0.1";
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        admin.IsActive = true;
        var farm = new Farm { Id = 1, FarmName = "Test Farm", IsActive = true };
        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";
        
        // Mock principal from expired token
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1"),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, "test-jti")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        
        var existingRefreshToken = new RefreshToken
        {
            Id = 1,
            AdminId = 1,
            Token = "valid-refresh-token",
            JwtId = "test-jti",
            ExpiryDate = DateTime.UtcNow.AddDays(7),
            IsUsed = false,
            IsRevoked = false
        };
        
        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken))
            .Returns(principal);
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(existingRefreshToken);
        _adminRepositoryMock.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(admin);
        _farmRepositoryMock.Setup(r => r.GetByIdAsync(admin.FarmId))
            .ReturnsAsync(farm);
        _jwtServiceMock.Setup(j => j.GenerateAccessToken(admin))
            .Returns(newAccessToken);
        _jwtServiceMock.Setup(j => j.GenerateRefreshToken())
            .Returns(newRefreshToken);
        _jwtServiceMock.Setup(j => j.GetAccessTokenExpiryDate())
            .Returns(DateTime.UtcNow.AddMinutes(15));
        _refreshTokenRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<RefreshToken>()))
            .ReturnsAsync(new RefreshToken { Id = 2, Token = newRefreshToken });

        // Act
        var result = await _adminService.RefreshTokenAsync(refreshTokenDto, ipAddress);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be(newAccessToken);
        result.RefreshToken.Should().Be(newRefreshToken);
    }

    [Fact]
    public async Task RefreshTokenAsync_MissingAccessToken_ThrowsBadRequestException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto { AccessToken = "", RefreshToken = "token" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _adminService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_MissingRefreshToken_ThrowsBadRequestException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto { AccessToken = "token", RefreshToken = "" };
        string ipAddress = "127.0.0.1";

        // Act & Assert
        await Assert.ThrowsAsync<BadRequestException>(() => 
            _adminService.RefreshTokenAsync(refreshTokenDto, ipAddress));
    }

    [Fact]
    public async Task RefreshTokenAsync_InvalidAccessToken_ThrowsUnauthorizedException()
    {
        // Arrange
        var refreshTokenDto = new RefreshTokenDto
        {
            AccessToken = "invalid-token",
            RefreshToken = "refresh-token"
        };
        string ipAddress = "127.0.0.1";
        
        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken))
            .Returns((System.Security.Claims.ClaimsPrincipal?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.RefreshTokenAsync(refreshTokenDto, ipAddress));
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
        
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1"),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, "test-jti")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        
        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken))
            .Returns(principal);
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.RefreshTokenAsync(refreshTokenDto, ipAddress));
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
        
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1"),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, "test-jti")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        
        var existingRefreshToken = new RefreshToken
        {
            Id = 1,
            AdminId = 1,
            Token = "revoked-refresh-token",
            JwtId = "test-jti",
            IsRevoked = true
        };
        
        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken))
            .Returns(principal);
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(existingRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.RefreshTokenAsync(refreshTokenDto, ipAddress));
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
        
        var claims = new List<System.Security.Claims.Claim>
        {
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, "1"),
            new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti, "test-jti")
        };
        var identity = new System.Security.Claims.ClaimsIdentity(claims);
        var principal = new System.Security.Claims.ClaimsPrincipal(identity);
        
        var existingRefreshToken = new RefreshToken
        {
            Id = 1,
            AdminId = 1,
            Token = "expired-refresh-token",
            JwtId = "test-jti",
            ExpiryDate = DateTime.UtcNow.AddDays(-1),
            IsUsed = false,
            IsRevoked = false
        };
        
        _jwtServiceMock.Setup(j => j.GetPrincipalFromExpiredToken(refreshTokenDto.AccessToken))
            .Returns(principal);
        _refreshTokenRepositoryMock.Setup(r => r.GetByTokenAsync(refreshTokenDto.RefreshToken))
            .ReturnsAsync(existingRefreshToken);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => 
            _adminService.RefreshTokenAsync(refreshTokenDto, ipAddress));
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
        var result = await _adminService.RevokeTokenAsync(revokeDto, ipAddress);

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
            _adminService.RevokeTokenAsync(revokeDto, ipAddress));
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
        var result = await _adminService.RevokeTokenAsync(revokeDto, ipAddress);

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
        var result = await _adminService.RevokeTokenAsync(revokeDto, ipAddress);

        // Assert
        result.Should().BeFalse();
    }

    // =============================================
    // REVOKE ALL USER TOKENS TESTS
    // =============================================

    [Fact]
    public async Task RevokeAllUserTokensAsync_ValidAdmin_ReturnsTrue()
    {
        // Arrange
        int adminId = 1;
        string ipAddress = "127.0.0.1";
        
        var admin = TestHelper.CreateTestAdmin(1, 1);
        
        _adminRepositoryMock.Setup(r => r.GetByIdAsync(adminId))
            .ReturnsAsync(admin);
        _refreshTokenRepositoryMock.Setup(r => r.RevokeAllUserTokensAsync(adminId, ipAddress))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _adminService.RevokeAllUserTokensAsync(adminId, ipAddress);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task RevokeAllUserTokensAsync_AdminNotFound_ReturnsFalse()
    {
        // Arrange
        int adminId = 999;
        string ipAddress = "127.0.0.1";
        
        _adminRepositoryMock.Setup(r => r.GetByIdAsync(adminId))
            .ReturnsAsync((Admin?)null);

        // Act
        var result = await _adminService.RevokeAllUserTokensAsync(adminId, ipAddress);

        // Assert
        result.Should().BeFalse();
    }
}