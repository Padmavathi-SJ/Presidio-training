// AgriculturePlatform.Tests/Services/TaskManagement/TaskServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.TaskManagement;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<IExcelTaskService> _excelTaskServiceMock;
    private readonly TaskService _taskService;

    public TaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _excelTaskServiceMock = new Mock<IExcelTaskService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _taskService = new TaskService(
            _taskRepositoryMock.Object,
            _workerRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _cropCycleRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _excelTaskServiceMock.Object,  // ← ADD THIS
            mapper);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateTaskDto
        {
            WorkerId = 1,
            FieldId = 1,
            TaskName = "IRRIGATION",
            DueDate = DateTime.UtcNow.AddDays(3),
            Priority = "HIGH",
            Notes = "Test task"
        };
        int farmId = 1, adminId = 1;
        
        var worker = TestHelper.CreateTestWorker(1, farmId, adminId);
        var field = TestHelper.CreateTestField(1, farmId, adminId);
        var createdTask = new WorkerTask
        {
            Id = 1,
            WorkerId = 1,
            TaskName = TaskTypeEnum.IRRIGATION,
            Status = TaskStatusEnum.PENDING
        };
        
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(createDto.WorkerId, farmId, false))
            .ReturnsAsync(worker);
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(createDto.FieldId.Value, farmId, false))
            .ReturnsAsync(field);
        _taskRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<WorkerTask>()))
            .ReturnsAsync(createdTask);

        // Act
        var result = await _taskService.CreateAsync(createDto, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Task created successfully");
    }

    [Fact]
    public async Task GetOverdueTasksAsync_ReturnsOverdueTasks()
    {
        // Arrange
        int farmId = 1;
        var overdueTasks = new List<WorkerTask>
        {
            new WorkerTask { Id = 1, DueDate = DateTime.UtcNow.AddDays(-1), Status = TaskStatusEnum.PENDING }
        };
        
        _taskRepositoryMock.Setup(r => r.GetOverdueTasksAsync(farmId))
            .ReturnsAsync(overdueTasks);

        // Act
        var result = await _taskService.GetOverdueTasksAsync(farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        int id = 1, farmId = 1, adminId = 1;
        string newStatus = "COMPLETED";
        
        var task = new WorkerTask
        {
            Id = id,
            Status = TaskStatusEnum.PENDING,
            Worker = TestHelper.CreateTestWorker(1, farmId, adminId)
        };
        
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(task);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<WorkerTask>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _taskService.UpdateTaskStatusAsync(id, newStatus, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain($"Task status updated to {newStatus}");
    }

    [Fact]
    public async Task ReassignTaskAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        int id = 1, newWorkerId = 2, farmId = 1, adminId = 1;
        
        var task = new WorkerTask
        {
            Id = id,
            WorkerId = 1,
            Status = TaskStatusEnum.PENDING
        };
        var newWorker = TestHelper.CreateTestWorker(2, farmId, adminId);
        
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(task);
        _workerRepositoryMock.Setup(r => r.GetByIdAsync(newWorkerId, farmId, false))
            .ReturnsAsync(newWorker);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<WorkerTask>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _taskService.ReassignTaskAsync(id, newWorkerId, farmId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Task reassigned");
    }

    [Fact]
    public async Task GetTaskStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        int farmId = 1;
        var stats = new TaskStatisticsDto
        {
            TotalTasks = 10,
            PendingTasks = 5,
            InProgressTasks = 2,
            CompletedTasks = 3,
            OverdueTasks = 1,
            CancelledTasks = 0,
            TasksByPriority = new Dictionary<string, int>(),
            TasksByType = new Dictionary<string, int>(),
            AverageCompletionTimeDays = 2.5
        };
        
        _taskRepositoryMock.Setup(r => r.GetTaskStatisticsAsync(farmId))
            .ReturnsAsync(stats);

        // Act
        var result = await _taskService.GetTaskStatisticsAsync(farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.TotalTasks.Should().Be(10);
        result.Data.PendingTasks.Should().Be(5);
    }
}