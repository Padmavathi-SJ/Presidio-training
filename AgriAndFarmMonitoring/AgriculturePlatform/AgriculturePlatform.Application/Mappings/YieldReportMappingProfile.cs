// Application/Mappings/YieldReportMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.YieldReport;
using AgriculturePlatform.Domain.Entities.YieldReports;
using System.Text.Json;

namespace AgriculturePlatform.Application.Mappings;

public class YieldReportMappingProfile : Profile
{
    public YieldReportMappingProfile()
    {
        CreateMap<YieldReport, YieldReportDto>()
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.FieldBreakdown, opt => opt.MapFrom(src => DeserializeJson<List<FieldYieldBreakdownDto>>(src.FieldBreakdownJson)))
            .ForMember(dest => dest.CropTypeBreakdown, opt => opt.MapFrom(src => DeserializeJson<List<CropTypeYieldBreakdownDto>>(src.CropTypeBreakdownJson)))
            .ForMember(dest => dest.MonthlyTrend, opt => opt.MapFrom(src => DeserializeJson<List<MonthlyYieldTrendDto>>(src.MonthlyTrendJson)))
            .ForMember(dest => dest.QualityDistribution, opt => opt.MapFrom(src => DeserializeJson<List<QualityDistributionDto>>(src.QualityDistributionJson)));

        CreateMap<CreateYieldReportDto, YieldReport>()
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
            .ForMember(dest => dest.TotalYieldKg, opt => opt.Ignore())
            .ForMember(dest => dest.AverageYieldPerHectare, opt => opt.Ignore())
            .ForMember(dest => dest.TotalHarvests, opt => opt.Ignore())
            .ForMember(dest => dest.AveragePricePerKg, opt => opt.Ignore())
            .ForMember(dest => dest.TotalValue, opt => opt.Ignore())
            .ForMember(dest => dest.AverageQualityGrade, opt => opt.Ignore())
            .ForMember(dest => dest.PassRate, opt => opt.Ignore())
            .ForMember(dest => dest.RejectionRate, opt => opt.Ignore())
            .ForMember(dest => dest.FieldBreakdownJson, opt => opt.Ignore())
            .ForMember(dest => dest.CropTypeBreakdownJson, opt => opt.Ignore())
            .ForMember(dest => dest.MonthlyTrendJson, opt => opt.Ignore())
            .ForMember(dest => dest.QualityDistributionJson, opt => opt.Ignore());
    }

    private static T? DeserializeJson<T>(string? json) where T : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        return JsonSerializer.Deserialize<T>(json);
    }
}