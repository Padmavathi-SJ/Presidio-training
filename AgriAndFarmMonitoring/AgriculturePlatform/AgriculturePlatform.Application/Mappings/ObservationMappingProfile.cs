// Application/Mappings/ObservationMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class ObservationMappingProfile : Profile
{
    public ObservationMappingProfile()
    {
        CreateMap<Observation, ObservationDto>()
            .ForMember(dest => dest.CropHealth, 
                opt => opt.MapFrom(src => src.CropHealth.HasValue ? FormatCropHealthForDisplay(src.CropHealth.Value) : string.Empty))
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty))
            .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Name : string.Empty))
            .ForMember(dest => dest.ValidatorName, opt => opt.MapFrom(src => src.Validator != null ? src.Validator.Name : string.Empty))
            .ForMember(dest => dest.ImageUrls, opt => opt.Ignore());

        CreateMap<CreateObservationDto, Observation>()
            .ForMember(dest => dest.CropHealth, opt => opt.Ignore())
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
            .ForMember(dest => dest.WorkerId, opt => opt.Ignore())
            .ForMember(dest => dest.Field, opt => opt.Ignore())
            .ForMember(dest => dest.CropCycle, opt => opt.Ignore())
            .ForMember(dest => dest.Worker, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.Validator, opt => opt.Ignore())
            // NEW: Set default validation status
            .ForMember(dest => dest.ValidationStatus, opt => opt.MapFrom(src => "pending"));

        CreateMap<UpdateObservationDto, Observation>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    private string FormatCropHealthForDisplay(CropHealthEnum health)
    {
        return health switch
        {
            CropHealthEnum.EXCELLENT => "Excellent",
            CropHealthEnum.GOOD => "Good",
            CropHealthEnum.AVERAGE => "Average",
            CropHealthEnum.POOR => "Poor",
            CropHealthEnum.CRITICAL => "Critical",
            _ => health.ToString()
        };
    }
}