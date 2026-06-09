// AgriculturePlatform.Tests/Helpers/MapperHelper.cs
using AutoMapper;
using AgriculturePlatform.Application.Mappings;

namespace AgriculturePlatform.Tests.Helpers;

public static class MapperHelper
{
    public static IMapper CreateMapper()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<FieldMappingProfile>();
            cfg.AddProfile<CropCycleMappingProfile>();
            cfg.AddProfile<WorkerMappingProfile>();
            cfg.AddProfile<WorkerProfileMappingProfile>();
            cfg.AddProfile<WorkerFieldAssignmentMappingProfile>();
            cfg.AddProfile<WorkerFieldMappingProfile>();
            cfg.AddProfile<WeatherMappingProfile>();
            cfg.AddProfile<TaskMappingProfile>(); 
            cfg.AddProfile<WorkerTaskMappingProfile>();
            cfg.AddProfile<SensorMappingProfile>();
            cfg.AddProfile<AlertMappingProfile>();
            

        });
        
        return config.CreateMapper();
    }
}