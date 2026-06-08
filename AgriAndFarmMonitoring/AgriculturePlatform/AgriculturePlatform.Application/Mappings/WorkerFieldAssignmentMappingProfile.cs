// AgriculturePlatform.Application/Mappings/WorkerFieldAssignmentMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.WorkerField;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Mappings;

public class WorkerFieldAssignmentMappingProfile : Profile
{
    public WorkerFieldAssignmentMappingProfile()
    {
        CreateMap<WorkerFieldAssignment, WorkerFieldAssignmentDto>()
            .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Name : string.Empty))
            .ForMember(dest => dest.WorkerEmail, opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Email : string.Empty))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.FieldLocation, opt => opt.MapFrom(src => src.Field != null ? src.Field.Location : string.Empty))
            .ForMember(dest => dest.FieldAreaHectares, opt => opt.MapFrom(src => src.Field != null ? src.Field.AreaHectares : null))
            .ForMember(dest => dest.FieldSoilType, opt => opt.MapFrom(src => src.Field != null && src.Field.SoilType != null ? src.Field.SoilType.ToString() : string.Empty));

        CreateMap<AssignFieldToWorkerDto, WorkerFieldAssignment>()
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
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.Worker, opt => opt.Ignore())
            .ForMember(dest => dest.Field, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore());
    }
}