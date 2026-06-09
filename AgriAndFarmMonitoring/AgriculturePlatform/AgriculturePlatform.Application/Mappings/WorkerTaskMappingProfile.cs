// AgriculturePlatform.Application/Mappings/WorkerTaskMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.WorkerTask;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class WorkerTaskMappingProfile : Profile
{
    public WorkerTaskMappingProfile()
    {
        CreateMap<WorkerTask, WorkerTaskDto>()
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Name : string.Empty))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty))
            .ForMember(dest => dest.CompletedAt, opt => opt.MapFrom(src => src.Status == TaskStatusEnum.COMPLETED ? src.UpdatedAt : null))
            .ForMember(dest => dest.CompletionNotes, opt => opt.Ignore());
    }
}