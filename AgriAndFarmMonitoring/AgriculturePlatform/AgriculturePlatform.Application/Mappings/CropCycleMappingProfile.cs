// AgriculturePlatform.Application/Mappings/CropCycleMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.CropCycle;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class CropCycleMappingProfile : Profile
{
    public CropCycleMappingProfile()
    {
        // Map CropCycle → CropCycleDto
        CreateMap<CropCycle, CropCycleDto>()
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropType != null ? src.CropType.ToString() : null))
            .ForMember(dest => dest.GrowthStage, opt => opt.MapFrom(src => src.GrowthStage != null ? src.GrowthStage.ToString() : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => MapStatusToString(src.Status)))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.IsDeleted, opt => opt.MapFrom(src => src.IsDeleted));

        // Map CreateCropCycleDto → CropCycle
        CreateMap<CreateCropCycleDto, CropCycle>()
            .ForMember(dest => dest.CropType, opt => opt.Ignore())
            .ForMember(dest => dest.GrowthStage, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.FarmId, opt => opt.Ignore())
            .ForMember(dest => dest.AdminId, opt => opt.Ignore())
            .ForMember(dest => dest.Field, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.SensorReadings, opt => opt.Ignore())
            .ForMember(dest => dest.Alerts, opt => opt.Ignore())
            .ForMember(dest => dest.Observations, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Harvests, opt => opt.Ignore())
            .ForMember(dest => dest.YieldReports, opt => opt.Ignore());

        // Map UpdateCropCycleDto → CropCycle (only non-null values)
        CreateMap<UpdateCropCycleDto, CropCycle>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    private string MapStatusToString(TaskStatusEnum? status)
    {
        if (!status.HasValue) return "PENDING";
        
        return status.Value switch
        {
            TaskStatusEnum.IN_PROGRESS => "ACTIVE",
            TaskStatusEnum.COMPLETED => "COMPLETED",
            TaskStatusEnum.CANCELLED => "CANCELLED",
            _ => "PENDING"
        };
    }
}