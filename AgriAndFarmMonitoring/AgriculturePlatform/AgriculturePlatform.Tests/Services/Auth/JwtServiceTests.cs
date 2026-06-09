// AgriculturePlatform.Tests/Services/Auth/JwtServiceTests.cs
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
    private readonly int _expiryDays = 7d;

    public JwtServiceTests()
    {
        _jwtService = new JwtService(_secretKey, _issuer, _audience, _expiryDays);
    }

    [Fact]
    public void GenerateToken_ValidAdmin_ReturnsValidToken()
    {
        // Arrange
        var admin = TestHelper.CreateTestAdmin(1, 1);

        // Act
        var token = _jwtService.GenerateToken(admin);

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
    public void GenerateToken_TokenContainsCorrectExpiry()
    {
        // Arrange
        var admin = TestHelper.CreateTestAdmin(1, 1);

        // Act
        var token = _jwtService.GenerateToken(admin);
        
        var handler = new JwtSecurityTokenHandler();
        var jsonToken = handler.ReadJwtToken(token);
        
        var expiry = jsonToken.ValidTo;

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddDays(_expiryDays);
        (expiry - expectedExpiry).TotalSeconds.Should().BeLessThan(5);
    }

    [Fact]
    public void GetExpiryDate_ReturnsCorrectDate()
    {
        // Act
        var expiryDate = _jwtService.GetExpiryDate();

        // Assert
        var expectedExpiry = DateTime.UtcNow.AddDays(_expiryDays);
        (expiryDate - expectedExpiry).TotalSeconds.Should().BeLessThan(1);
    }
}