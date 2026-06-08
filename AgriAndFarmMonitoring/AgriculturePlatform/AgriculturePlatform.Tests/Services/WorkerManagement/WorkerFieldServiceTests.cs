// AgriculturePlatform.Tests/Services/WorkerManagement/WorkerFieldServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.WorkerManagement;

public class WorkerFieldServiceTests
{
    private readonly Mock<IWorkerFieldAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly WorkerFieldService _workerFieldService;

    public WorkerFieldServiceTests()
    {
        _assignmentRepositoryMock = new Mock<IWorkerFieldAssignmentRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _workerFieldService = new WorkerFieldService(
            _assignmentRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _cropCycleRepositoryMock.Object,
            mapper);
    }

    [Fact]
    public async Task GetMyAssignedFieldsAsync_WorkerWithAssignments_ReturnsFields()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        
        // Create fields
        var field1 = TestHelper.CreateTestField(1, farmId, 1);
        var field2 = TestHelper.CreateTestField(2, farmId, 1);
        
        // Create assignments with Field property populated
        var assignment1 = TestHelper.CreateTestAssignment(1, workerId, 1, farmId);
        assignment1.Field = field1;
        
        var assignment2 = TestHelper.CreateTestAssignment(2, workerId, 2, farmId);
        assignment2.Field = field2;
        
        var assignments = new List<WorkerFieldAssignment> { assignment1, assignment2 };
        
        _assignmentRepositoryMock.Setup(r => r.GetWorkerActiveAssignmentsAsync(workerId, farmId))
            .ReturnsAsync(assignments);
        
        _cropCycleRepositoryMock.Setup(r => r.GetActiveCountByFieldAsync(It.IsAny<int>()))
            .ReturnsAsync(1);

        // Act
        var result = await _workerFieldService.GetMyAssignedFieldsAsync(workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
        result.Data[0].FieldName.Should().Be(field1.FieldName);
        result.Data[1].FieldName.Should().Be(field2.FieldName);
    }

    [Fact]
    public async Task GetMyAssignedFieldsAsync_WorkerWithNoAssignments_ReturnsEmptyList()
    {
        // Arrange
        int workerId = 2, farmId = 1;
        
        _assignmentRepositoryMock.Setup(r => r.GetWorkerActiveAssignmentsAsync(workerId, farmId))
            .ReturnsAsync(new List<WorkerFieldAssignment>());

        // Act
        var result = await _workerFieldService.GetMyAssignedFieldsAsync(workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().BeEmpty();
        result.Message.Should().Contain("No fields assigned");
    }
}