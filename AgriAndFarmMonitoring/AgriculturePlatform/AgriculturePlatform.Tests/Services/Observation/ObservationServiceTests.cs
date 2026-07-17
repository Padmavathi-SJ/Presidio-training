// AgriculturePlatform.Tests/Services/Observation/ObservationServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;

using DomainObservation = AgriculturePlatform.Domain.Entities.CropMonitoring.Observation;

namespace AgriculturePlatform.Tests.Services.Observation;

public class ObservationServiceTests
{
    private readonly Mock<IObservationRepository> _observationRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly ObservationService _observationService;

    public ObservationServiceTests()
    {
        _observationRepositoryMock = new Mock<IObservationRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _notificationServiceMock = new Mock<INotificationService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _observationService = new ObservationService(
            _observationRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _cropCycleRepositoryMock.Object,
            _workerRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _fileStorageServiceMock.Object,
            _notificationServiceMock.Object,
            mapper);
    }

    [Fact]
    public async Task CreateObservationAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateObservationDto
        {
            FieldId = 1,
            CropHealth = "GOOD",
            Notes = "Crop looking healthy",
            ObservationDate = DateTime.UtcNow
        };
        int farmId = 1, workerId = 1, adminId = 1;
        
        var field = TestHelper.CreateTestField(1, farmId, adminId);
        var createdObservation = new DomainObservation { Id = 1, FieldId = 1 };
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(createDto.FieldId, farmId, false))
            .ReturnsAsync(field);
        _observationRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<DomainObservation>()))
            .ReturnsAsync(createdObservation);

        // Act
        var result = await _observationService.CreateObservationAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("created successfully");
    }

    [Fact]
    public async Task CreateObservationAsync_InvalidField_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateObservationDto { FieldId = 999 };
        int farmId = 1, workerId = 1, adminId = 1;
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(createDto.FieldId, farmId, false))
            .ReturnsAsync((Field?)null);

        // Act
        var result = await _observationService.CreateObservationAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task UpdateOwnObservationAsync_ValidOwner_ReturnsSuccess()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateObservationDto
        {
            Notes = "Updated observation notes",
            CropHealth = "EXCELLENT"
        };
        
        var observation = new DomainObservation { Id = id, WorkerId = workerId, FarmId = farmId };
        
        _observationRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _observationRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(observation);
        _observationRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DomainObservation>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _observationService.UpdateOwnObservationAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateOwnObservationAsync_NotOwner_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateObservationDto { Notes = "Updated notes" };
        
        _observationRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _observationService.UpdateOwnObservationAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have permission");
    }

    [Fact]
    public async Task DeleteOwnObservationAsync_ValidOwner_ReturnsSuccess()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var observation = new DomainObservation { Id = id, WorkerId = workerId, FarmId = farmId };
        
        _observationRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _observationRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(observation);
        _observationRepositoryMock.Setup(r => r.SoftDeleteAsync(It.IsAny<DomainObservation>(), workerId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _observationService.DeleteOwnObservationAsync(id, workerId, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
    }

    [Fact]
    public async Task GetPestStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        int farmId = 1;
        var stats = new ObservationStatisticsDto
        {
            TotalObservations = 10,
            ObservationsWithPest = 3,
            ObservationsWithoutPest = 7,
            PestTypeDistribution = new Dictionary<string, int>(),
            CropHealthDistribution = new Dictionary<string, int>(),
            ObservationsByField = new Dictionary<string, int>(),
            ObservationsByWorker = new Dictionary<string, int>(),
            RecentTrend = new List<DailyObservationTrendDto>()
        };
        
        _observationRepositoryMock.Setup(r => r.GetPestDetectionStatisticsAsync(farmId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), null))
            .ReturnsAsync(stats);

        // Act
        var result = await _observationService.GetPestStatisticsAsync(farmId, null, null);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.TotalObservations.Should().Be(10);
        result.Data.ObservationsWithPest.Should().Be(3);
    }
}