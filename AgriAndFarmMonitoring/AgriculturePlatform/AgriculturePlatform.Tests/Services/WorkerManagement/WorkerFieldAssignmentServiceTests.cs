// AgriculturePlatform.Tests/Services/WorkerManagement/WorkerFieldAssignmentServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.DTOs.WorkerField;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Tests.Helpers;
using AgriculturePlatform.Application.Common;

namespace AgriculturePlatform.Tests.Services.WorkerManagement;

public class WorkerFieldAssignmentServiceTests
{
    private readonly Mock<IWorkerFieldAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly WorkerFieldAssignmentService _assignmentService;

    public WorkerFieldAssignmentServiceTests()
    {
        _assignmentRepositoryMock = new Mock<IWorkerFieldAssignmentRepository>();
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _assignmentService = new WorkerFieldAssignmentService(
            _assignmentRepositoryMock.Object,
            _workerRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _cropCycleRepositoryMock.Object,
            _auditLogServiceMock.Object,
            mapper);
    }

    [Fact]
    public async Task AssignFieldToWorkerAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var assignDto = new AssignFieldToWorkerDto
        {
            WorkerId = 2,
            FieldId = 2,
            Notes = "Test assignment"
        };
        int farmId = 1, adminId = 1;
        
        var worker = TestHelper.CreateTestWorker(2, farmId, adminId);
        var field = TestHelper.CreateTestField(2, farmId, adminId);
        
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(assignDto.WorkerId, farmId, false))
            .ReturnsAsync(worker);
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(assignDto.FieldId, farmId, false))
            .ReturnsAsync(field);
        _assignmentRepositoryMock.Setup(r => r.IsFieldAssignedToWorkerAsync(assignDto.FieldId, assignDto.WorkerId, farmId))
            .ReturnsAsync(false);
        _assignmentRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<WorkerFieldAssignment>()))
            .ReturnsAsync((WorkerFieldAssignment a) => { a.Id = 3; return a; });

        // Act
        var result = await _assignmentService.AssignFieldToWorkerAsync(assignDto, farmId, adminId, "127.0.0.1", "TestAgent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("assigned to worker successfully");
    }

    [Fact]
    public async Task AssignFieldToWorkerAsync_InvalidAdminId_ReturnsFailure()
    {
        // Arrange
        var assignDto = new AssignFieldToWorkerDto
        {
            WorkerId = 1,
            FieldId = 1
        };
        int farmId = 1, adminId = 0;

        // Act
        var result = await _assignmentService.AssignFieldToWorkerAsync(assignDto, farmId, adminId, "127.0.0.1", "TestAgent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Invalid admin ID");
    }

    [Fact]
    public async Task AssignFieldToWorkerAsync_NonExistentWorker_ReturnsFailure()
    {
        // Arrange
        var assignDto = new AssignFieldToWorkerDto
        {
            WorkerId = 999,
            FieldId = 1
        };
        int farmId = 1, adminId = 1;
        
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(assignDto.WorkerId, farmId, false))
            .ReturnsAsync((Worker?)null);

        // Act
        var result = await _assignmentService.AssignFieldToWorkerAsync(assignDto, farmId, adminId, "127.0.0.1", "TestAgent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task AssignFieldToWorkerAsync_AlreadyAssigned_ReturnsFailure()
    {
        // Arrange
        var assignDto = new AssignFieldToWorkerDto
        {
            WorkerId = 1,
            FieldId = 1
        };
        int farmId = 1, adminId = 1;
        
        var worker = TestHelper.CreateTestWorker(1, farmId, adminId);
        var field = TestHelper.CreateTestField(1, farmId, adminId);
        
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(assignDto.WorkerId, farmId, false))
            .ReturnsAsync(worker);
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(assignDto.FieldId, farmId, false))
            .ReturnsAsync(field);
        _assignmentRepositoryMock.Setup(r => r.IsFieldAssignedToWorkerAsync(assignDto.FieldId, assignDto.WorkerId, farmId))
            .ReturnsAsync(true);

        // Act
        var result = await _assignmentService.AssignFieldToWorkerAsync(assignDto, farmId, adminId, "127.0.0.1", "TestAgent");

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("already assigned");
    }

    [Fact]
    public async Task RemoveAssignmentAsync_ExistingAssignment_ReturnsSuccess()
    {
        // Arrange
        int id = 1, farmId = 1, adminId = 1;
        var assignment = TestHelper.CreateTestAssignment(id, 1, 1, farmId);
        
        _assignmentRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(assignment);
        _assignmentRepositoryMock.Setup(r => r.SoftDeleteAsync(It.IsAny<WorkerFieldAssignment>(), adminId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _assignmentService.RemoveAssignmentAsync(id, farmId, adminId, "127.0.0.1", "TestAgent");

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("removed successfully");
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_WithFilters_ReturnsPagedResult()
    {
        // Arrange
        var filter = new WorkerFieldFilterDto
        {
            WorkerId = null,
            FieldId = null,
            IsActive = null,
            AssignedDateFrom = null,
            AssignedDateTo = null,
            EndDateFrom = null,     // ✅ Added
            EndDateTo = null,       // ✅ Added
            Page = 1,
            PageSize = 10
        };
        int farmId = 1;
        
        var assignments = new List<WorkerFieldAssignment>
        {
            TestHelper.CreateTestAssignment(1, 1, 1, farmId),
            TestHelper.CreateTestAssignment(2, 2, 2, farmId)
        };
        
        var pagedResult = new PagedResult<WorkerFieldAssignment>
        {
            Items = assignments,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        // ✅ Update the mock with all parameters including endDateFrom and endDateTo
        _assignmentRepositoryMock.Setup(r => r.GetPagedAssignmentsAsync(
            farmId,
            filter.WorkerId,
            filter.FieldId,
            filter.IsActive,
            filter.AssignedDateFrom,
            filter.AssignedDateTo,
            filter.EndDateFrom,    // ✅ Added
            filter.EndDateTo,      // ✅ Added
            It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().NotBeNull();
        result.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_WithDateFilters_ReturnsFilteredResult()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        
        var filter = new WorkerFieldFilterDto
        {
            WorkerId = null,
            FieldId = null,
            IsActive = true,
            AssignedDateFrom = fromDate,
            AssignedDateTo = toDate,
            EndDateFrom = null,     // ✅ Added
            EndDateTo = null,       // ✅ Added
            Page = 1,
            PageSize = 10
        };
        int farmId = 1;
        
        var assignments = new List<WorkerFieldAssignment>
        {
            TestHelper.CreateTestAssignment(1, 1, 1, farmId)
        };
        
        var pagedResult = new PagedResult<WorkerFieldAssignment>
        {
            Items = assignments,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        
        // ✅ Update the mock with all parameters
        _assignmentRepositoryMock.Setup(r => r.GetPagedAssignmentsAsync(
            farmId,
            filter.WorkerId,
            filter.FieldId,
            filter.IsActive,
            filter.AssignedDateFrom,
            filter.AssignedDateTo,
            filter.EndDateFrom,    // ✅ Added
            filter.EndDateTo,      // ✅ Added
            It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_WithEndDateFilters_ReturnsFilteredResult()
    {
        // Arrange
        var fromDate = DateTime.UtcNow.AddDays(-30);
        var toDate = DateTime.UtcNow;
        
        var filter = new WorkerFieldFilterDto
        {
            WorkerId = null,
            FieldId = null,
            IsActive = true,
            AssignedDateFrom = null,
            AssignedDateTo = null,
            EndDateFrom = fromDate,   // ✅ Added
            EndDateTo = toDate,       // ✅ Added
            Page = 1,
            PageSize = 10
        };
        int farmId = 1;
        
        var assignments = new List<WorkerFieldAssignment>
        {
            TestHelper.CreateTestAssignment(1, 1, 1, farmId)
        };
        
        var pagedResult = new PagedResult<WorkerFieldAssignment>
        {
            Items = assignments,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        
        // ✅ Update the mock with all parameters
        _assignmentRepositoryMock.Setup(r => r.GetPagedAssignmentsAsync(
            farmId,
            filter.WorkerId,
            filter.FieldId,
            filter.IsActive,
            filter.AssignedDateFrom,
            filter.AssignedDateTo,
            filter.EndDateFrom,    // ✅ Added
            filter.EndDateTo,      // ✅ Added
            It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Count.Should().Be(1);
    }

    [Fact]
    public async Task GetAllAssignmentsAsync_WithWorkerAndFieldFilters_ReturnsFilteredResult()
    {
        // Arrange
        var filter = new WorkerFieldFilterDto
        {
            WorkerId = 1,
            FieldId = 2,
            IsActive = true,
            AssignedDateFrom = null,
            AssignedDateTo = null,
            EndDateFrom = null,     // ✅ Added
            EndDateTo = null,       // ✅ Added
            Page = 1,
            PageSize = 10
        };
        int farmId = 1;
        
        var assignments = new List<WorkerFieldAssignment>
        {
            TestHelper.CreateTestAssignment(1, 1, 2, farmId)
        };
        
        var pagedResult = new PagedResult<WorkerFieldAssignment>
        {
            Items = assignments,
            TotalCount = 1,
            Page = 1,
            PageSize = 10
        };
        
        // ✅ Update the mock with all parameters
        _assignmentRepositoryMock.Setup(r => r.GetPagedAssignmentsAsync(
            farmId,
            filter.WorkerId,
            filter.FieldId,
            filter.IsActive,
            filter.AssignedDateFrom,
            filter.AssignedDateTo,
            filter.EndDateFrom,    // ✅ Added
            filter.EndDateTo,      // ✅ Added
            It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _assignmentService.GetAllAssignmentsAsync(filter, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Count.Should().Be(1);
        result.Data.Items.First().WorkerId.Should().Be(1);
        result.Data.Items.First().FieldId.Should().Be(2);
    }
}