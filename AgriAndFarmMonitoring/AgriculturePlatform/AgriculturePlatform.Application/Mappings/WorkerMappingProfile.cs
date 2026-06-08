// AgriculturePlatform.Application/Mappings/WorkerMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Worker;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using System.Security.Cryptography;
using System.Text;

namespace AgriculturePlatform.Application.Mappings;

public class WorkerMappingProfile : Profile
{
    public WorkerMappingProfile()
    {
        // Map Worker → WorkerDto
        CreateMap<Worker, WorkerDto>()
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.LastLoginDaysAgo, opt => opt.Ignore());

        // Map CreateWorkerDto → Worker
        CreateMap<CreateWorkerDto, Worker>()
            .ForMember(dest => dest.PasswordHash, opt => opt.MapFrom(src => HashPassword(src.Password)))
            .ForMember(dest => dest.HireDate, opt => opt.MapFrom(src => src.HireDate ?? DateTime.UtcNow))
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
            .ForMember(dest => dest.Tasks, opt => opt.Ignore())
            .ForMember(dest => dest.Observations, opt => opt.Ignore())
            .ForMember(dest => dest.Harvests, opt => opt.Ignore())
            .ForMember(dest => dest.QualityChecks, opt => opt.Ignore())
            .ForMember(dest => dest.AuditLogs, opt => opt.Ignore())
            .ForMember(dest => dest.Notifications, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore());

        // Map UpdateWorkerDto → Worker (only non-null values)
        CreateMap<UpdateWorkerDto, Worker>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    private string HashPassword(string? password)
    {
        if (string.IsNullOrEmpty(password)) return string.Empty;
        using var sha256 = SHA256.Create();
        var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(hashedBytes);
    }
}