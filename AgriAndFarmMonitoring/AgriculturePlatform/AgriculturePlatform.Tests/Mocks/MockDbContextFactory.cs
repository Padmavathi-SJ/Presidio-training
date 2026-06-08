// AgriculturePlatform.Tests/Mocks/MockRepositories.cs
using Moq;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Application.Common;

namespace AgriculturePlatform.Tests.Mocks;

public static class MockRepositories
{
    public static Mock<IAdminRepository> GetMockAdminRepository(List<Admin> admins)
    {
        var mockRepo = new Mock<IAdminRepository>();
        
        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => admins.FirstOrDefault(a => a.Email == email));
        
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((int id) => admins.FirstOrDefault(a => a.Id == id));
        
        mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => admins.Any(a => a.Email == email));
        
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<Admin>()))
            .ReturnsAsync((Admin admin) =>
            {
                admin.Id = admins.Count + 1;
                admins.Add(admin);
                return admin;
            });
        
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Admin>()))
            .Returns(Task.CompletedTask);
        
        return mockRepo;
    }

    public static Mock<IWorkerRepository> GetMockWorkerRepository(List<Worker> workers)
    {
        var mockRepo = new Mock<IWorkerRepository>();
        
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int farmId, bool includeDeleted) => 
                workers.FirstOrDefault(w => w.Id == id && w.FarmId == farmId));
        
        mockRepo.Setup(r => r.GetByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((string email) => workers.FirstOrDefault(w => w.Email == email));
        
        mockRepo.Setup(r => r.EmailExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync((string email, int farmId, int? excludeId) => 
                workers.Any(w => w.Email == email && w.FarmId == farmId && w.Id != excludeId));
        
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<Worker>()))
            .ReturnsAsync((Worker worker) =>
            {
                worker.Id = workers.Count + 1;
                workers.Add(worker);
                return worker;
            });
        
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Worker>()))
            .Returns(Task.CompletedTask);
        
        return mockRepo;
    }

    public static Mock<IFieldRepository> GetMockFieldRepository(List<Field> fields)
    {
        var mockRepo = new Mock<IFieldRepository>();
        
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int farmId, bool includeDeleted) => 
                fields.FirstOrDefault(f => f.Id == id && f.FarmId == farmId));
        
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int farmId, bool includeDeleted) => 
                fields.Where(f => f.FarmId == farmId).ToList());
        
        mockRepo.Setup(r => r.FieldNameExistsAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int?>()))
            .ReturnsAsync((string name, int farmId, int? excludeId) => 
                fields.Any(f => f.FieldName == name && f.FarmId == farmId && f.Id != excludeId));
        
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<Field>()))
            .ReturnsAsync((Field field) =>
            {
                field.Id = fields.Count + 1;
                fields.Add(field);
                return field;
            });
        
        mockRepo.Setup(r => r.UpdateAsync(It.IsAny<Field>()))
            .Returns(Task.CompletedTask);
        
        mockRepo.Setup(r => r.GetActiveCropsCountAsync(It.IsAny<int>()))
            .ReturnsAsync(0);
        
        return mockRepo;
    }

    public static Mock<ICropCycleRepository> GetMockCropCycleRepository(List<CropCycle> cropCycles)
    {
        var mockRepo = new Mock<ICropCycleRepository>();
        
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int id, int farmId, bool includeDeleted) => 
                cropCycles.FirstOrDefault(c => c.Id == id && c.FarmId == farmId));
        
        mockRepo.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<bool>()))
            .ReturnsAsync((int farmId, bool includeDeleted) => 
                cropCycles.Where(c => c.FarmId == farmId).ToList());
        
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<CropCycle>()))
            .ReturnsAsync((CropCycle cycle) =>
            {
                cycle.Id = cropCycles.Count + 1;
                cropCycles.Add(cycle);
                return cycle;
            });
        
        return mockRepo;
    }

    public static Mock<IWorkerFieldAssignmentRepository> GetMockAssignmentRepository(List<WorkerFieldAssignment> assignments)
    {
        var mockRepo = new Mock<IWorkerFieldAssignmentRepository>();
        
        mockRepo.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int id, int farmId) => 
                assignments.FirstOrDefault(a => a.Id == id && a.FarmId == farmId));
        
        mockRepo.Setup(r => r.IsFieldAssignedToWorkerAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int fieldId, int workerId, int farmId) => 
                assignments.Any(a => a.FieldId == fieldId && a.WorkerId == workerId && a.FarmId == farmId && a.IsActive));
        
        mockRepo.Setup(r => r.GetWorkerAssignedFieldsAsync(It.IsAny<int>(), It.IsAny<int>()))
            .ReturnsAsync((int workerId, int farmId) => 
                assignments.Where(a => a.WorkerId == workerId && a.FarmId == farmId && a.IsActive).ToList());
        
        mockRepo.Setup(r => r.CreateAsync(It.IsAny<WorkerFieldAssignment>()))
            .ReturnsAsync((WorkerFieldAssignment assignment) =>
            {
                assignment.Id = assignments.Count + 1;
                assignments.Add(assignment);
                return assignment;
            });
        
        return mockRepo;
    }
}