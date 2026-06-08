// AgriculturePlatform.Application/Mappings/WorkerProfileMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Domain.Entities.WorkerManagement;

namespace AgriculturePlatform.Application.Mappings;

public class WorkerProfileMappingProfile : Profile
{
    public WorkerProfileMappingProfile()
    {
        CreateMap<Worker, WorkerProfileDto>()
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty));
        
        CreateMap<UpdateWorkerProfileDto, Worker>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }
}