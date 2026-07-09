// AgriculturePlatform.Tests/Services/QualityCheck/QualityCheckServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.QualityCheckTest;

public class QualityCheckServiceTests
{
    private readonly Mock<IQualityCheckRepository> _qualityCheckRepositoryMock;
    private readonly Mock<IHarvestRepository> _harvestRepositoryMock;
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly QualityCheckService _qualityCheckService;

    public QualityCheckServiceTests()
    {
        _qualityCheckRepositoryMock = new Mock<IQualityCheckRepository>();
        _harvestRepositoryMock = new Mock<IHarvestRepository>();
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _notificationServiceMock = new Mock<INotificationService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _qualityCheckService = new QualityCheckService(
            _qualityCheckRepositoryMock.Object,
            _harvestRepositoryMock.Object,
            _workerRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _notificationServiceMock.Object,
            mapper);
    }

    // =============================================
    // CREATE QUALITY CHECK TESTS
    // =============================================

    [Fact]
    public async Task CreateQualityCheckAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateQualityCheckDto
        {
            HarvestId = 1,
            CheckDate = DateTime.UtcNow,
            MoisturePct = 14.5m,
            DefectPct = 2.3m,
            FinalGrade = "A",
            Notes = "Good quality harvest"
        };
        int farmId = 1, workerId = 1, adminId = 1;
        
        var harvest = new Harvest 
        { 
            Id = 1, 
            FarmId = farmId, 
            QuantityKg = 1000,
            BatchNumber = "BATCH-001"
        };
        var createdCheck = new QualityCheck 
        { 
            Id = 1, 
            HarvestId = 1,
            MoisturePct = 14.5m,
            DefectPct = 2.3m
        };
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(createDto.HarvestId, farmId))
            .ReturnsAsync(harvest);
        _qualityCheckRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<QualityCheck>()))
            .ReturnsAsync(createdCheck);

        // Act
        var result = await _qualityCheckService.CreateQualityCheckAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("submitted for approval");
        result.Data.Should().NotBeNull();
    }

    [Fact]
    public async Task CreateQualityCheckAsync_InvalidHarvest_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateQualityCheckDto { HarvestId = 999 };
        int farmId = 1, workerId = 1, adminId = 1;
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(createDto.HarvestId, farmId))
            .ReturnsAsync((Harvest?)null);

        // Act
        var result = await _qualityCheckService.CreateQualityCheckAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task CreateQualityCheckAsync_WithHighMoisture_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateQualityCheckDto
        {
            HarvestId = 1,
            CheckDate = DateTime.UtcNow,
            MoisturePct = 18.5m,  // High moisture
            DefectPct = 5.0m,
            FinalGrade = "C",
            Notes = "High moisture content"
        };
        int farmId = 1, workerId = 1, adminId = 1;
        
        var harvest = new Harvest { Id = 1, FarmId = farmId, QuantityKg = 1000 };
        var createdCheck = new QualityCheck { Id = 1, HarvestId = 1 };
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(createDto.HarvestId, farmId))
            .ReturnsAsync(harvest);
        _qualityCheckRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<QualityCheck>()))
            .ReturnsAsync(createdCheck);

        // Act
        var result = await _qualityCheckService.CreateQualityCheckAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("submitted for approval");
    }

    // =============================================
    // UPDATE OWN QUALITY CHECK TESTS
    // =============================================

    [Fact]
    public async Task UpdateOwnQualityCheckAsync_ValidOwner_ReturnsSuccess()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateQualityCheckDto
        {
            MoisturePct = 13.5m,
            DefectPct = 1.8m,
            FinalGrade = "A_PLUS",
            Notes = "Updated quality notes"
        };
        
        var qualityCheck = new QualityCheck 
        { 
            Id = id, 
            CheckedBy = workerId, 
            FarmId = farmId,
            ApprovalStatus = "PENDING",
            MoisturePct = 14.5m,
            DefectPct = 2.3m
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.CanWorkerEditAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<QualityCheck>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.UpdateOwnQualityCheckAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("updated successfully");
    }

    [Fact]
    public async Task UpdateOwnQualityCheckAsync_NotOwner_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateQualityCheckDto { Notes = "Updated notes" };
        
        _qualityCheckRepositoryMock.Setup(r => r.CanWorkerEditAsync(id, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _qualityCheckService.UpdateOwnQualityCheckAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have permission");
    }

    [Fact]
    public async Task UpdateOwnQualityCheckAsync_WhenApproved_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateQualityCheckDto { Notes = "Updated notes" };
        
        _qualityCheckRepositoryMock.Setup(r => r.CanWorkerEditAsync(id, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _qualityCheckService.UpdateOwnQualityCheckAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have permission");
    }

    [Fact]
    public async Task UpdateOwnQualityCheckAsync_WhenRequestChanges_ResetsToPending()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateQualityCheckDto
        {
            MoisturePct = 12.5m,
            Notes = "Made requested changes"
        };
        
        var qualityCheck = new QualityCheck 
        { 
            Id = id, 
            CheckedBy = workerId, 
            FarmId = farmId,
            ApprovalStatus = "REQUEST_CHANGES",
            MoisturePct = 14.5m
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.CanWorkerEditAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<QualityCheck>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.UpdateOwnQualityCheckAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data!.ApprovalStatus.Should().Be("PENDING");
    }

    // =============================================
    // DELETE OWN QUALITY CHECK TESTS
    // =============================================

    [Fact]
    public async Task DeleteOwnQualityCheckAsync_ValidOwner_ReturnsSuccess()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var qualityCheck = new QualityCheck 
        { 
            Id = id, 
            CheckedBy = workerId, 
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.SoftDeleteAsync(It.IsAny<QualityCheck>(), workerId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.DeleteOwnQualityCheckAsync(id, workerId, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
    }

    [Fact]
    public async Task DeleteOwnQualityCheckAsync_ApprovedCheck_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var qualityCheck = new QualityCheck 
        { 
            Id = id, 
            CheckedBy = workerId, 
            FarmId = farmId,
            ApprovalStatus = "APPROVED"
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);

        // Act
        var result = await _qualityCheckService.DeleteOwnQualityCheckAsync(id, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot delete an approved quality check");
    }

    [Fact]
    public async Task DeleteOwnQualityCheckAsync_NotOwner_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        
        _qualityCheckRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _qualityCheckService.DeleteOwnQualityCheckAsync(id, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have permission");
    }

    // =============================================
    // RESPOND TO ADMIN TESTS
    // =============================================

    [Fact]
    public async Task RespondToAdminAsync_ValidResponse_ReturnsSuccess()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var response = new QualityCheckWorkerResponseDto
        {
            QualityCheckId = id,
            WorkerResponse = "I have adjusted the moisture levels as requested"
        };
        
        var qualityCheck = new QualityCheck
        {
            Id = id,
            CheckedBy = workerId,
            FarmId = farmId,
            ApprovalStatus = "REQUEST_CHANGES"
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<QualityCheck>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.RespondToAdminAsync(id, response, farmId, workerId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Response submitted");
        result.Data!.ApprovalStatus.Should().Be("PENDING");
    }

    [Fact]
    public async Task RespondToAdminAsync_NotRequestChanges_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var response = new QualityCheckWorkerResponseDto { QualityCheckId = id, WorkerResponse = "Response" };
        
        var qualityCheck = new QualityCheck
        {
            Id = id,
            CheckedBy = workerId,
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);

        // Act
        var result = await _qualityCheckService.RespondToAdminAsync(id, response, farmId, workerId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Can only respond to quality checks that need changes");
    }

    // =============================================
    // ADMIN APPROVE QUALITY CHECK TESTS
    // =============================================

    [Fact]
    public async Task ApproveQualityCheckAsync_Approve_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var approval = new QualityCheckApprovalDto
        {
            QualityCheckId = id,
            ApprovalStatus = "APPROVED",
            AdminNotes = "Quality meets all standards"
        };
        
        var qualityCheck = new QualityCheck
        {
            Id = id,
            FarmId = farmId,
            ApprovalStatus = "PENDING",
            MoisturePct = 14.5m,
            DefectPct = 2.3m
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<QualityCheck>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.ApproveQualityCheckAsync(id, approval, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("approved");
        result.Data!.ApprovalStatus.Should().Be("APPROVED");
        result.Data.ApprovedBy.Should().Be(adminId);
        result.Data.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ApproveQualityCheckAsync_Reject_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var approval = new QualityCheckApprovalDto
        {
            QualityCheckId = id,
            ApprovalStatus = "REJECTED",
            RejectionReason = "Moisture content too high for Grade A",
            AdminNotes = "Please re-test after drying"
        };
        
        var qualityCheck = new QualityCheck
        {
            Id = id,
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<QualityCheck>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.ApproveQualityCheckAsync(id, approval, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("rejected");
        result.Data!.ApprovalStatus.Should().Be("REJECTED");
        result.Data.RejectionReason.Should().Be("Moisture content too high for Grade A");
    }

    [Fact]
    public async Task ApproveQualityCheckAsync_RequestChanges_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var approval = new QualityCheckApprovalDto
        {
            QualityCheckId = id,
            ApprovalStatus = "REQUEST_CHANGES",
            AdminNotes = "Please provide more accurate moisture readings"
        };
        
        var qualityCheck = new QualityCheck
        {
            Id = id,
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<QualityCheck>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.ApproveQualityCheckAsync(id, approval, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("request_changes");
        result.Data!.ApprovalStatus.Should().Be("REQUEST_CHANGES");
        result.Data.AdminNotes.Should().Be("Please provide more accurate moisture readings");
    }

    // =============================================
    // GET ALL QUALITY CHECKS TESTS
    // =============================================

    [Fact]
    public async Task GetAllQualityChecksAsync_ReturnsPagedResult()
    {
        // Arrange
        int farmId = 1;
        var filter = new QualityCheckFilterDto { Page = 1, PageSize = 10 };
        
        var checks = new List<QualityCheck>
        {
            new QualityCheck { Id = 1, MoisturePct = 14.5m, ApprovalStatus = "APPROVED" },
            new QualityCheck { Id = 2, MoisturePct = 16.2m, ApprovalStatus = "PENDING" }
        };
        
        var pagedResult = new PagedResult<QualityCheck>
        {
            Items = checks,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, filter.HarvestId, filter.WorkerId, filter.ApprovalStatus,
            filter.FinalGrade, filter.FromDate, filter.ToDate,
            filter.IncludeDeleted ?? false, It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _qualityCheckService.GetAllQualityChecksAsync(filter, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
    }

    // =============================================
    // GET QUALITY CHECK BY ID TESTS
    // =============================================

    [Fact]
    public async Task GetQualityCheckByIdAsync_ExistingCheck_ReturnsSuccess()
    {
        // Arrange
        int id = 1, farmId = 1;
        var qualityCheck = new QualityCheck 
        { 
            Id = id, 
            FarmId = farmId,
            MoisturePct = 14.5m,
            DefectPct = 2.3m,
            FinalGrade = QualityGradeEnum.A,
            Harvest = new Harvest { Id = 1, BatchNumber = "BATCH-001" }
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);

        // Act
        var result = await _qualityCheckService.GetQualityCheckByIdAsync(id, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
        result.Data.MoisturePct.Should().Be(14.5m);
    }

    [Fact]
    public async Task GetQualityCheckByIdAsync_NonExistingCheck_ReturnsFailure()
    {
        // Arrange
        int id = 999, farmId = 1;
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync((QualityCheck?)null);

        // Act
        var result = await _qualityCheckService.GetQualityCheckByIdAsync(id, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // =============================================
    // GET QUALITY CHECKS BY HARVEST TESTS
    // =============================================

    [Fact]
    public async Task GetQualityChecksByHarvestAsync_ReturnsChecks()
    {
        // Arrange
        int harvestId = 1, farmId = 1;
        var checks = new List<QualityCheck>
        {
            new QualityCheck { Id = 1, HarvestId = harvestId, ApprovalStatus = "APPROVED" },
            new QualityCheck { Id = 2, HarvestId = harvestId, ApprovalStatus = "PENDING" }
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByHarvestAsync(harvestId, farmId))
            .ReturnsAsync(checks);

        // Act
        var result = await _qualityCheckService.GetQualityChecksByHarvestAsync(harvestId, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    // =============================================
    // GET QUALITY CHECKS BY WORKER TESTS
    // =============================================

    [Fact]
    public async Task GetQualityChecksByWorkerAsync_ReturnsChecks()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        var checks = new List<QualityCheck>
        {
            new QualityCheck { Id = 1, CheckedBy = workerId, ApprovalStatus = "APPROVED" },
            new QualityCheck { Id = 2, CheckedBy = workerId, ApprovalStatus = "REJECTED" }
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByWorkerAsync(workerId, farmId))
            .ReturnsAsync(checks);

        // Act
        var result = await _qualityCheckService.GetQualityChecksByWorkerAsync(workerId, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().HaveCount(2);
    }

    // =============================================
    // GET PENDING APPROVALS TESTS
    // =============================================

    [Fact]
    public async Task GetPendingApprovalsAsync_ReturnsPendingChecks()
    {
        // Arrange
        int farmId = 1;
        var pagination = new PaginationParams { Page = 1, PageSize = 10 };
        
        var pendingChecks = new List<QualityCheck>
        {
            new QualityCheck { Id = 1, ApprovalStatus = "PENDING" },
            new QualityCheck { Id = 2, ApprovalStatus = "PENDING" },
            new QualityCheck { Id = 3, ApprovalStatus = "PENDING" }
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetPendingApprovalsAsync(farmId))
            .ReturnsAsync(pendingChecks);

        // Act
        var result = await _qualityCheckService.GetPendingApprovalsAsync(farmId, pagination);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(3);
    }

    // =============================================
    // QUALITY STATISTICS TESTS
    // =============================================

    [Fact]
    public async Task GetQualityStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        int farmId = 1;
        //  Don't set PassRate and RejectionRate - they are computed
        var stats = new QualityStatisticsDto
        {
            TotalChecks = 50,
            ApprovedChecks = 35,
            RejectedChecks = 10,
            PendingChecks = 5,
            // PassRate and RejectionRate will be computed automatically
            AverageMoisturePct = 14.2m,
            AverageDefectPct = 2.5m,
            MinMoisturePct = 10.5m,
            MaxMoisturePct = 18.0m,
            MinDefectPct = 0.5m,
            MaxDefectPct = 8.0m,
            GradeDistribution = new Dictionary<string, int>
            {
                { "A_PLUS", 10 },
                { "A", 15 },
                { "B", 10 },
                { "C", 5 },
                { "REJECTED", 10 }
            },
            MonthlyTrend = new List<MonthlyQualityTrendDto>(),
            QualityByWorker = new Dictionary<string, int>(),
            QualityByHarvest = new Dictionary<string, int>()
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetQualityStatisticsAsync(farmId, null, null, It.IsAny<int?>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _qualityCheckService.GetQualityStatisticsAsync(farmId, null, null);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalChecks.Should().Be(50);
        // Use the computed PassRate from the actual object
        result.Data.PassRate.Should().Be(70); // 35/50 * 100 = 70
        result.Data.AverageMoisturePct.Should().Be(14.2m);
        result.Data.GradeDistribution.Should().ContainKey("A_PLUS");
    }

    [Fact]
    public async Task GetQualityStatisticsAsync_WithDateRange_ReturnsFilteredStats()
    {
        // Arrange
        int farmId = 1;
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);
        
        //  Don't set PassRate - it will be computed
        var stats = new QualityStatisticsDto
        {
            TotalChecks = 30,
            ApprovedChecks = 25,
            RejectedChecks = 3,
            PendingChecks = 2,
            QualityByWorker = new Dictionary<string, int>(),
            QualityByHarvest = new Dictionary<string, int>(),
            GradeDistribution = new Dictionary<string, int>(),
            MonthlyTrend = new List<MonthlyQualityTrendDto>()
            // PassRate will be 25/30 * 100 = 83.33
        };
        
        _qualityCheckRepositoryMock.Setup(repo => repo.GetQualityStatisticsAsync(farmId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>(), It.IsAny<int?>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _qualityCheckService.GetQualityStatisticsAsync(farmId, fromDate, toDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalChecks.Should().Be(30);
        result.Data.PassRate.Should().Be(83.33m);
    }

    [Fact]
    public async Task GetQualityStatisticsAsync_EmptyData_ReturnsEmptyStats()
    {
        // Arrange
        int farmId = 1;
        //  Don't set PassRate and RejectionRate - they will be 0
        var stats = new QualityStatisticsDto
        {
            TotalChecks = 0,
            ApprovedChecks = 0,
            RejectedChecks = 0,
            PendingChecks = 0,
            GradeDistribution = new Dictionary<string, int>(),
            MonthlyTrend = new List<MonthlyQualityTrendDto>(),
            QualityByWorker = new Dictionary<string, int>(),
            QualityByHarvest = new Dictionary<string, int>()
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetQualityStatisticsAsync(farmId, null, null, It.IsAny<int?>()))
            .ReturnsAsync(stats);

        // Act
        var result = await _qualityCheckService.GetQualityStatisticsAsync(farmId, null, null);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalChecks.Should().Be(0);
        result.Data.PassRate.Should().Be(0);
    }

    // =============================================
    // VALIDATION TESTS
    // =============================================

    [Fact]
    public async Task ValidateQualityCheckOwnershipAsync_ReturnsTrue_WhenOwner()
    {
        // Arrange
        int qualityCheckId = 1, workerId = 1, farmId = 1;
        
        _qualityCheckRepositoryMock.Setup(r => r.IsOwnerAsync(qualityCheckId, workerId, farmId))
            .ReturnsAsync(true);

        // Act
        var result = await _qualityCheckService.ValidateQualityCheckOwnershipAsync(qualityCheckId, workerId, farmId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateQualityCheckOwnershipAsync_ReturnsFalse_WhenNotOwner()
    {
        // Arrange
        int qualityCheckId = 1, workerId = 1, farmId = 1;
        
        _qualityCheckRepositoryMock.Setup(r => r.IsOwnerAsync(qualityCheckId, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _qualityCheckService.ValidateQualityCheckOwnershipAsync(qualityCheckId, workerId, farmId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPendingApprovalsAsync_ReturnsTrue_WhenPendingExists()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        
        _qualityCheckRepositoryMock.Setup(r => r.HasPendingApprovalAsync(workerId, farmId))
            .ReturnsAsync(true);

        // Act
        var result = await _qualityCheckService.HasPendingApprovalsAsync(workerId, farmId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPendingApprovalsAsync_ReturnsFalse_WhenNoPending()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        
        _qualityCheckRepositoryMock.Setup(r => r.HasPendingApprovalAsync(workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _qualityCheckService.HasPendingApprovalsAsync(workerId, farmId);

        // Assert
        result.Should().BeFalse();
    }

    // =============================================
    // ADMIN UPDATE TESTS
    // =============================================

    [Fact]
    public async Task UpdateQualityCheckAsync_AdminUpdate_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var updateDto = new UpdateQualityCheckDto
        {
            MoisturePct = 13.0m,
            DefectPct = 1.5m,
            FinalGrade = "A_PLUS",
            Notes = "Updated by admin"
        };
        
        var qualityCheck = new QualityCheck
        {
            Id = id,
            FarmId = farmId,
            MoisturePct = 14.5m,
            DefectPct = 2.3m
        };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<QualityCheck>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.UpdateQualityCheckAsync(id, updateDto, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("updated successfully");
    }

    [Fact]
    public async Task DeleteQualityCheckAsync_AdminDelete_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var qualityCheck = new QualityCheck { Id = id, FarmId = farmId };
        
        _qualityCheckRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(qualityCheck);
        _qualityCheckRepositoryMock.Setup(r => r.SoftDeleteAsync(It.IsAny<QualityCheck>(), adminId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _qualityCheckService.DeleteQualityCheckAsync(id, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
    }
}