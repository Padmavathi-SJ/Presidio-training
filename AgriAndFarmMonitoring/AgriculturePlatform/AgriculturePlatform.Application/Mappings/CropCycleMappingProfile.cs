// Application/Mappings/CropCycleMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.CropCycle;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class CropCycleMappingProfile : Profile
{
    public CropCycleMappingProfile()
    {
        CreateMap<CropCycle, CropCycleDto>()
            .ForMember(dest => dest.CropType, 
                opt => opt.MapFrom(src => src.CropType.HasValue ? src.CropType.Value.ToString() : string.Empty))
            .ForMember(dest => dest.GrowthStage, 
                opt => opt.MapFrom(src => src.GrowthStage.HasValue ? src.GrowthStage.Value.ToString() : string.Empty))
            .ForMember(dest => dest.PreviousGrowthStage, 
                opt => opt.MapFrom(src => src.PreviousGrowthStage.HasValue ? src.PreviousGrowthStage.Value.ToString() : string.Empty))
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => src.Status.HasValue ? src.Status.Value.ToString() : string.Empty))
            .ForMember(dest => dest.FarmName, 
                opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.FieldName, 
                opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            // ✅ Computed properties
            .ForMember(dest => dest.GrowthPercentage, 
                opt => opt.MapFrom(src => src.GrowthPercentage))
            .ForMember(dest => dest.DaysUntilHarvest, 
                opt => opt.MapFrom(src => src.DaysUntilHarvest))
            .ForMember(dest => dest.IsOverdue, 
                opt => opt.MapFrom(src => src.IsOverdue))
            .ForMember(dest => dest.IsReadyForHarvest, 
                opt => opt.MapFrom(src => src.GrowthStage == GrowthStageEnum.READY_FOR_HARVEST));

        CreateMap<CreateCropCycleDto, CropCycle>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FarmId, opt => opt.Ignore())
            .ForMember(dest => dest.AdminId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedBy, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore())
            .ForMember(dest => dest.ActualHarvestDate, opt => opt.Ignore())
            .ForMember(dest => dest.LastStageUpdate, opt => opt.Ignore())
            .ForMember(dest => dest.PreviousGrowthStage, opt => opt.Ignore())
            .ForMember(dest => dest.GrowthStage, opt => opt.Ignore());

        CreateMap<UpdateCropCycleDto, CropCycle>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}