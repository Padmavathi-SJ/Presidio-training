// AgriculturePlatform.Tests/Services/AlertServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;

// Add using alias to resolve ambiguity
using DomainAlert = AgriculturePlatform.Domain.Entities.CropMonitoring.Alert;

namespace AgriculturePlatform.Tests.Services.Alert;

public class AlertServiceTests
{
    private readonly Mock<IAlertRepository> _alertRepositoryMock;
    private readonly Mock<IAlertThresholdRepository> _thresholdRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<IAlertNotificationService> _notificationServiceMock;
    private readonly AlertService _alertService;

    public AlertServiceTests()
    {
        _alertRepositoryMock = new Mock<IAlertRepository>();
        _thresholdRepositoryMock = new Mock<IAlertThresholdRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _notificationServiceMock = new Mock<IAlertNotificationService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _alertService = new AlertService(
            _alertRepositoryMock.Object,
            _thresholdRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _cropCycleRepositoryMock.Object,
            _auditLogServiceMock.Object,
            mapper,
            _notificationServiceMock.Object);
    }

    [Fact]
    public async Task GetAllAlertsAsync_WithFilters_ReturnsPagedResult()
    {
        // Arrange
        var filter = new AlertFilterDto { Page = 1, PageSize = 20 };
        int farmId = 1;
        
        var alerts = new List<DomainAlert>
        {
            new DomainAlert { Id = 1, AlertType = AlertTypeEnum.DROUGHT_STRESS, Severity = AlertSeverityEnum.HIGH, IsResolved = false, Message = "Test Alert 1" },
            new DomainAlert { Id = 2, AlertType = AlertTypeEnum.WATERLOGGED, Severity = AlertSeverityEnum.MEDIUM, IsResolved = true, Message = "Test Alert 2" }
        };
        
        var pagedResult = new PagedResult<DomainAlert>
        {
            Items = alerts,
            TotalCount = 2,
            Page = 1,
            PageSize = 20
        };
        
        _alertRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, filter.FieldId, filter.CropCycleId, filter.AlertType,
            filter.Severity, filter.IsResolved, filter.FromDate, filter.ToDate, It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _alertService.GetAllAlertsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetUnresolvedCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        int farmId = 1;
        int expectedCount = 5;
        
        _alertRepositoryMock.Setup(r => r.GetUnresolvedCountAsync(farmId))
            .ReturnsAsync(expectedCount);

        // Act
        var result = await _alertService.GetUnresolvedCountAsync(farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().Be(expectedCount);
    }

    [Fact]
    public async Task GetCriticalAlertsAsync_ReturnsCriticalAlerts()
    {
        // Arrange
        int farmId = 1;
        var criticalAlerts = new List<DomainAlert>
        {
            new DomainAlert { Id = 1, AlertType = AlertTypeEnum.DROUGHT_STRESS, Severity = AlertSeverityEnum.CRITICAL, Message = "Critical Alert 1" },
            new DomainAlert { Id = 2, AlertType = AlertTypeEnum.HEAT_STRESS, Severity = AlertSeverityEnum.CRITICAL, Message = "Critical Alert 2" }
        };
        
        _alertRepositoryMock.Setup(r => r.GetCriticalAlertsAsync(farmId))
            .ReturnsAsync(criticalAlerts);

        // Act
        var result = await _alertService.GetCriticalAlertsAsync(farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task ResolveAlertAsync_ValidAlert_ReturnsSuccess()
    {
        // Arrange
        int alertId = 1, farmId = 1, adminId = 1;
        var resolveDto = new ResolveAlertDto { ResolutionNotes = "Issue resolved" };
        
        var alert = new DomainAlert
        {
            Id = alertId,
            FarmId = farmId,
            IsResolved = false,
            CreatedAt = DateTime.UtcNow
        };
        
        _alertRepositoryMock.Setup(r => r.GetByIdAsync(alertId, farmId))
            .ReturnsAsync(alert);
        _alertRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DomainAlert>()))
            .Returns(Task.CompletedTask);
        _notificationServiceMock.Setup(n => n.NotifyAlertResolvedAsync(farmId, It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _alertService.ResolveAlertAsync(alertId, resolveDto, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Alert resolved successfully");
    }

    [Fact]
    public async Task ResolveAlertAsync_AlreadyResolved_ReturnsFailure()
    {
        // Arrange
        int alertId = 1, farmId = 1, adminId = 1;
        var resolveDto = new ResolveAlertDto { ResolutionNotes = "Already resolved" };
        
        var alert = new DomainAlert
        {
            Id = alertId,
            FarmId = farmId,
            IsResolved = true
        };
        
        _alertRepositoryMock.Setup(r => r.GetByIdAsync(alertId, farmId))
            .ReturnsAsync(alert);

        // Act
        var result = await _alertService.ResolveAlertAsync(alertId, resolveDto, farmId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Alert is already resolved");
    }

    [Fact]
    public async Task ResolveAlertAsync_AlertNotFound_ReturnsFailure()
    {
        // Arrange
        int alertId = 999, farmId = 1, adminId = 1;
        var resolveDto = new ResolveAlertDto { ResolutionNotes = "Not found" };
        
        _alertRepositoryMock.Setup(r => r.GetByIdAsync(alertId, farmId))
            .ReturnsAsync((DomainAlert?)null);

        // Act
        var result = await _alertService.ResolveAlertAsync(alertId, resolveDto, farmId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Alert with ID 999 not found");
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStatistics()
    {
        // Arrange
        int farmId = 1;
        var expectedStats = new AlertStatisticsDto
        {
            TotalAlerts = 10,
            ResolvedAlerts = 6,
            UnresolvedAlerts = 4,
            AlertsByType = new Dictionary<string, int> { { "DROUGHT_STRESS", 5 }, { "WATERLOGGED", 3 }, { "HEAT_STRESS", 2 } },
            AlertsBySeverity = new Dictionary<string, int> { { "HIGH", 4 }, { "MEDIUM", 4 }, { "LOW", 2 } },
            AlertsByField = new Dictionary<string, int> { { "North Field", 6 }, { "South Field", 4 } }
        };
        
        _alertRepositoryMock.Setup(r => r.GetStatisticsAsync(farmId, null, null))
            .ReturnsAsync(expectedStats);

        // Act
        var result = await _alertService.GetStatisticsAsync(farmId, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.TotalAlerts.Should().Be(10);
        result.Data.ResolvedAlerts.Should().Be(6);
        result.Data.UnresolvedAlerts.Should().Be(4);
        result.Data.AlertsByType.Should().ContainKey("DROUGHT_STRESS");
        result.Data.AlertsBySeverity.Should().ContainKey("HIGH");
    }

    [Fact]
    public async Task CheckAndCreateAlertAsync_ThresholdViolation_CreatesAlert()
    {
        // Arrange
        int fieldId = 1, cropCycleId = 1, farmId = 1, adminId = 1;
        string sensorType = "SOIL_MOISTURE";
        decimal value = 12m; // Below threshold
        
        var field = TestHelper.CreateTestField(fieldId, farmId, adminId);
        var cropCycle = TestHelper.CreateTestCropCycle(cropCycleId, fieldId, farmId);
        var threshold = new AlertThreshold
        {
            MinValue = 20m,
            MaxValue = 45m,
            Severity = "HIGH",
            IsActive = true
        };
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync(field);
        _cropCycleRepositoryMock.Setup(r => r.GetByIdAsync(cropCycleId, farmId, false))
            .ReturnsAsync(cropCycle);
        _thresholdRepositoryMock.Setup(r => r.GetByCropAndStageAsync(
            It.IsAny<string>(), It.IsAny<string>(), sensorType, farmId))
            .ReturnsAsync(threshold);
        _alertRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<DomainAlert>()))
            .ReturnsAsync((DomainAlert a) => { a.Id = 1; return a; });
        _notificationServiceMock.Setup(n => n.NotifyNewAlertAsync(farmId, It.IsAny<object>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _alertService.CheckAndCreateAlertAsync(fieldId, cropCycleId, sensorType, value, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Message.Should().Contain("Alert created");
    }

    [Fact]
    public async Task CheckAndCreateAlertAsync_NoThreshold_ReturnsNoAlert()
    {
        // Arrange
        int fieldId = 1, cropCycleId = 1, farmId = 1, adminId = 1;
        string sensorType = "SOIL_MOISTURE";
        decimal value = 25m;
        
        var field = TestHelper.CreateTestField(fieldId, farmId, adminId);
        var cropCycle = TestHelper.CreateTestCropCycle(cropCycleId, fieldId, farmId);
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync(field);
        _cropCycleRepositoryMock.Setup(r => r.GetByIdAsync(cropCycleId, farmId, false))
            .ReturnsAsync(cropCycle);
        _thresholdRepositoryMock.Setup(r => r.GetByCropAndStageAsync(
            It.IsAny<string>(), It.IsAny<string>(), sensorType, farmId))
            .ReturnsAsync((AlertThreshold?)null);

        // Act
        var result = await _alertService.CheckAndCreateAlertAsync(fieldId, cropCycleId, sensorType, value, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Message.Should().Contain("No threshold configured");
    }

    [Fact]
    public async Task CheckAndCreateAlertAsync_NoViolation_ReturnsNoAlert()
    {
        // Arrange
        int fieldId = 1, cropCycleId = 1, farmId = 1, adminId = 1;
        string sensorType = "SOIL_MOISTURE";
        decimal value = 30m; // Within threshold
        
        var field = TestHelper.CreateTestField(fieldId, farmId, adminId);
        var cropCycle = TestHelper.CreateTestCropCycle(cropCycleId, fieldId, farmId);
        var threshold = new AlertThreshold
        {
            MinValue = 20m,
            MaxValue = 45m,
            Severity = "MEDIUM",
            IsActive = true
        };
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync(field);
        _cropCycleRepositoryMock.Setup(r => r.GetByIdAsync(cropCycleId, farmId, false))
            .ReturnsAsync(cropCycle);
        _thresholdRepositoryMock.Setup(r => r.GetByCropAndStageAsync(
            It.IsAny<string>(), It.IsAny<string>(), sensorType, farmId))
            .ReturnsAsync(threshold);

        // Act
        var result = await _alertService.CheckAndCreateAlertAsync(fieldId, cropCycleId, sensorType, value, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeNull();
        result.Message.Should().Contain("No violation detected");
    }

    [Fact]
    public async Task CheckAndCreateAlertAsync_FieldNotFound_ReturnsFailure()
    {
        // Arrange
        int fieldId = 999, cropCycleId = 1, farmId = 1, adminId = 1;
        string sensorType = "SOIL_MOISTURE";
        decimal value = 25m;
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync((Field?)null);

        // Act
        var result = await _alertService.CheckAndCreateAlertAsync(fieldId, cropCycleId, sensorType, value, farmId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Field not found");
    }

    [Fact]
    public async Task CheckAndCreateAlertAsync_CropCycleNotFound_ReturnsFailure()
    {
        // Arrange
        int fieldId = 1, cropCycleId = 999, farmId = 1, adminId = 1;
        string sensorType = "SOIL_MOISTURE";
        decimal value = 25m;
        
        var field = TestHelper.CreateTestField(fieldId, farmId, adminId);
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(fieldId, farmId, false))
            .ReturnsAsync(field);
        _cropCycleRepositoryMock.Setup(r => r.GetByIdAsync(cropCycleId, farmId, false))
            .ReturnsAsync((CropCycle?)null);

        // Act
        var result = await _alertService.CheckAndCreateAlertAsync(fieldId, cropCycleId, sensorType, value, farmId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Crop cycle not found");
    }
}