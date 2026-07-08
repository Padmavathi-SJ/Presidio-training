// AgriculturePlatform.Application/Services/WorkerFieldService.cs
using AutoMapper;
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class WorkerFieldService : IWorkerFieldService
{
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IMapper _mapper;

    public WorkerFieldService(
        IWorkerFieldAssignmentRepository assignmentRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IMapper mapper)
    {
        _assignmentRepository = assignmentRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _mapper = mapper;
    }

    public async Task<ApiResponse<List<WorkerFieldListDto>>> GetMyAssignedFieldsAsync(int workerId, int farmId)
    {
        // Get all active assignments for this worker
        var assignments = await _assignmentRepository.GetWorkerActiveAssignmentsAsync(workerId, farmId);
        
        if (assignments == null || !assignments.Any())
        {
            return ApiResponse<List<WorkerFieldListDto>>.Ok(new List<WorkerFieldListDto>(), "No fields assigned");
        }

        var result = new List<WorkerFieldListDto>();

        foreach (var assignment in assignments)
        {
            if (assignment.Field == null) continue;

            // Get active crop cycles count for this field
            var activeCropCount = await _cropCycleRepository.GetActiveCountByFieldAsync(assignment.FieldId);

            result.Add(new WorkerFieldListDto
            {
                AssignmentId = assignment.Id,
                FieldId = assignment.FieldId,
                FieldName = assignment.Field.FieldName,
                Location = assignment.Field.Location,
                AreaHectares = assignment.Field.AreaHectares,
                SoilType = assignment.Field.SoilType?.ToString(),
                Status = assignment.Field.Status?.ToString(),
                AssignedDate = assignment.AssignedDate,
                ActiveCropCount = activeCropCount
            });
        }

        return ApiResponse<List<WorkerFieldListDto>>.Ok(result);
    }

    public async Task<ApiResponse<WorkerFieldDetailDto>> GetAssignedFieldDetailAsync(int fieldId, int workerId, int farmId)
    {
        // Verify worker has access to this field
        var hasAccess = await _assignmentRepository.HasWorkerAccessToFieldAsync(workerId, fieldId, farmId);
        
        if (!hasAccess)
        {
            return ApiResponse<WorkerFieldDetailDto>.Fail("You don't have access to this field");
        }

        // Get field details
        var field = await _fieldRepository.GetByIdAsync(fieldId, farmId);
        
        if (field == null)
        {
            return ApiResponse<WorkerFieldDetailDto>.Fail("Field not found");
        }

        // Get the assignment details
        var assignments = await _assignmentRepository.GetWorkerActiveAssignmentsAsync(workerId, farmId);
        var assignment = assignments.FirstOrDefault(a => a.FieldId == fieldId);

        // Get all crop cycles for this field
        var allCropCycles = await _cropCycleRepository.GetAllAsync(farmId, false);
        var fieldCropCycles = allCropCycles.Where(c => c.FieldId == fieldId);

        var cropCycleDtos = new List<WorkerCropCycleDto>();

        foreach (var cropCycle in fieldCropCycles)
        {
            var daysSincePlanting = cropCycle.PlantingDate.HasValue 
                ? (DateTime.UtcNow - cropCycle.PlantingDate.Value).Days 
                : 0;
            
            var daysToHarvest = cropCycle.ExpectedHarvestDate.HasValue 
                ? (cropCycle.ExpectedHarvestDate.Value - DateTime.UtcNow).Days 
                : 0;
            
            // Calculate growth progress based on growth stage
            var growthProgress = CalculateGrowthProgress(cropCycle.GrowthStage);

            cropCycleDtos.Add(new WorkerCropCycleDto
            {
                Id = cropCycle.Id,
                CropType = cropCycle.CropType?.ToString(),
                PlantingDate = cropCycle.PlantingDate,
                ExpectedHarvestDate = cropCycle.ExpectedHarvestDate,
                GrowthStage = cropCycle.GrowthStage?.ToString(),
                Status = cropCycle.Status?.ToString(),
                DaysSincePlanting = daysSincePlanting > 0 ? daysSincePlanting : 0,
                DaysToHarvest = daysToHarvest > 0 ? daysToHarvest : 0,
                GrowthProgressPercent = growthProgress
            });
        }

        var result = new WorkerFieldDetailDto
        {
            AssignmentId = assignment?.Id ?? 0,
            FieldId = field.Id,
            FieldName = field.FieldName,
            Location = field.Location,
            AreaHectares = field.AreaHectares,
            SoilType = field.SoilType?.ToString(),
            Status = field.Status?.ToString(),
            AssignedDate = assignment?.AssignedDate,
            Notes = assignment?.Notes,
            CreatedAt = field.CreatedAt,
            CropCycles = cropCycleDtos
        };

        return ApiResponse<WorkerFieldDetailDto>.Ok(result);
    }

    private double? CalculateGrowthProgress(GrowthStageEnum? growthStage)
    {
        if (!growthStage.HasValue) return 0;

        var stageProgress = new Dictionary<GrowthStageEnum, double>
       {
        { GrowthStageEnum.PLANTED, 5 },
        { GrowthStageEnum.GERMINATION, 10 },
        { GrowthStageEnum.SEEDLING, 25 },
        { GrowthStageEnum.VEGETATIVE, 50 },
        { GrowthStageEnum.FLOWERING, 65 },
        { GrowthStageEnum.FRUITING, 80 },
        { GrowthStageEnum.MATURE, 90 },              // ✅ CHANGED from MATURITY to MATURE
        { GrowthStageEnum.READY_FOR_HARVEST, 98 },   // ✅ ADDED
        { GrowthStageEnum.HARVESTED, 100 },          // ✅ ADDED
        { GrowthStageEnum.OVERRIPE, 100 }            // ✅ ADDED
    };

        return stageProgress.GetValueOrDefault(growthStage.Value, 0);
    }
}