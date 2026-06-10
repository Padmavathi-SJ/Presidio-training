// Application/Mappings/HarvestMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Harvest;
using AgriculturePlatform.Domain.Entities.YieldReports;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class HarvestMappingProfile : Profile
{
    public HarvestMappingProfile()
    {
        CreateMap<Harvest, HarvestDto>()
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty))
            .ForMember(dest => dest.HarvesterName, opt => opt.MapFrom(src => src.Harvester != null ? src.Harvester.Name : string.Empty))
            .ForMember(dest => dest.SubmitterName, opt => opt.MapFrom(src => src.Submitter != null ? src.Submitter.Name : string.Empty))
            .ForMember(dest => dest.ApproverName, opt => opt.MapFrom(src => src.Approver != null ? src.Approver.Name : string.Empty))
            .ForMember(dest => dest.QualityGrade, opt => opt.MapFrom(src => src.QualityGrade.HasValue ? src.QualityGrade.ToString() : string.Empty))
            .ForMember(dest => dest.HarvestMethod, opt => opt.MapFrom(src => src.HarvestMethod.HasValue ? src.HarvestMethod.ToString() : string.Empty))
            .ForMember(dest => dest.TotalValue, opt => opt.MapFrom(src => src.TotalValue));

        CreateMap<CreateHarvestDto, Harvest>()
            .ForMember(dest => dest.QualityGrade, opt => opt.Ignore())
            .ForMember(dest => dest.HarvestMethod, opt => opt.Ignore())
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
            .ForMember(dest => dest.SubmittedBy, opt => opt.Ignore())
            .ForMember(dest => dest.HarvestedBy, opt => opt.Ignore());

        CreateMap<UpdateHarvestDto, Harvest>()
            .ForMember(dest => dest.QualityGrade, opt => opt.Ignore())
            .ForMember(dest => dest.HarvestMethod, opt => opt.Ignore())
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}