// AgriculturePlatform.Tests/Services/CropMonitoring/WeatherServiceTests.cs
using FluentAssertions;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;
using Xunit;

namespace AgriculturePlatform.Tests.Services.CropMonitoring;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherRepository> _weatherRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<IWeatherApiService> _weatherApiServiceMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<ILogger<WeatherService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _weatherRepositoryMock = new Mock<IWeatherRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _weatherApiServiceMock = new Mock<IWeatherApiService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<WeatherService>>();
        
        _mapper = MapperHelper.CreateMapper();
        
        _weatherService = new WeatherService(
            _weatherRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _weatherApiServiceMock.Object,
            _auditLogServiceMock.Object,
            _mapper,
            _loggerMock.Object);  // Add logger parameter
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_ExistingWeatherData_ReturnsWeather()
    {
        // Arrange
        int fieldId = 1, farmId = 1;
        var field = TestHelper.CreateTestField(fieldId, farmId, 1);
        var weather = new WeatherData
        {
            Id = 1,
            FieldId = fieldId,
            FarmId = farmId,
            Temperature = 25.5,
            Humidity = 65,
            Condition = WeatherConditionEnum.CLEAR,
            RecordedAt = DateTime.UtcNow
        };
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync(field);
        _weatherRepositoryMock.Setup(r => r.GetLatestByFieldAsync(fieldId, farmId))
            .ReturnsAsync(weather);

        // Act
        var result = await _weatherService.GetCurrentWeatherAsync(fieldId, farmId, 1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Temperature.Should().Be(25.5);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_NoWeatherData_CallsApiAndSaves()
    {
        // Arrange
        int fieldId = 1, farmId = 1;
        var field = TestHelper.CreateTestField(fieldId, farmId, 1);
        field.Latitude = 40.7128;
        field.Longitude = -74.0060;
        
        var apiWeather = new CurrentWeatherDto
        {
            Temperature = 28.0,
            Humidity = 70,
            WindSpeed = 5.5,
            Condition = "CLOUDY",
            ObservedAt = DateTime.UtcNow
        };
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync(field);
        _weatherRepositoryMock.Setup(r => r.GetLatestByFieldAsync(fieldId, farmId))
            .ReturnsAsync((WeatherData?)null);
        _weatherApiServiceMock.Setup(r => r.GetCurrentWeatherAsync(field.Latitude.Value, field.Longitude.Value))
            .ReturnsAsync(apiWeather);
        _weatherRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<WeatherData>()))
            .ReturnsAsync((WeatherData w) => w);

        // Act
        var result = await _weatherService.GetCurrentWeatherAsync(fieldId, farmId, 1);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Temperature.Should().Be(28.0);
    }

    [Fact]
    public async Task GetCurrentWeatherAsync_NoCoordinates_ReturnsFailure()
    {
        // Arrange
        int fieldId = 1, farmId = 1;
        var field = TestHelper.CreateTestField(fieldId, farmId, 1);
        field.Latitude = null;
        field.Longitude = null;
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync(field);

        // Act
        var result = await _weatherService.GetCurrentWeatherAsync(fieldId, farmId, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Field location not set");
    }
}