// AgriculturePlatform.Tests/Services/HarvestTest/HarvestServiceTests.cs
using FluentAssertions;
using AutoMapper;
using Moq;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.HarvestTest;

public class HarvestServiceTests
{
    private readonly Mock<IHarvestRepository> _harvestRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly Mock<IWorkerRepository> _workerRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly IMapper _mapper;
    private readonly HarvestService _harvestService;

    public HarvestServiceTests()
    {
        _harvestRepositoryMock = new Mock<IHarvestRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        _workerRepositoryMock = new Mock<IWorkerRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();
        _notificationServiceMock = new Mock<INotificationService>();
        
        _mapper = MapperHelper.CreateMapper();
        
        _harvestService = new HarvestService(
            _harvestRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _cropCycleRepositoryMock.Object,
            _workerRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _fileStorageServiceMock.Object,
            _notificationServiceMock.Object,
            _mapper);
    }

    // =============================================
    // CREATE HARVEST TESTS
    // =============================================

    [Fact]
    public async Task CreateHarvestAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateHarvestDto
        {
            FieldId = 1,
            CropCycleId = 1,
            HarvestDate = DateTime.UtcNow,
            QuantityKg = 1000,
            QualityGrade = "A",
            HarvestMethod = "MECHANICAL",
            Notes = "Good harvest this season",
            PricePerKg = 2.5m,
            BatchNumber = "BATCH-001"
        };
        int farmId = 1, workerId = 1, adminId = 1;
        
        var field = TestHelper.CreateTestField(1, farmId, adminId);
        
        var cropCycle = new CropCycle 
        { 
            Id = 1, 
            FarmId = farmId, 
            AdminId = adminId, 
            FieldId = 1,
            CropType = CropTypeEnum.WHEAT,
            Status = TaskStatusEnum.IN_PROGRESS 
        };
        
        var createdHarvest = new Harvest { Id = 1, FieldId = 1, QuantityKg = 1000 };
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(createDto.FieldId, farmId, false))
            .ReturnsAsync(field);
        _cropCycleRepositoryMock.Setup(r => r.GetByIdAsync(createDto.CropCycleId, farmId, false))
            .ReturnsAsync(cropCycle);
        _harvestRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Harvest>()))
            .ReturnsAsync(createdHarvest);

        // Act
        var result = await _harvestService.CreateHarvestAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("submitted for approval");
        result.Data.Should().NotBeNull();
        result.Data!.QuantityKg.Should().Be(1000);
    }

    [Fact]
    public async Task CreateHarvestAsync_InvalidField_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateHarvestDto { FieldId = 999, CropCycleId = 1 };
        int farmId = 1, workerId = 1, adminId = 1;
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(createDto.FieldId, farmId, false))
            .ReturnsAsync((Field?)null);

        // Act
        var result = await _harvestService.CreateHarvestAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Field with ID 999 not found");
    }

    [Fact]
    public async Task CreateHarvestAsync_InvalidCropCycle_ReturnsFailure()
    {
        // Arrange
        var createDto = new CreateHarvestDto { FieldId = 1, CropCycleId = 999 };
        int farmId = 1, workerId = 1, adminId = 1;
        
        var field = TestHelper.CreateTestField(1, farmId, adminId);
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(createDto.FieldId, farmId, false))
            .ReturnsAsync(field);
        _cropCycleRepositoryMock.Setup(r => r.GetByIdAsync(createDto.CropCycleId, farmId, false))
            .ReturnsAsync((CropCycle?)null);

        // Act
        var result = await _harvestService.CreateHarvestAsync(createDto, farmId, workerId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Crop cycle with ID 999 not found");
    }

    // =============================================
    // UPDATE OWN HARVEST TESTS
    // =============================================

    [Fact]
    public async Task UpdateOwnHarvestAsync_ValidOwner_ReturnsSuccess()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateHarvestDto
        {
            Notes = "Updated harvest notes",
            QuantityKg = 1200,
            QualityGrade = "A_PLUS"
        };
        
        var harvest = new Harvest 
        { 
            Id = id, 
            HarvestedBy = workerId, 
            SubmittedBy = workerId,
            FarmId = farmId,
            ApprovalStatus = "PENDING",
            QuantityKg = 1000
        };
        
        _harvestRepositoryMock.Setup(r => r.CanWorkerEditAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);
        _harvestRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Harvest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _harvestService.UpdateOwnHarvestAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("updated successfully");
    }

    [Fact]
    public async Task UpdateOwnHarvestAsync_NotOwner_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateHarvestDto { Notes = "Updated notes" };
        
        _harvestRepositoryMock.Setup(r => r.CanWorkerEditAsync(id, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _harvestService.UpdateOwnHarvestAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have permission");
    }

    [Fact]
    public async Task UpdateOwnHarvestAsync_WhenApproved_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var updateDto = new UpdateHarvestDto { Notes = "Updated notes" };
        
        _harvestRepositoryMock.Setup(r => r.CanWorkerEditAsync(id, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _harvestService.UpdateOwnHarvestAsync(id, updateDto, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("don't have permission");
    }

    // =============================================
    // DELETE OWN HARVEST TESTS
    // =============================================

    [Fact]
    public async Task DeleteOwnHarvestAsync_ValidOwner_ReturnsSuccess()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var harvest = new Harvest 
        { 
            Id = id, 
            HarvestedBy = workerId, 
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _harvestRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);
        _harvestRepositoryMock.Setup(r => r.SoftDeleteAsync(It.IsAny<Harvest>(), workerId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _harvestService.DeleteOwnHarvestAsync(id, workerId, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
    }

    [Fact]
    public async Task DeleteOwnHarvestAsync_ApprovedHarvest_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        var harvest = new Harvest 
        { 
            Id = id, 
            HarvestedBy = workerId, 
            FarmId = farmId,
            ApprovalStatus = "APPROVED"
        };
        
        _harvestRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);

        // Act
        var result = await _harvestService.DeleteOwnHarvestAsync(id, workerId, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Cannot delete an approved harvest");
    }

    [Fact]
    public async Task DeleteOwnHarvestAsync_NotOwner_ReturnsFailure()
    {
        // Arrange
        int id = 1, workerId = 1, farmId = 1;
        
        _harvestRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _harvestService.DeleteOwnHarvestAsync(id, workerId, farmId);

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
        var response = new HarvestWorkerResponseDto
        {
            HarvestId = id,
            WorkerResponse = "I have made the requested changes"
        };
        
        var harvest = new Harvest
        {
            Id = id,
            HarvestedBy = workerId,
            FarmId = farmId,
            ApprovalStatus = "REQUEST_CHANGES"
        };
        
        _harvestRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);
        _harvestRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Harvest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _harvestService.RespondToAdminAsync(id, response, farmId, workerId);

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
        var response = new HarvestWorkerResponseDto { HarvestId = id, WorkerResponse = "Response" };
        
        var harvest = new Harvest
        {
            Id = id,
            HarvestedBy = workerId,
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _harvestRepositoryMock.Setup(r => r.IsOwnerAsync(id, workerId, farmId))
            .ReturnsAsync(true);
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);

        // Act
        var result = await _harvestService.RespondToAdminAsync(id, response, farmId, workerId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Can only respond to harvests that need changes");
    }

    // =============================================
    // ADMIN APPROVE HARVEST TESTS
    // =============================================

    [Fact]
    public async Task ApproveHarvestAsync_Approve_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var approval = new HarvestApprovalDto
        {
            HarvestId = id,
            ApprovalStatus = "APPROVED",
            AdminNotes = "Good quality, approved"
        };
        
        var harvest = new Harvest
        {
            Id = id,
            FarmId = farmId,
            ApprovalStatus = "PENDING",
            QuantityKg = 1000
        };
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);
        _harvestRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Harvest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _harvestService.ApproveHarvestAsync(id, approval, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("approved");
        result.Data!.ApprovalStatus.Should().Be("APPROVED");
        result.Data.ApprovedBy.Should().Be(adminId);
        result.Data.ApprovedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ApproveHarvestAsync_Reject_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var approval = new HarvestApprovalDto
        {
            HarvestId = id,
            ApprovalStatus = "REJECTED",
            RejectionReason = "Quality below standard",
            AdminNotes = "Please improve quality"
        };
        
        var harvest = new Harvest
        {
            Id = id,
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);
        _harvestRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Harvest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _harvestService.ApproveHarvestAsync(id, approval, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("rejected");
        result.Data!.ApprovalStatus.Should().Be("REJECTED");
        result.Data.RejectionReason.Should().Be("Quality below standard");
    }

    [Fact]
    public async Task ApproveHarvestAsync_RequestChanges_ReturnsSuccess()
    {
        // Arrange
        int id = 1, adminId = 1, farmId = 1;
        var approval = new HarvestApprovalDto
        {
            HarvestId = id,
            ApprovalStatus = "REQUEST_CHANGES",
            AdminNotes = "Please update the quantity"
        };
        
        var harvest = new Harvest
        {
            Id = id,
            FarmId = farmId,
            ApprovalStatus = "PENDING"
        };
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);
        _harvestRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<Harvest>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _harvestService.ApproveHarvestAsync(id, approval, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("request_changes");
        result.Data!.ApprovalStatus.Should().Be("REQUEST_CHANGES");
    }

    // =============================================
    // GET ALL HARVESTS TESTS
    // =============================================

    [Fact]
    public async Task GetAllHarvestsAsync_ReturnsPagedResult()
    {
        // Arrange
        int farmId = 1;
        var filter = new HarvestFilterDto { Page = 1, PageSize = 10 };
        
        var harvests = new List<Harvest>
        {
            new Harvest { Id = 1, QuantityKg = 1000 },
            new Harvest { Id = 2, QuantityKg = 2000 }
        };
        
        var pagedResult = new PagedResult<Harvest>
        {
            Items = harvests,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        _harvestRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, filter.FieldId, filter.CropCycleId, filter.WorkerId,
            filter.ApprovalStatus, filter.QualityGrade, filter.FromDate, filter.ToDate,
            filter.IncludeDeleted ?? false, It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _harvestService.GetAllHarvestsAsync(filter, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
    }

    // =============================================
    // GET HARVEST BY ID TESTS
    // =============================================

    [Fact]
    public async Task GetHarvestByIdAsync_ExistingHarvest_ReturnsSuccess()
    {
        // Arrange
        int id = 1, farmId = 1;
        var harvest = new Harvest 
        { 
            Id = id, 
            FarmId = farmId,
            QuantityKg = 1000,
            Field = new Field { FieldName = "North Field" }
        };
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(harvest);

        // Act
        var result = await _harvestService.GetHarvestByIdAsync(id, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
    }

    [Fact]
    public async Task GetHarvestByIdAsync_NonExistingHarvest_ReturnsFailure()
    {
        // Arrange
        int id = 999, farmId = 1;
        
        _harvestRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync((Harvest?)null);

        // Act
        var result = await _harvestService.GetHarvestByIdAsync(id, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    // =============================================
    // GET PENDING APPROVALS TESTS
    // =============================================

    [Fact]
    public async Task GetPendingApprovalsAsync_ReturnsPendingHarvests()
    {
        // Arrange
        int farmId = 1;
        var pagination = new PaginationParams { Page = 1, PageSize = 10 };
        
        var pendingHarvests = new List<Harvest>
        {
            new Harvest { Id = 1, ApprovalStatus = "PENDING" },
            new Harvest { Id = 2, ApprovalStatus = "PENDING" },
            new Harvest { Id = 3, ApprovalStatus = "PENDING" }
        };
        
        _harvestRepositoryMock.Setup(r => r.GetPendingApprovalsAsync(farmId))
            .ReturnsAsync(pendingHarvests);

        // Act
        var result = await _harvestService.GetPendingApprovalsAsync(farmId, pagination);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(3);
    }

    // =============================================
    // YIELD STATISTICS TESTS
    // =============================================

    [Fact]
    public async Task GetYieldStatisticsAsync_ReturnsStatistics()
    {
        // Arrange
        int farmId = 1;
        var stats = new YieldStatisticsDto
        {
            TotalHarvests = 10,
            TotalYieldKg = 15000,
            AverageYieldPerHectare = 5000,
            TotalValue = 37500,
            AveragePricePerKg = 2.5m,
            MonthlyTrend = new List<MonthlyYieldDto>()
        };
        
        _harvestRepositoryMock.Setup(r => r.GetYieldStatisticsAsync(farmId, null, null, null, null))
            .ReturnsAsync(stats);

        // Act
        var result = await _harvestService.GetYieldStatisticsAsync(farmId, null, null, null);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalHarvests.Should().Be(10);
        result.Data.TotalYieldKg.Should().Be(15000);
    }

    // =============================================
    // YEAR OVER YEAR COMPARISON TESTS
    // =============================================

    [Fact]
    public async Task GetYearOverYearComparisonAsync_ReturnsComparison()
    {
        // Arrange
        int farmId = 1, currentYear = 2024;
        
        var currentStats = new YieldStatisticsDto { TotalYieldKg = 15000 };
        var previousStats = new YieldStatisticsDto { TotalYieldKg = 12000 };
        
        _harvestRepositoryMock.Setup(r => r.GetYieldStatisticsAsync(farmId, null, 
            new DateTime(2024, 1, 1), new DateTime(2024, 12, 31), null))
            .ReturnsAsync(currentStats);
        _harvestRepositoryMock.Setup(r => r.GetYieldStatisticsAsync(farmId, null,
            new DateTime(2023, 1, 1), new DateTime(2023, 12, 31), null))
            .ReturnsAsync(previousStats);

        // Act
        var result = await _harvestService.GetYearOverYearComparisonAsync(farmId, currentYear, null);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.PreviousSeasonYield.Should().Be(12000);
        result.Data.YieldGrowthPercentage.Should().Be(25);
    }

    // =============================================
    // VALIDATION TESTS
    // =============================================

    [Fact]
    public async Task ValidateHarvestOwnershipAsync_ReturnsTrue_WhenOwner()
    {
        // Arrange
        int harvestId = 1, workerId = 1, farmId = 1;
        
        _harvestRepositoryMock.Setup(r => r.IsOwnerAsync(harvestId, workerId, farmId))
            .ReturnsAsync(true);

        // Act
        var result = await _harvestService.ValidateHarvestOwnershipAsync(harvestId, workerId, farmId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateHarvestOwnershipAsync_ReturnsFalse_WhenNotOwner()
    {
        // Arrange
        int harvestId = 1, workerId = 1, farmId = 1;
        
        _harvestRepositoryMock.Setup(r => r.IsOwnerAsync(harvestId, workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _harvestService.ValidateHarvestOwnershipAsync(harvestId, workerId, farmId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPendingApprovalsAsync_ReturnsTrue_WhenPendingExists()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        
        _harvestRepositoryMock.Setup(r => r.HasPendingApprovalAsync(workerId, farmId))
            .ReturnsAsync(true);

        // Act
        var result = await _harvestService.HasPendingApprovalsAsync(workerId, farmId);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPendingApprovalsAsync_ReturnsFalse_WhenNoPending()
    {
        // Arrange
        int workerId = 1, farmId = 1;
        
        _harvestRepositoryMock.Setup(r => r.HasPendingApprovalAsync(workerId, farmId))
            .ReturnsAsync(false);

        // Act
        var result = await _harvestService.HasPendingApprovalsAsync(workerId, farmId);

        // Assert
        result.Should().BeFalse();
    }
}