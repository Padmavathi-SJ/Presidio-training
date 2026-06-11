// AgriculturePlatform.Tests/Services/YieldReport/YieldReportServiceTests.cs
using FluentAssertions;
using Moq;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.YieldReport;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.YieldReportTest;

public class YieldReportServiceTests
{
    private readonly Mock<IYieldReportRepository> _reportRepositoryMock;
    private readonly Mock<IHarvestRepository> _harvestRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly Mock<IWorkerFieldAssignmentRepository> _assignmentRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly Mock<IFarmRepository> _farmRepositoryMock;
    private readonly Mock<IFileStorageService> _fileStorageServiceMock;  // ✅ Add this
    private readonly YieldReportService _reportService;

    public YieldReportServiceTests()
    {
        _reportRepositoryMock = new Mock<IYieldReportRepository>();
        _harvestRepositoryMock = new Mock<IHarvestRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        _assignmentRepositoryMock = new Mock<IWorkerFieldAssignmentRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        _farmRepositoryMock = new Mock<IFarmRepository>();
        _fileStorageServiceMock = new Mock<IFileStorageService>();  // ✅ Add this
        
        var mapper = MapperHelper.CreateMapper();
        
        _reportService = new YieldReportService(
            _reportRepositoryMock.Object,
            _harvestRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _cropCycleRepositoryMock.Object,
            _assignmentRepositoryMock.Object,
            _auditLogServiceMock.Object,
            _farmRepositoryMock.Object,
            _fileStorageServiceMock.Object,  // ✅ Add this parameter
            mapper);
    }

    [Fact]
    public async Task GenerateReportAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var dto = new GenerateYieldReportDto
        {
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 12, 31),
            ReportName = "Annual Yield Report 2024"
        };
        int farmId = 1, adminId = 1;
        
        var harvests = new List<Harvest>
        {
            new Harvest { Id = 1, QuantityKg = 1000, ApprovalStatus = "APPROVED", PricePerKg = 2.5m },
            new Harvest { Id = 2, QuantityKg = 1500, ApprovalStatus = "APPROVED", PricePerKg = 2.5m }
        };
        
        _harvestRepositoryMock.Setup(r => r.GetByDateRangeAsync(farmId, dto.StartDate, dto.EndDate))
            .ReturnsAsync(harvests);
        _reportRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<YieldReport>()))
            .ReturnsAsync(new YieldReport { Id = 1 });

        // Act
        var result = await _reportService.GenerateReportAsync(dto, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("generated successfully");
    }

    [Fact]
    public async Task GenerateReportAsync_InvalidDateRange_ReturnsFailure()
    {
        // Arrange
        var dto = new GenerateYieldReportDto
        {
            StartDate = new DateTime(2024, 12, 31),
            EndDate = new DateTime(2024, 1, 1)
        };
        int farmId = 1, adminId = 1;

        // Act
        var result = await _reportService.GenerateReportAsync(dto, farmId, adminId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("Start date must be less than end date");
    }

    [Fact]
    public async Task GetReportsForWorkerAsync_ReturnsFilteredReports()
    {
        // Arrange
        int farmId = 1, workerId = 1;
        var filter = new YieldReportFilterDto { Page = 1, PageSize = 10 };
        
        var assignedFields = new List<Field>
        {
            new Field { Id = 1, FieldName = "North Field" }
        };
        
        var reports = new List<YieldReport>
        {
            new YieldReport { Id = 1, FieldId = 1, ReportName = "Report 1" },
            new YieldReport { Id = 2, FieldId = 2, ReportName = "Report 2" }
        };
        
        var pagedResult = new PagedResult<YieldReport>
        {
            Items = reports,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        _assignmentRepositoryMock.Setup(r => r.GetFieldsByWorkerAsync(workerId, farmId))
            .ReturnsAsync(assignedFields);
        _reportRepositoryMock.Setup(r => r.GetPagedAsync(farmId, null, null, null, null, null, null, It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reportService.GetReportsForWorkerAsync(filter, farmId, workerId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(1);
    }

    [Fact]
    public async Task CompareYieldsAsync_ReturnsComparison()
    {
        // Arrange
        int farmId = 1, currentYear = 2024, previousYear = 2023;
        
        var fields = new List<Field>
        {
            new Field { Id = 1, FieldName = "North Field", AreaHectares = 10 },
            new Field { Id = 2, FieldName = "South Field", AreaHectares = 15 }
        };
        
        _fieldRepositoryMock.Setup(r => r.GetByFarmIdAsync(farmId))
            .ReturnsAsync(fields);
        _reportRepositoryMock.Setup(r => r.GetTotalYieldForPeriodAsync(farmId, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<int?>()))
            .ReturnsAsync(10000);

        // Act
        var result = await _reportService.CompareYieldsAsync(farmId, null, currentYear, previousYear);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FieldComparisons.Should().HaveCount(2);
        result.Data.Summary.BestPerformingField.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetYieldSummaryAsync_ReturnsSummary()
    {
        // Arrange
        int farmId = 1;
        var fromDate = new DateTime(2024, 1, 1);
        var toDate = new DateTime(2024, 12, 31);
        
        var harvests = new List<Harvest>
        {
            new Harvest { Id = 1, QuantityKg = 1000, ApprovalStatus = "APPROVED", PricePerKg = 2.5m },
            new Harvest { Id = 2, QuantityKg = 2000, ApprovalStatus = "APPROVED", PricePerKg = 2.5m }
        };
        
        _reportRepositoryMock.Setup(r => r.GetTotalYieldForPeriodAsync(farmId, fromDate, toDate, null))
            .ReturnsAsync(3000);
        _harvestRepositoryMock.Setup(r => r.GetByDateRangeAsync(farmId, fromDate, toDate))
            .ReturnsAsync(harvests);

        // Act
        var result = await _reportService.GetYieldSummaryAsync(farmId, fromDate, toDate);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalYieldKg.Should().Be(3000);
        result.Data.TotalHarvests.Should().Be(2);
        result.Data.TotalValue.Should().Be(7500);
    }

    [Fact]
    public async Task ExportReportAsync_ValidReport_ReturnsFile()
    {
        // Arrange
        int id = 1, farmId = 1, adminId = 1;
        var report = new YieldReport
        {
            Id = id,
            FarmId = farmId,
            ReportName = "Test Report",
            TotalYieldKg = 5000,
            TotalHarvests = 5
        };
        
        var fileContent = System.Text.Encoding.UTF8.GetBytes("test content");
        var downloadUrl = "http://localhost:5000/api/downloads/reports/test.csv";
        
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(report);
        _reportRepositoryMock.Setup(r => r.UpdateAsync(It.IsAny<YieldReport>()))
            .Returns(Task.CompletedTask);
        _fileStorageServiceMock.Setup(r => r.SaveFileAsync(It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Reports/YieldReports/test.csv");
        _fileStorageServiceMock.Setup(r => r.GetDownloadUrl(It.IsAny<string>()))
            .Returns(downloadUrl);

        // Act
        var result = await _reportService.ExportReportAsync(id, "CSV", farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.FileSize.Should().BeGreaterThan(0);  // ✅ Use FileSize, not Length
        result.Data.DownloadUrl.Should().Be(downloadUrl);
        result.Data.FileFormat.Should().Be("CSV");
    }

    [Fact]
    public async Task DeleteReportAsync_ValidReport_ReturnsSuccess()
    {
        // Arrange
        int id = 1, farmId = 1, adminId = 1;
        var report = new YieldReport { Id = id, FarmId = farmId, FilePath = "Reports/YieldReports/test.csv" };
        
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(report);
        _reportRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<YieldReport>()))
            .Returns(Task.CompletedTask);
        _fileStorageServiceMock.Setup(r => r.DeleteFileAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _reportService.DeleteReportAsync(id, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("deleted successfully");
    }

    [Fact]
    public async Task GetReportByIdAsync_ExistingReport_ReturnsSuccess()
    {
        // Arrange
        int id = 1, farmId = 1;
        var report = new YieldReport 
        { 
            Id = id, 
            FarmId = farmId,
            ReportName = "Test Report",
            TotalYieldKg = 5000
        };
        
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync(report);

        // Act
        var result = await _reportService.GetReportByIdAsync(id, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(id);
        result.Data.ReportName.Should().Be("Test Report");
    }

    [Fact]
    public async Task GetReportByIdAsync_NonExistingReport_ReturnsFailure()
    {
        // Arrange
        int id = 999, farmId = 1;
        
        _reportRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId))
            .ReturnsAsync((YieldReport?)null);

        // Act
        var result = await _reportService.GetReportByIdAsync(id, farmId);

        // Assert
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("not found");
    }

    [Fact]
    public async Task GetAllReportsAsync_ReturnsPagedResult()
    {
        // Arrange
        int farmId = 1;
        var filter = new YieldReportFilterDto { Page = 1, PageSize = 10 };
        
        var reports = new List<YieldReport>
        {
            new YieldReport { Id = 1, ReportName = "Report 1", TotalYieldKg = 1000 },
            new YieldReport { Id = 2, ReportName = "Report 2", TotalYieldKg = 2000 }
        };
        
        var pagedResult = new PagedResult<YieldReport>
        {
            Items = reports,
            TotalCount = 2,
            Page = 1,
            PageSize = 10
        };
        
        _reportRepositoryMock.Setup(r => r.GetPagedAsync(
            farmId, filter.CropCycleId, filter.FieldId, filter.ReportType,
            filter.FromDate, filter.ToDate, filter.IsScheduled, It.IsAny<PaginationParams>()))
            .ReturnsAsync(pagedResult);

        // Act
        var result = await _reportService.GetAllReportsAsync(filter, farmId);

        // Assert
        result.Success.Should().BeTrue();
        result.Data.Should().NotBeNull();
        result.Data!.TotalCount.Should().Be(2);
        result.Data.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateScheduledReportAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var dto = new CreateYieldReportDto
        {
            ReportName = "Weekly Report",
            ReportType = "WEEKLY",
            StartDate = new DateTime(2024, 1, 1),
            EndDate = new DateTime(2024, 12, 31),
            IsScheduled = true,
            ScheduleCron = "0 0 * * 0"
        };
        int farmId = 1, adminId = 1;
        
        _reportRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<YieldReport>()))
            .ReturnsAsync(new YieldReport { Id = 1 });

        // Act
        var result = await _reportService.CreateScheduledReportAsync(dto, farmId, adminId);

        // Assert
        result.Success.Should().BeTrue();
        result.Message.Should().Contain("Scheduled report created successfully");
    }
}