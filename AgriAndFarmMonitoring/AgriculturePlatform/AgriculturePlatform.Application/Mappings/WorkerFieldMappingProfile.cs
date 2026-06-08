// AgriculturePlatform.Application/Mappings/WorkerFieldMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Mappings;

public class WorkerFieldMappingProfile : Profile
{
    public WorkerFieldMappingProfile()
    {
        CreateMap<Field, WorkerFieldListDto>()
            .ForMember(dest => dest.AssignmentId, opt => opt.Ignore())
            .ForMember(dest => dest.ActiveCropCount, opt => opt.Ignore())
            .ForMember(dest => dest.SoilType, opt => opt.MapFrom(src => src.SoilType != null ? src.SoilType.ToString() : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? src.Status.ToString() : null));

        CreateMap<Field, WorkerFieldDetailDto>()
            .ForMember(dest => dest.AssignmentId, opt => opt.Ignore())
            .ForMember(dest => dest.AssignedDate, opt => opt.Ignore())
            .ForMember(dest => dest.Notes, opt => opt.Ignore())
            .ForMember(dest => dest.CropCycles, opt => opt.Ignore())
            .ForMember(dest => dest.SoilType, opt => opt.MapFrom(src => src.SoilType != null ? src.SoilType.ToString() : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? src.Status.ToString() : null));

        CreateMap<CropCycle, WorkerCropCycleDto>()
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropType != null ? src.CropType.ToString() : null))
            .ForMember(dest => dest.GrowthStage, opt => opt.MapFrom(src => src.GrowthStage != null ? src.GrowthStage.ToString() : null))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status != null ? src.Status.ToString() : null))
            .ForMember(dest => dest.DaysToHarvest, opt => opt.Ignore())
            .ForMember(dest => dest.DaysSincePlanting, opt => opt.Ignore())
            .ForMember(dest => dest.GrowthProgressPercent, opt => opt.Ignore());
    }
}