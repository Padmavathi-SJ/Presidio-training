// AgriculturePlatform.Tests/Services/WorkerTask/WorkerTaskServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.WorkerTask;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;

// Using alias must be at the top, outside the namespace
using DomainWorkerTask = AgriculturePlatform.Domain.Entities.WorkerManagement.WorkerTask;

namespace AgriculturePlatform.Tests.Services.WorkerTask;

public class WorkerTaskServiceTests
{
    private readonly Mock<ITaskRepository> _taskRepositoryMock;
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly WorkerTaskService _workerTaskService;

    public WorkerTaskServiceTests()
    {
        _taskRepositoryMock = new Mock<ITaskRepository>();
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _workerTaskService = new WorkerTaskService(
            _taskRepositoryMock.Object,
            _workerRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _notificationServiceMock.Object,
            mapper);
    }

    [Fact]
    public async Task GetMyTasksAsync_ReturnsTasksForWorker()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        var filter = new WorkerTaskFilterDto { Page = 1, PageSize = 10 };
        
        var tasks = new List<DomainWorkerTask>
        {
            new DomainWorkerTask 
            { 
                Id = 1, 
                WorkerId = workerId, 
                TaskName = TaskTypeEnum.IRRIGATION,
                Status = TaskStatusEnum.PENDING,
                Priority = TaskPriorityEnum.HIGH,
                DueDate = DateTime.UtcNow.AddDays(2)
            },
            new DomainWorkerTask 
            { 
                Id = 2, 
                WorkerId = workerId, 
                TaskName = TaskTypeEnum.FERTILIZING,
                Status = TaskStatusEnum.IN_PROGRESS,
                Priority = TaskPriorityEnum.MEDIUM,
                DueDate = DateTime.UtcNow.AddDays(5)
            }
        };
        
        var pagedResult = new PagedResult<DomainWorkerTask>
        {
            Items = tasks,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        _taskRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, workerId, null, null, null, null, null, null, null, null, null, null, true, It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _workerTaskService.GetMyTasksAsync(filter, workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Items.Should().HaveCount(2);
        result.Data.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ValidTask_ReturnsTask()
    {
        // Arrange
        int taskId = 1, workerId = 1, farmId = 1;
        
        var task = new DomainWorkerTask
        {
            Id = taskId,
            WorkerId = workerId,
            TaskName = TaskTypeEnum.IRRIGATION,
            Status = TaskStatusEnum.PENDING,
            Priority = TaskPriorityEnum.HIGH,
            DueDate = DateTime.UtcNow.AddDays(2),
            Worker = TestHelper.CreateTestWorker(workerId, farmId, 1)
        };
        
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, farmId))
            .ReturnsAsync(task);

        // Act
        var result = await _workerTaskService.GetTaskByIdAsync(taskId, workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Id.Should().Be(taskId);
        result.Data.TaskName.Should().Be("IRRIGATION");
    }

    [Fact]
    public async Task GetTaskByIdAsync_WrongWorker_ReturnsUnauthorized()
    {
        // Arrange
        int taskId = 1, workerId = 1, farmId = 1;
        
        var task = new DomainWorkerTask
        {
            Id = taskId,
            WorkerId = 999,
            TaskName = TaskTypeEnum.IRRIGATION
        };
        
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, farmId))
            .ReturnsAsync(task);

        // Act
        var result = await _workerTaskService.GetTaskByIdAsync(taskId, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have permission");
    }

    [Fact]
    public async Task UpdateTaskStatusAsync_ValidTransition_ReturnsSuccess()
    {
        // Arrange
        int taskId = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateWorkerTaskStatusDto
        {
            Status = "IN_PROGRESS",
            CompletionNotes = "Started working on this task"
        };
        
        var task = new DomainWorkerTask
        {
            Id = taskId,
            WorkerId = workerId,
            Status = TaskStatusEnum.PENDING,
            TaskName = TaskTypeEnum.IRRIGATION
        };
        
        _taskRepositoryMock.Setup(r => r.GetByIdAsync(taskId, farmId))
            .ReturnsAsync(task);
        _taskRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<DomainWorkerTask>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _workerTaskService.UpdateTaskStatusAsync(taskId, updateDto, workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Task status updated to IN_PROGRESS");
    }

    [Fact]
    public async Task GetTaskStatisticsAsync_ReturnsCorrectStats()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        
        var tasks = new List<DomainWorkerTask>
        {
            new DomainWorkerTask { Id = 1, WorkerId = workerId, Status = TaskStatusEnum.PENDING, Priority = TaskPriorityEnum.HIGH },
            new DomainWorkerTask { Id = 2, WorkerId = workerId, Status = TaskStatusEnum.PENDING, Priority = TaskPriorityEnum.URGENT },
            new DomainWorkerTask { Id = 3, WorkerId = workerId, Status = TaskStatusEnum.IN_PROGRESS, Priority = TaskPriorityEnum.MEDIUM },
            new DomainWorkerTask { Id = 4, WorkerId = workerId, Status = TaskStatusEnum.COMPLETED, Priority = TaskPriorityEnum.HIGH, UpdatedAt = DateTime.UtcNow, CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new DomainWorkerTask { Id = 5, WorkerId = workerId, Status = TaskStatusEnum.PENDING, Priority = TaskPriorityEnum.LOW, DueDate = DateTime.UtcNow.AddDays(-1) }
        };
        
        _taskRepositoryMock.Setup(r => r.GetTasksByWorkerAsync(workerId, farmId))
            .ReturnsAsync(tasks);

        // Act
        var result = await _workerTaskService.GetTaskStatisticsAsync(workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.TotalTasks.Should().Be(5);
        result.Data.PendingTasks.Should().Be(3);
        result.Data.InProgressTasks.Should().Be(1);
        result.Data.CompletedTasks.Should().Be(1);
        result.Data.OverdueTasks.Should().Be(1);
        result.Data.HighPriorityTasks.Should().Be(2);
        result.Data.UrgentPriorityTasks.Should().Be(1);
        result.Data.CompletionRate.Should().Be(20);
    }
}