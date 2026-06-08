// AgriculturePlatform.Tests/Services/WorkerManagement/WorkerProfileServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.AdminEntities;  // ← ADD THIS for Farm class
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.WorkerManagement;

public class WorkerProfileServiceTests
{
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly WorkerProfileService _workerProfileService;

    public WorkerProfileServiceTests()
    {
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        
        // Use MapperHelper to create proper mapper
        var mapper = MapperHelper.CreateMapper();
        
        _workerProfileService = new WorkerProfileService(
            _workerRepositoryMock.Object,
            _auditLogServiceMock.Object,
            mapper);
    }

    [Fact]
    public async Task GetProfileAsync_ExistingWorker_ReturnsProfile()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        var farm = new Farm { Id = farmId, FarmName = "Test Farm" };  // Now Farm is recognized
        worker.Farm = farm;
        
        _workerRepositoryMock.Setup(r => r.GetWorkerWithFarmAsync(workerId, farmId))
            .ReturnsAsync(worker);

        // Act
        var result = await _workerProfileService.GetProfileAsync(workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Id.Should().Be(workerId);
        result.Data.Name.Should().Be(worker.Name);
    }

    [Fact]
    public async Task UpdateProfileAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        var updateDto = new UpdateWorkerProfileDto
        {
            Name = "Updated Name",
            Phone = "9999999999"
        };
        var worker = TestHelper.CreateTestWorker(1, 1, 1);
        
        _workerRepositoryMock.Setup(r => r.GetWorkerWithFarmAsync(workerId, farmId))
            .ReturnsAsync(worker);
        _workerRepositoryMock.Setup(r => r.UpdateWorkerProfileAsync(It.IsAny<Worker>()))
            .ReturnsAsync(true);

        // Act
        var result = await _workerProfileService.UpdateProfileAsync(workerId, farmId, updateDto);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }
}