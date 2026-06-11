// Application/Mappings/QualityCheckMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.QualityCheck;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class QualityCheckMappingProfile : Profile
{
    public QualityCheckMappingProfile()
    {
        CreateMap<QualityCheck, QualityCheckDto>()
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.CheckerName, opt => opt.MapFrom(src => src.Checker != null ? src.Checker.Name : string.Empty))
            .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.Name : string.Empty))
            .ForMember(dest => dest.HarvestBatchNumber, opt => opt.MapFrom(src => src.Harvest != null ? src.Harvest.BatchNumber : string.Empty))
            .ForMember(dest => dest.HarvestQuantity, opt => opt.MapFrom(src => src.Harvest != null ? src.Harvest.QuantityKg : (decimal?)null))
            .ForMember(dest => dest.FinalGrade, opt => opt.MapFrom(src => src.FinalGrade.HasValue ? src.FinalGrade.ToString() : string.Empty))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));  // ✅ Add this line

        CreateMap<CreateQualityCheckDto, QualityCheck>()
            .ForMember(dest => dest.FinalGrade, opt => opt.Ignore())
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
            .ForMember(dest => dest.ApprovalStatus, opt => opt.MapFrom(src => "PENDING"))
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes));  // ✅ Add this line

        CreateMap<UpdateQualityCheckDto, QualityCheck>()
            .ForMember(dest => dest.FinalGrade, opt => opt.Ignore())
            .ForMember(dest => dest.Notes, opt => opt.MapFrom(src => src.Notes))  // ✅ Add this line
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}