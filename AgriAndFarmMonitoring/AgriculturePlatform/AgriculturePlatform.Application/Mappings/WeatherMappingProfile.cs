// AgriculturePlatform.Application/Mappings/WeatherMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Mappings;

public class WeatherMappingProfile : Profile
{
    public WeatherMappingProfile()
    {
        CreateMap<WeatherData, WeatherDataDto>()
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.Condition, opt => opt.MapFrom(src => src.Condition != null ? src.Condition.ToString() : null));

        CreateMap<ManualWeatherEntryDto, WeatherData>()
            .ForMember(dest => dest.Condition, opt => opt.Ignore())
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
            .ForMember(dest => dest.RecordedAt, opt => opt.Ignore())
            .ForMember(dest => dest.Field, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore());
    }
}