// AgriculturePlatform.Tests/Services/Sensor/SensorReadingServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.Sensor;

public class SensorReadingServiceTests
{
    private readonly Mock<ISensorReadingRepository> _sensorRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<IAlertService> _alertServiceMock;
    private readonly Mock<IAlertNotificationService> _notificationServiceMock;
    private readonly SensorReadingService _sensorService;

    public SensorReadingServiceTests()
    {
        _sensorRepositoryMock = new Mock<ISensorReadingRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _alertServiceMock = new Mock<IAlertService>();
        _notificationServiceMock = new Mock<IAlertNotificationService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _sensorService = new SensorReadingService(
            _sensorRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _alertServiceMock.Object,
            _notificationServiceMock.Object,
            mapper);
    }

    [Fact]
    public async Task GetAllReadingsAsync_WithFilters_ReturnsPagedResult()
    {
        // Arrange
        var filter = new SensorReadingFilterDto { 
            Page = 1, 
            PageSize = 10,
            SensorType = "SOIL_MOISTURE"  // ✅ Add sensor type as string
        };
        int farmId = 1;
        
        var readings = new List<SensorReading>
        {
            new SensorReading { Id = 1, Value = 25.5m, SensorType = SensorTypeEnum.SOIL_MOISTURE },
            new SensorReading { Id = 2, Value = 30.2m, SensorType = SensorTypeEnum.SOIL_TEMP }
        };
        
        var pagedResult = new PagedResult<SensorReading>
        {
            Items = readings,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        // ✅ Fix: Pass SensorTypeEnum? instead of string
        _sensorRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, filter.FieldId, filter.CropCycleId, It.IsAny<SensorTypeEnum?>(),
            filter.FromDate, filter.ToDate, It.IsAny<PaginationParams>(), It.IsAny<List<int>>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sensorService.GetAllReadingsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllReadingsAsync_WithSensorTypeFilter_ReturnsPagedResult()
    {
        // Arrange
        var filter = new SensorReadingFilterDto { 
            Page = 1, 
            PageSize = 10,
            SensorType = "SOIL_MOISTURE"
        };
        int farmId = 1;
        
        var readings = new List<SensorReading>
        {
            new SensorReading { Id = 1, Value = 25.5m, SensorType = SensorTypeEnum.SOIL_MOISTURE }
        };
        
        var pagedResult = new PagedResult<SensorReading>
        {
            Items = readings,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        
        _sensorRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, 
            filter.FieldId, 
            filter.CropCycleId, 
            SensorTypeEnum.SOIL_MOISTURE,  // ✅ Pass specific enum value
            filter.FromDate, 
            filter.ToDate, 
            It.IsAny<PaginationParams>(), It.IsAny<List<int>>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sensorService.GetAllReadingsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(1);
        result.Data.Items.First().SensorType.Should().Be("SOIL_MOISTURE");
    }

    [Fact]
    public async Task GetAllReadingsAsync_WithInvalidSensorType_ReturnsAllReadings()
    {
        // Arrange
        var filter = new SensorReadingFilterDto { 
            Page = 1, 
            PageSize = 10,
            SensorType = "INVALID_SENSOR"  // Invalid sensor type
        };
        int farmId = 1;
        
        var readings = new List<SensorReading>
        {
            new SensorReading { Id = 1, Value = 25.5m, SensorType = SensorTypeEnum.SOIL_MOISTURE },
            new SensorReading { Id = 2, Value = 30.2m, SensorType = SensorTypeEnum.SOIL_TEMP }
        };
        
        var pagedResult = new PagedResult<SensorReading>
        {
            Items = readings,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        // ✅ When invalid sensor type is provided, null should be passed
        _sensorRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, filter.FieldId, filter.CropCycleId, null,
            filter.FromDate, filter.ToDate, It.IsAny<PaginationParams>(), It.IsAny<List<int>>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _sensorService.GetAllReadingsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetLatestReadingsPerFieldAsync_ReturnsLatestReadings()
    {
        // Arrange
        int farmId = 1;
        var readings = new List<SensorReading>
        {
            new SensorReading { Id = 1, FieldId = 1, Value = 25.5m, SensorType = SensorTypeEnum.SOIL_MOISTURE, RecordedAt = DateTime.UtcNow }
        };
        
        _sensorRepositoryMock.Setup(r => r.GetLatestPerFieldAsync(farmId, It.IsAny<List<int>>()))
            .ReturnsAsync(readings);

        // Act
        var result = await _sensorService.GetLatestReadingsPerFieldAsync(farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task GetReadingsByDateRangeAsync_ReturnsReadings()
    {
        // Arrange
        int fieldId = 1, farmId = 1;
        DateTime fromDate = DateTime.UtcNow.AddDays(-7);
        DateTime toDate = DateTime.UtcNow;
        
        var readings = new List<SensorReading>
        {
            new SensorReading { Id = 1, Value = 25.5m, RecordedAt = DateTime.UtcNow.AddDays(-1) },
            new SensorReading { Id = 2, Value = 26.0m, RecordedAt = DateTime.UtcNow }
        };
        
        _sensorRepositoryMock.Setup(r => r.GetByFieldAndDateRangeAsync(fieldId, farmId, fromDate, toDate))
            .ReturnsAsync(readings);

        // Act
        var result = await _sensorService.GetReadingsByDateRangeAsync(fieldId, farmId, fromDate, toDate);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetThresholdViolationsAsync_ReturnsViolations()
    {
        // Arrange
        int farmId = 1;
        var violations = new List<SensorReading>
        {
            new SensorReading { Id = 1, Value = 12m, SensorType = SensorTypeEnum.SOIL_MOISTURE }
        };
        
        _sensorRepositoryMock.Setup(r => r.GetThresholdViolationsAsync(farmId, null, null, It.IsAny<List<int>>()))
            .ReturnsAsync(violations);

        // Act
        var result = await _sensorService.GetThresholdViolationsAsync(farmId, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(1);
    }

    [Fact]
    public async Task ExportToExcelAsync_ReturnsExcelFile()
    {
        // Arrange
        int farmId = 1;
        var excelData = new byte[] { 1, 2, 3, 4 };
        
        _sensorRepositoryMock.Setup(r => r.ExportToExcelAsync(farmId, null, null, null, It.IsAny<List<int>>()))
            .ReturnsAsync(excelData);

        // Act
        var result = await _sensorService.ExportToExcelAsync(farmId, null, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAverageReadingsAsync_ReturnsStatistics()
    {
        // Arrange
        int farmId = 1;
        string groupBy = "day";
        var stats = new SensorStatisticsDto { Period = "day" };
        
        _sensorRepositoryMock.Setup(r => r.GetAverageReadingsAsync(farmId, groupBy, null, null, It.IsAny<List<int>>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _sensorService.GetAverageReadingsAsync(farmId, groupBy, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Period.Should().Be("day");
    }
}