// AgriculturePlatform.Tests/Fixtures/TestDataFixtures.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Tests.Helpers;

namespace AgriculturePlatform.Tests.Fixtures;

public class TestDataFixtures
{
    public List<Admin> GetSampleAdmins()
    {
        return new List<Admin>
        {
            TestHelper.CreateTestAdmin(1, 1),
            TestHelper.CreateTestAdmin(2, 1),
            TestHelper.CreateTestAdmin(3, 2)
        };
    }

    public List<Worker> GetSampleWorkers()
    {
        return new List<Worker>
        {
            TestHelper.CreateTestWorker(1, 1, 1),
            TestHelper.CreateTestWorker(2, 1, 1),
            TestHelper.CreateTestWorker(3, 2, 2)
        };
    }

    public List<Field> GetSampleFields()
    {
        return new List<Field>
        {
            TestHelper.CreateTestField(1, 1, 1),
            TestHelper.CreateTestField(2, 1, 1),
            TestHelper.CreateTestField(3, 2, 2)
        };
    }

    public List<CropCycle> GetSampleCropCycles()
    {
        return new List<CropCycle>
        {
            TestHelper.CreateTestCropCycle(1, 1, 1),
            TestHelper.CreateTestCropCycle(2, 1, 1),
            TestHelper.CreateTestCropCycle(3, 2, 2)
        };
    }
}