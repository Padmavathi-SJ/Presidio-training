using FluentAssertions;
using System.IdentityModel.Tokens.Jwt;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.Auth;

public class JwtServiceTests
{
    private readonly JwtService _jwtService;
    private readonly string _secretKey = "this-is-a-very-long-secret-key-for-testing-purposes-only";
    private readonly string _issuer = "TestIssuer";
    private readonly string _audience = "TestAudience";
    private readonly int _accessTokenExpiryMinutes = 15;
    private readonly int _refreshTokenExpiryDays = 7;

    public JwtServiceTests()
    {
        _jwtService = new JwtService(_secretKey, _issuer, _audience, _accessTokenExpiryMinutes, _refreshTokenExpiryDays);
    }

    [Fact]
    public void GenerateAccessToken_ValidAdmin_ReturnsValidToken()
    {
        // Arrange
        var admin = TestHelper.CreateTestAdmin(1, 1);

        // Act
        var token = _jwtService.GenerateAccessToken(admin);  

        // Assert
        token.Should().NotBeNullOrEmpty();
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        jsonToken.Claims.Should().Contain(c => c.Type == "sub" && c.Value == admin.Id.ToString());
        jsonToken.Claims.Should().Contain(c => c.Type == "email" && c.Value == admin.Email);
        jsonToken.Claims.Should().Contain(c => c.Type == "name" && c.Value == admin.Name);
        jsonToken.Claims.Should().Contain(c => c.Type == "farmId" && c.Value == admin.FarmId.ToString());
    }

    [Fact]
    public void GenerateRefreshToken_ReturnsValidToken()
    {
        // Act
        var refreshToken = _jwtService.GenerateRefreshToken();

        // Assert
        refreshToken.Should().NotBeNullOrEmpty();
        refreshToken.Length.Should().BeGreaterThan(20);
    }

    [Fact]
    public void GetAccessTokenExpiryDate_ReturnsCorrectDate()
    {
        // Act
        var expiryDate = _jwtService.GetAccessTokenExpiryDate();

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddMinutes(_accessTokenExpiryMinutes);
        (expiryDate - expectedExpiry).TotalSeconds.Should().BeLessThan(5);
    }
}