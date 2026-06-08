// AgriculturePlatform.Application/Mappings/FieldMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Field;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Mappings;

public class FieldMappingProfile : Profile
{
    public FieldMappingProfile()
    {
        // Map Field → FieldDto
        CreateMap<Field, FieldDto>()
            .ForMember(dest => dest.SoilType, 
                opt => opt.MapFrom(src => src.SoilType != null ? src.SoilType.ToString() : null))
            .ForMember(dest => dest.Status, 
                opt => opt.MapFrom(src => src.Status != null ? src.Status.ToString() : null))
            .ForMember(dest => dest.FarmName, 
                opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.IsDeleted, 
                opt => opt.MapFrom(src => src.IsDeleted))
            .ForMember(dest => dest.DeletedAt, 
                opt => opt.MapFrom(src => src.DeletedAt))
            .ForMember(dest => dest.Latitude,    // ADD THIS
                opt => opt.MapFrom(src => src.Latitude))
            .ForMember(dest => dest.Longitude,   // ADD THIS
                opt => opt.MapFrom(src => src.Longitude));
        
        // Map CreateFieldDto → Field
        CreateMap<CreateFieldDto, Field>()
            .ForMember(dest => dest.SoilType, opt => opt.Ignore())
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
            .ForMember(dest => dest.CropCycles, opt => opt.Ignore())
            .ForMember(dest => dest.Alerts, opt => opt.Ignore())
            .ForMember(dest => dest.Observations, opt => opt.Ignore())
            .ForMember(dest => dest.WeatherData, opt => opt.Ignore())
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Harvests, opt => opt.Ignore())
            .ForMember(dest => dest.SensorReadings, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.Latitude, opt => opt.MapFrom(src => src.Latitude))     // ADD THIS
            .ForMember(dest => dest.Longitude, opt => opt.MapFrom(src => src.Longitude));  // ADD THIS
        
        // Map UpdateFieldDto → Field (only non-null values)
        CreateMap<UpdateFieldDto, Field>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}