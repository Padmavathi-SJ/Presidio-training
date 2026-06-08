// AgriculturePlatform.Application/Mappings/TaskMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Task;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Mappings;

public class TaskMappingProfile : Profile
{
    public TaskMappingProfile()
    {
        CreateMap<WorkerTask, TaskDto>()
            .ForMember(dest => dest.TaskName, opt => opt.MapFrom(src => src.TaskName.ToString()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.Priority, opt => opt.MapFrom(src => src.Priority.ToString()))
            .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Name : string.Empty))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty));

        CreateMap<CreateTaskDto, WorkerTask>()
            .ForMember(dest => dest.TaskName, opt => opt.Ignore())
            .ForMember(dest => dest.Priority, opt => opt.Ignore())
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
            .ForMember(dest => dest.Status, opt => opt.Ignore());

        CreateMap<UpdateTaskDto, WorkerTask>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}