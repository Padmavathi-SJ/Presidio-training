// AgriculturePlatform.Application/Mappings/WeatherAlertMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums; 

namespace AgriculturePlatform.Application.Mappings;

public class WeatherAlertMappingProfile : Profile
{
    public WeatherAlertMappingProfile()
    {
        CreateMap<WeatherAlert, WeatherAlertDto>()
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.AlertType, opt => opt.MapFrom(src => src.AlertType.ToString()))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()));

        CreateMap<WeatherAlertCreateDto, WeatherAlert>()
            .ForMember(dest => dest.AlertType, opt => opt.MapFrom(src => Enum.Parse<WeatherAlertTypeEnum>(src.AlertType, true)))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => Enum.Parse<WeatherAlertSeverityEnum>(src.Severity, true)))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.FarmId, opt => opt.Ignore())
            .ForMember(dest => dest.AdminId, opt => opt.Ignore())
            .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.UpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.AlertTime, opt => opt.Ignore())
            .ForMember(dest => dest.IsAcknowledged, opt => opt.Ignore())
            .ForMember(dest => dest.AcknowledgedBy, opt => opt.Ignore())
            .ForMember(dest => dest.AcknowledgedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Acknowledger, opt => opt.Ignore())
            .ForMember(dest => dest.Field, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.IsDeleted, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedAt, opt => opt.Ignore())
            .ForMember(dest => dest.DeletedBy, opt => opt.Ignore());

        CreateMap<WeatherAlertUpdateDto, WeatherAlert>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}