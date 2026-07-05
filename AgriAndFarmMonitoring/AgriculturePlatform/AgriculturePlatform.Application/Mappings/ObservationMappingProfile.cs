// Application/Mappings/ObservationMappingProfile.cs
using AutoMapper;
using AgriculturePlatform.Application.DTOs.Observation;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;
using AgriculturePlatform.Application.Interfaces;
using System.Linq;
using System.Collections.Generic;

namespace AgriculturePlatform.Application.Mappings;

public class ObservationMappingProfile : Profile
{
    public ObservationMappingProfile()
    {
        CreateMap<Observation, ObservationDto>()
            .ForMember(dest => dest.CropHealth, 
                opt => opt.MapFrom(src => src.CropHealth.HasValue ? FormatCropHealthForDisplay(src.CropHealth.Value) : string.Empty))
            .ForMember(dest => dest.FarmName, opt => opt.MapFrom(src => src.Farm != null ? src.Farm.FarmName : string.Empty))
            .ForMember(dest => dest.FieldName, opt => opt.MapFrom(src => src.Field != null ? src.Field.FieldName : string.Empty))
            .ForMember(dest => dest.CropType, opt => opt.MapFrom(src => src.CropCycle != null && src.CropCycle.CropType != null ? src.CropCycle.CropType.ToString() : string.Empty))
            .ForMember(dest => dest.WorkerName, opt => opt.MapFrom(src => src.Worker != null ? src.Worker.Name : string.Empty))
            .ForMember(dest => dest.ValidatorName, opt => opt.MapFrom(src => src.Validator != null ? src.Validator.Name : string.Empty))
            .ForMember(dest => dest.ImagePath, opt => opt.MapFrom<ObservationImagePathResolver>())
            .ForMember(dest => dest.AdditionalImagePaths, opt => opt.MapFrom<ObservationAdditionalImagePathsResolver>());

        CreateMap<CreateObservationDto, Observation>()
            .ForMember(dest => dest.CropHealth, opt => opt.Ignore())
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
            .ForMember(dest => dest.WorkerId, opt => opt.Ignore())
            .ForMember(dest => dest.Field, opt => opt.Ignore())
            .ForMember(dest => dest.CropCycle, opt => opt.Ignore())
            .ForMember(dest => dest.Worker, opt => opt.Ignore())
            .ForMember(dest => dest.Farm, opt => opt.Ignore())
            .ForMember(dest => dest.Admin, opt => opt.Ignore())
            .ForMember(dest => dest.Validator, opt => opt.Ignore())
            // Set default validation status
            .ForMember(dest => dest.ValidationStatus, opt => opt.MapFrom(src => "pending"))
            .ForMember(dest => dest.IsImageVerified, opt => opt.MapFrom(src => false));

        CreateMap<UpdateObservationDto, Observation>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));
    }

    private string FormatCropHealthForDisplay(CropHealthEnum health)
    {
        return health switch
        {
            CropHealthEnum.EXCELLENT => "Excellent",
            CropHealthEnum.GOOD => "Good",
            CropHealthEnum.AVERAGE => "Average",
            CropHealthEnum.POOR => "Poor",
            CropHealthEnum.CRITICAL => "Critical",
            _ => health.ToString()
        };
    }
}

public class ObservationImagePathResolver : IValueResolver<Observation, ObservationDto, string?>
{
    private readonly IFileStorageService? _fileStorageService;

    public ObservationImagePathResolver()
    {
        _fileStorageService = null;
    }

    public ObservationImagePathResolver(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public string? Resolve(Observation source, ObservationDto destination, string? destMember, ResolutionContext context)
    {
        if (string.IsNullOrEmpty(source.ImagePath))
            return null;

        if (_fileStorageService == null)
            return source.ImagePath;

        return _fileStorageService.GetDownloadUrl(source.ImagePath);
    }
}

public class ObservationAdditionalImagePathsResolver : IValueResolver<Observation, ObservationDto, List<string>?>
{
    private readonly IFileStorageService? _fileStorageService;

    public ObservationAdditionalImagePathsResolver()
    {
        _fileStorageService = null;
    }

    public ObservationAdditionalImagePathsResolver(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    public List<string>? Resolve(Observation source, ObservationDto destination, List<string>? destMember, ResolutionContext context)
    {
        if (source.AdditionalImagePaths == null)
            return null;

        if (_fileStorageService == null)
            return source.AdditionalImagePaths;

        return source.AdditionalImagePaths
            .Select(path => _fileStorageService.GetDownloadUrl(path))
            .ToList();
    }
}