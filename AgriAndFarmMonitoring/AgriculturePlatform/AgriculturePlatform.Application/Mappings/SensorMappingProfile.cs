// AgriculturePlatform.Application/Mappings/SensorMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Sensor;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class SensorMappingProfile : Profile
{
    public SensorMappingProfile()
    {
        CreateMap<SensorReading, SensorReadingDto>()
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty))
            .ForMember(dest => dest.SensorType, opt => opt.MapFrom(src => src.SensorType.ToString()))
            .ForMember(dest => dest.IsThresholdViolation, opt => opt.MapFrom(src => src.Alerts != null && src.Alerts.Any()));  // Now works
    }
}