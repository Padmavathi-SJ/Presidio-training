// AgriculturePlatform.Tests/Services/CropMonitoring/WeatherServiceTests.cs
using FluentAssertions;
using Moq;
using AutoMapper;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.AdminEntities; // ✅ ADD THIS
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;
using Xunit;

namespace AgriculturePlatform.Tests.Services.CropMonitoring;

public class WeatherServiceTests
{
    private readonly Mock<IWeatherRepository> _weatherRepositoryMock;
    private readonly Mock<IWeatherAlertRepository> _weatherAlertRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<IAdminRepository> _adminRepositoryMock;
    private readonly Mock<IWeatherApiService> _weatherApiServiceMock;
    private readonly Mock<IWorkerFieldAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<ILogger<WeatherService>> _loggerMock;
    private readonly IMapper _mapper;
    private readonly WeatherService _weatherService;

    public WeatherServiceTests()
    {
        _weatherRepositoryMock = new Mock<IWeatherRepository>();
        _weatherAlertRepositoryMock = new Mock<IWeatherAlertRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _adminRepositoryMock = new Mock<IAdminRepository>(); 
        _weatherApiServiceMock = new Mock<IWeatherApiService>();
        _assignmentRepositoryMock = new Mock<IWorkerFieldAssignmentRepository>();
        _notificationServiceMock = new Mock<INotificationService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _loggerMock = new Mock<ILogger<WeatherService>>();
        
        _mapper = MapperHelper.CreateMapper();
        
        _weatherService = new WeatherService(
            _weatherRepositoryMock.Object,
            _weatherAlertRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _adminRepositoryMock.Object,
            _weatherApiServiceMock.Object,
            _assignmentRepositoryMock.Object,
            _notificationServiceMock.Object,
            _auditLogServiceMock.Object,
            _mapper,
            _loggerMock.Object);
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
        
        // ✅ Use It.IsAny with a callback that handles the parameters
        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int fId, bool includeDeleted) => 
                id == fieldId && fId == farmId ? field : null);
        
        _weatherRepositoryMock
            .Setup(r => r.GetLatestByFieldAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int id, int fId) => 
                id == fieldId && fId == farmId ? weather : null);

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
        
        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int fId, bool includeDeleted) => 
                id == fieldId && fId == farmId ? field : null);
        
        _weatherRepositoryMock
            .Setup(r => r.GetLatestByFieldAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((WeatherData?)null);
        
        _weatherApiServiceMock
            .Setup(r => r.GetCurrentWeatherAsync(field.Latitude.Value, field.Longitude.Value))
            .ReturnsAsync(apiWeather);
        
        _weatherRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<WeatherData>()))
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
        
        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int fId, bool includeDeleted) => 
                id == fieldId && fId == farmId ? field : null);

        // Act
        var result = await _weatherService.GetCurrentWeatherAsync(fieldId, farmId, 1);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Field location not set");
    }

    [Fact]
    public async Task GetActiveWeatherAlertsAsync_ReturnsAlerts()
    {
        // Arrange
        int farmId = 1;
        var field = TestHelper.CreateTestField(1, farmId, 1);
        field.Latitude = 40.0;
        field.Longitude = -70.0;
        var fields = new List<Field> { field };
        var alerts = new List<WeatherAlert>
        {
            new WeatherAlert
            {
                Id = 1,
                FarmId = farmId,
                FieldId = 1,
                AlertType = WeatherAlertTypeEnum.HEAT_WAVE,
                Severity = WeatherAlertSeverityEnum.WARNING,
                Title = "Test Alert",
                Message = "Test message",
                AlertTime = DateTime.UtcNow,
                IsAcknowledged = false
            }
        };

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync(field);
        
        _weatherAlertRepositoryMock
            .Setup(r => r.GetActiveAlertsAsync(It.IsAny<int>(), It.IsAny<List<int>?>()))
            .ReturnsAsync(alerts);

        // Act
        var result = await _weatherService.GetActiveWeatherAlertsAsync(farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task AddManualWeatherEntryAsync_ValidData_ReturnsSuccess()
    {
        // Arrange
        int farmId = 1, adminId = 1;
        int fieldId = 1;
        var dto = new ManualWeatherEntryDto
        {
            FieldId = fieldId,
            Temperature = 25.5,
            Humidity = 65,
            Condition = "CLEAR",
            RecordedAt = DateTime.UtcNow
        };
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

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int fId, bool includeDeleted) => 
                id == fieldId && fId == farmId ? field : null);
        
        _weatherRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<WeatherData>()))
            .ReturnsAsync(weather);

        // Act
        var result = await _weatherService.AddManualWeatherEntryAsync(dto, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Temperature.Should().Be(25.5);
    }

    [Fact]
    public async Task AddManualWeatherEntryAsync_InvalidField_ReturnsFailure()
    {
        // Arrange
        int farmId = 1, adminId = 1;
        int fieldId = 999;
        var dto = new ManualWeatherEntryDto
        {
            FieldId = fieldId,
            Temperature = 25.5,
            RecordedAt = DateTime.UtcNow
        };

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((Field?)null);

        // Act
        var result = await _weatherService.AddManualWeatherEntryAsync(dto, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetForecastAsync_ValidField_ReturnsForecast()
    {
        // Arrange
        int fieldId = 1, farmId = 1;
        var field = TestHelper.CreateTestField(fieldId, farmId, 1);
        field.Latitude = 40.7128;
        field.Longitude = -74.0060;
        
        var forecast = new WeatherForecastDto
        {
            FieldId = fieldId,
            FieldName = field.FieldName,
            DailyForecasts = new List<DailyForecastDto>()
        };

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int fId, bool includeDeleted) => 
                id == fieldId && fId == farmId ? field : null);
        
        _weatherApiServiceMock
            .Setup(r => r.GetWeatherForecastAsync(field.Latitude.Value, field.Longitude.Value))
            .ReturnsAsync(forecast);

        // Act
        var result = await _weatherService.GetForecastAsync(fieldId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.FieldId.Should().Be(fieldId);
    }

    [Fact]
    public async Task GetForecastAsync_FieldNotFound_ReturnsFailure()
    {
        // Arrange
        int fieldId = 999, farmId = 1;

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((Field?)null);

        // Act
        var result = await _weatherService.GetForecastAsync(fieldId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetForecastAsync_NoCoordinates_ReturnsFailure()
    {
        // Arrange
        int fieldId = 1, farmId = 1;
        var field = TestHelper.CreateTestField(fieldId, farmId, 1);
        field.Latitude = null;
        field.Longitude = null;

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int fId, bool includeDeleted) => 
                id == fieldId && fId == farmId ? field : null);

        // Act
        var result = await _weatherService.GetForecastAsync(fieldId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Field location not set");
    }

    [Fact]
    public async Task RefreshWeatherDataAsync_ValidField_ReturnsSuccess()
    {
        // Arrange
        int fieldId = 1, farmId = 1, adminId = 1;
        var field = TestHelper.CreateTestField(fieldId, farmId, 1);
        field.Latitude = 40.7128;
        field.Longitude = -74.0060;
        
        var admin = TestHelper.CreateTestAdmin(adminId, farmId);
        var weatherData = new CurrentWeatherDto
        {
            Temperature = 25.0,
            Humidity = 60,
            WindSpeed = 5.0,
            Condition = "CLEAR",
            ObservedAt = DateTime.UtcNow
        };

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int fId, bool includeDeleted) => 
                id == fieldId && fId == farmId ? field : null);
        
        _adminRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync(admin);
        
        _weatherApiServiceMock
            .Setup(r => r.GetCurrentWeatherAsync(field.Latitude.Value, field.Longitude.Value))
            .ReturnsAsync(weatherData);
        
        _weatherRepositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<WeatherData>()))
            .ReturnsAsync((WeatherData w) => w);

        // Act
        var result = await _weatherService.RefreshWeatherDataAsync(fieldId, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("refreshed successfully");
    }

    [Fact]
    public async Task RefreshWeatherDataAsync_FieldNotFound_ReturnsFailure()
    {
        // Arrange
        int fieldId = 999, farmId = 1, adminId = 1;

        _fieldRepositoryMock
            .Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((Field?)null);

        // Act
        var result = await _weatherService.RefreshWeatherDataAsync(fieldId, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("has no coordinates");
    }
}