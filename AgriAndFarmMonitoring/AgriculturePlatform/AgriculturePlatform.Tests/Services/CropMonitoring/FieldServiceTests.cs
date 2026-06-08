// AgriculturePlatform.Tests/Services/CropMonitoring/FieldServiceTests.cs
using Moq;
using Xunit;
using FluentAssertions;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Application.Services;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Services.CropMonitoring;

public class FieldServiceTests
{
    private readonly Mock<IFieldRepository> _fieldRepositoryMock;
    private readonly Mock<IExcelService> _excelServiceMock;
    private readonly Mock<IAuditLogService> _auditLogServiceMock;
    private readonly FieldService _fieldService;

    public FieldServiceTests()
    {
        _fieldRepositoryMock = new Mock<IFieldRepository>();
        _excelServiceMock = new Mock<IExcelService>();
        _auditLogServiceMock = new Mock<IAuditLogService>();
        
        var mapper = MapperHelper.CreateMapper(); // Use the helper
        
        _fieldService = new FieldService(
            _fieldRepositoryMock.Object,
            _excelServiceMock.Object,
            _auditLogServiceMock.Object,
            mapper);
    }

    [Fact]
    public async Task CreateAsync_ValidInput_ReturnsSuccess()
    {
        // Arrange
        var createDto = new CreateFieldDto
        {
            FieldName = "New Field",
            Location = "New Location",
            AreaHectares = 15.5m,
            SoilType = "LOAMY",
            Status = "ACTIVE"
        };
        int farmId = 1, adminId = 1;
        
        var createdField = TestHelper.CreateTestField(2, farmId, adminId);
        createdField.FieldName = "New Field";
        
        _fieldRepositoryMock.Setup(r => r.FieldNameExistsAsync(It.IsAny<string>(), It.IsAny<int>(), null))
            .ReturnsAsync(false);
        _fieldRepositoryMock.Setup(r => r.CreateAsync(It.IsAny<Field>()))
            .ReturnsAsync(createdField);
        _fieldRepositoryMock.Setup(r => r.GetActiveCropsCountAsync(It.IsAny<int>()))
            .ReturnsAsync(0);

        // Act
        var result = await _fieldService.CreateAsync(createDto, farmId, adminId, "127.0.0.1", "TestAgent");

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Data.FieldName.Should().Be("New Field");
    }
}