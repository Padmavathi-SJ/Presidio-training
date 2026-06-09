// AgriculturePlatform.Application/Mappings/AlertMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Alert;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class AlertMappingProfile : Profile
{
    public AlertMappingProfile()
    {
        CreateMap<Alert, AlertDto>()
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty))
            .ForMember(dest => dest.AlertType, opt => opt.MapFrom(src => src.AlertType.ToString()))
            .ForMember(dest => dest.Severity, opt => opt.MapFrom(src => src.Severity.ToString()))
            .ForMember(dest => dest.SensorValue, opt => opt.MapFrom(src => src.SensorValue))  // Now exists
            .ForMember(dest => dest.ThresholdValue, opt => opt.Ignore());  // Keep as ignore if not in entity

        CreateMap<AlertThreshold, AlertThresholdDto>()
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropType))
            .ForMember(dest => dest.GrowthStage, opt => opt.MapFrom(src => src.GrowthStage));

        CreateMap<CreateAlertThresholdDto, AlertThreshold>()
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
            .ForMember(dest => dest.IsActive, opt => opt.Ignore());
    }
}