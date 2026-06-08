// AgriculturePlatform.Tests/Services/CropMonitoring/CropCycleServiceTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using AgriculturePlatform.Application.DTOs.CropCycle;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.CropMonitoring;

public class CropCycleServiceTests
{
    private readonly Mock<ICropCycleRepository> _cropCycleRepositoryMock;
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly CropCycleService _cropCycleService;

    public CropCycleServiceTests()
    {
        _cropCycleRepositoryMock = new Mock<ICropCycleRepository>();
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        
        var mapper = MapperHelper.CreateMapper();
        
        _cropCycleService = new CropCycleService(
            _cropCycleRepositoryMock.Object,
            _fieldRepositoryMock.Object,
            _auditLogServiceMock.Object,
            mapper);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateCropCycleDto
        {
            FieldId = 1,
            CropType = "WHEAT",
            PlantingDate = DateTime.UtcNow.AddDays(-30),
            ExpectedHarvestDate = DateTime.UtcNow.AddDays(60),
            GrowthStage = "VEGETATIVE",
            Status = "ACTIVE"
        };
        int farmId = 1, adminId = 1;
        
        var field = TestHelper.CreateTestField(1, farmId, adminId);
        var createdCycle = TestHelper.CreateTestCropCycle(2, 1, farmId);
        
        _fieldRepositoryMock.Setup(r => r.GetByIdAsync(createDto.FieldId, farmId, false))
            .ReturnsAsync(field);
        _cropCycleRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<CropCycle>()))
            .ReturnsAsync(createdCycle);

        // Act
        var result = await _cropCycleService.CreateAsync(createDto, farmId, adminId, "127.0.0.1", "TestAgent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingCropCycle_ReturnsSuccess()
    {
        // Arrange
        int id = 1, farmId = 1;
        var cropCycle = TestHelper.CreateTestCropCycle(id, 1, farmId);
        
        _cropCycleRepositoryMock.Setup(r => r.GetByIdAsync(id, farmId, false))
            .ReturnsAsync(cropCycle);

        // Act
        var result = await _cropCycleService.GetByIdAsync(id, farmId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.Id.Should().Be(id);
    }
}