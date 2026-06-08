// AgriculturePlatform.Tests/Helpers/TestHelper.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;
using System.Security.Cryptography;
using System.Text;

namespace AgriculturePlatform.Tests.Helpers;

public static class TestHelper
{
    // Exactly 32 characters = 256 bits (required for HS256)
    public const string TestJwtSecretKey = "12345678901234567890123456789012";
    
    public static string HashPassword(string password)
    {
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }

    public static Admin CreateTestAdmin(int id = 1, int farmId = 1)
    {
        return new Admin
        {
            Id = id,
            FarmId = farmId,
            Name = $"Test Admin {id}",
            Email = $"admin{id}@test.com",
            PasswordHash = HashPassword("Password123"),
            Phone = "1234567890",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Worker CreateTestWorker(int id = 1, int farmId = 1, int adminId = 1)
    {
        return new Worker
        {
            Id = id,
            FarmId = farmId,
            AdminId = adminId,
            Name = $"Test Worker {id}",
            Email = $"worker{id}@test.com",
            PasswordHash = HashPassword("WorkerPass123"),
            Phone = "9876543210",
            Role = "LABOR",
            HireDate = DateTime.UtcNow,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static Field CreateTestField(int id = 1, int farmId = 1, int adminId = 1)
    {
        return new Field
        {
            Id = id,
            FarmId = farmId,
            AdminId = adminId,
            FieldName = $"Test Field {id}",
            Location = $"Test Location {id}",
            AreaHectares = 10.5m,
            SoilType = SoilTypeEnum.LOAMY,
            Status = FieldStatusEnum.ACTIVE,
            Latitude = 40.7128,
            Longitude = -74.0060,
            CreatedAt = DateTime.UtcNow
        };
    }

    public static CropCycle CreateTestCropCycle(int id = 1, int fieldId = 1, int farmId = 1)
    {
        return new CropCycle
        {
            Id = id,
            FarmId = farmId,
            AdminId = 1,
            FieldId = fieldId,
            CropType = CropTypeEnum.WHEAT,
            PlantingDate = DateTime.UtcNow.AddDays(-30),
            ExpectedHarvestDate = DateTime.UtcNow.AddDays(60),
            GrowthStage = GrowthStageEnum.VEGETATIVE,
            Status = TaskStatusEnum.IN_PROGRESS,
            CreatedAt = DateTime.UtcNow
        };
    }

// In TestHelper.cs, update CreateTestAssignment to optionally include Field
public static WorkerFieldAssignment CreateTestAssignment(int id = 1, int workerId = 1, int fieldId = 1, int farmId = 1, Field? field = null)
{
    return new WorkerFieldAssignment
    {
        Id = id,
        FarmId = farmId,
        AdminId = 1,
        WorkerId = workerId,
        FieldId = fieldId,
        Field = field ?? CreateTestField(fieldId, farmId, 1),
        IsActive = true,
        AssignedDate = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    };
}


}