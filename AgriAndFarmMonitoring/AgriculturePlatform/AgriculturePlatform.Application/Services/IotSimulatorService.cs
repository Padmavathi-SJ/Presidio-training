// AgriculturePlatform.Application/Services/IoTSimulatorService.cs
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class IoTSimulatorService : IIoTSimulatorService
{
    private readonly ISensorReadingRepository _sensorRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IAlertService _alertService;
    private readonly ILogger<IoTSimulatorService> _logger;
    private readonly Random _random = new();

    // Realistic value ranges for different sensor types
    private readonly Dictionary<string, (decimal Min, decimal Max, string Unit, decimal Trend)> _sensorRanges = new()
    {
        { "SOIL_MOISTURE", (15m, 45m, "%", 0m) },
        { "SOIL_TEMP", (15m, 35m, "°C", 0m) },
        { "AIR_TEMP", (10m, 45m, "°C", 0m) },
        { "AIR_HUMIDITY", (30m, 90m, "%", 0m) },
        { "LIGHT_INTENSITY", (0m, 1200m, "lux", 0m) },
        { "SOIL_PH", (5.5m, 7.5m, "pH", 0m) },
        { "NPK_NITROGEN", (20m, 200m, "ppm", 0m) },
        { "NPK_PHOSPHORUS", (10m, 100m, "ppm", 0m) },
        { "NPK_POTASSIUM", (50m, 300m, "ppm", 0m) },
        { "WIND_SPEED", (0m, 20m, "m/s", 0m) },
        { "RAINFALL", (0m, 50m, "mm", 0m) }
    };

    private readonly string[] _sensorTypes = { 
        "SOIL_MOISTURE", "SOIL_TEMP", "AIR_TEMP", "AIR_HUMIDITY", 
        "LIGHT_INTENSITY", "SOIL_PH", "NPK_NITROGEN", "NPK_PHOSPHORUS", 
        "NPK_POTASSIUM", "WIND_SPEED", "RAINFALL" 
    };

    public IoTSimulatorService(
        ISensorReadingRepository sensorRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IFarmRepository farmRepository,
        IAlertService alertService,
        ILogger<IoTSimulatorService> logger)
    {
        _sensorRepository = sensorRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _farmRepository = farmRepository;
        _alertService = alertService;
        _logger = logger;
    }

    public async Task GenerateSensorReadingsAsync(int farmId)
    {
        _logger.LogInformation($"Starting sensor simulation for farm {farmId}");
        
        var fields = await _fieldRepository.GetAllAsync(farmId);
        var generatedCount = 0;

        foreach (var field in fields)
        {
            var activeCropCycles = await GetActiveCropCyclesForFieldAsync(field.Id, farmId);
            
            foreach (var cropCycle in activeCropCycles)
            {
                // Generate readings for each sensor type
                foreach (var sensorType in _sensorTypes)
                {
                    var reading = await GenerateAndSaveReadingAsync(field.Id, cropCycle.Id, sensorType, farmId);
                    if (reading != null)
                    {
                        generatedCount++;
                        
                        // Check for threshold violations and create alerts
                        await _alertService.CheckAndCreateAlertAsync(
                            field.Id, cropCycle.Id, sensorType, reading.Value ?? 0, farmId, 1);
                    }
                }
            }
        }

        _logger.LogInformation($"Generated {generatedCount} sensor readings for farm {farmId}");
    }

    public async Task GenerateSensorReadingsForAllFarmsAsync()
    {
        var farms = await _farmRepository.GetAllActiveAsync();
        
        foreach (var farm in farms)
        {
            await GenerateSensorReadingsAsync(farm.Id);
        }
        
        _logger.LogInformation($"Completed sensor simulation for all {farms.Count()} farms");
    }

    public async Task CheckThresholdsAndCreateAlertsAsync(int farmId, decimal value, string sensorType, int fieldId, int cropCycleId)
    {
        await _alertService.CheckAndCreateAlertAsync(fieldId, cropCycleId, sensorType, value, farmId, 1);
    }

    public async Task RunSimulationCycleAsync(int farmId, int adminId)
    {
        _logger.LogInformation($"Running manual simulation cycle for farm {farmId}");
        
        var fields = await _fieldRepository.GetAllAsync(farmId);
        var generatedReadings = new List<SensorReading>();

        foreach (var field in fields)
        {
            var activeCropCycles = await GetActiveCropCyclesForFieldAsync(field.Id, farmId);
            
            foreach (var cropCycle in activeCropCycles)
            {
                foreach (var sensorType in _sensorTypes)
                {
                    var reading = GenerateRealisticReading(sensorType, field, cropCycle);
                    
                    var sensorReading = new SensorReading
                    {
                        FarmId = farmId,
                        FieldId = field.Id,
                        CropCycleId = cropCycle.Id,
                        SensorType = Enum.Parse<SensorTypeEnum>(sensorType),
                        Value = reading.Value,
                        Unit = reading.Unit,
                        RecordedAt = DateTime.UtcNow
                    };
                    
                    generatedReadings.Add(sensorReading);
                    
                    // Check thresholds
                    await _alertService.CheckAndCreateAlertAsync(
                        field.Id, cropCycle.Id, sensorType, reading.Value, farmId, adminId);
                }
            }
        }

        if (generatedReadings.Any())
        {
            await _sensorRepository.BulkCreateAsync(generatedReadings);
        }
        
        _logger.LogInformation($"Manual simulation cycle completed. Generated {generatedReadings.Count} readings");
    }

    public async Task<IEnumerable<object>> GetSimulationStatusAsync(int farmId)
    {
        var fields = await _fieldRepository.GetAllAsync(farmId);
        var status = new List<object>();

        foreach (var field in fields)
        {
            var activeCropCycles = await GetActiveCropCyclesForFieldAsync(field.Id, farmId);
            var latestReadings = await _sensorRepository.GetLatestPerFieldAsync(farmId);
            var fieldReadings = latestReadings.Where(r => r.FieldId == field.Id);

            status.Add(new
            {
                FieldId = field.Id,
                FieldName = field.FieldName,
                ActiveCropCycles = activeCropCycles.Count(),
                LatestReadings = fieldReadings.Select(r => new
                {
                    r.SensorType,
                    r.Value,
                    r.Unit,
                    r.RecordedAt
                })
            });
        }

        return status;
    }

    private async Task<SensorReading?> GenerateAndSaveReadingAsync(int fieldId, int cropCycleId, string sensorType, int farmId)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, farmId);
        if (field == null) return null;

        var cropCycle = await _cropCycleRepository.GetByIdAsync(cropCycleId, farmId);
        if (cropCycle == null) return null;

        var reading = GenerateRealisticReading(sensorType, field, cropCycle);

        var sensorReading = new SensorReading
        {
            FarmId = farmId,
            FieldId = fieldId,
            CropCycleId = cropCycleId,
            SensorType = Enum.Parse<SensorTypeEnum>(sensorType),
            Value = reading.Value,
            Unit = reading.Unit,
            RecordedAt = DateTime.UtcNow
        };

        return await _sensorRepository.CreateAsync(sensorReading);
    }

    private (decimal Value, string Unit) GenerateRealisticReading(string sensorType, Field field, CropCycle cropCycle)
    {
        var range = _sensorRanges[sensorType];
        var value = (decimal)(_random.NextDouble() * (double)(range.Max - range.Min) + (double)range.Min);
        
        // Apply realistic patterns based on sensor type
        value = sensorType switch
        {
            "AIR_TEMP" => ApplyDiurnalPattern(value),
            "SOIL_MOISTURE" => ApplyMoisturePattern(value, cropCycle),
            "LIGHT_INTENSITY" => ApplyLightPattern(value),
            "SOIL_PH" => ApplyPhPattern(value, cropCycle),
            "RAINFALL" => ApplyRainfallPattern(value),
            _ => Math.Round(value, 2)
        };

        // Clamp to valid range
        value = Math.Clamp(value, range.Min, range.Max);
        
        return (Math.Round(value, 2), range.Unit);
    }

    private decimal ApplyDiurnalPattern(decimal baseValue)
    {
        var hour = DateTime.UtcNow.AddHours(5).Hour; // IST timezone
        var dayFactor = (decimal)Math.Sin(Math.PI * (hour - 6) / 12);
        return baseValue + (dayFactor * 10m);
    }

    private decimal ApplyMoisturePattern(decimal baseValue, CropCycle cropCycle)
    {
        // Moisture decreases as crop grows (more water consumption)
        var growthFactor = cropCycle.GrowthStage switch
        {
            GrowthStageEnum.GERMINATION => 0.9m,
            GrowthStageEnum.SEEDLING => 0.85m,
            GrowthStageEnum.VEGETATIVE => 0.7m,
            GrowthStageEnum.FLOWERING => 0.6m,
            GrowthStageEnum.FRUITING => 0.55m,
            GrowthStageEnum.MATURITY => 0.65m,
            _ => 0.8m
        };
        
        return baseValue * growthFactor;
    }

    private decimal ApplyLightPattern(decimal baseValue)
    {
        var hour = DateTime.UtcNow.AddHours(5).Hour;
        if (hour < 6 || hour > 18) return 0;
        
        var peakHour = 12;
        var intensityFactor = 1 - Math.Abs(hour - peakHour) / 12m;
        return baseValue * intensityFactor;
    }

    private decimal ApplyPhPattern(decimal baseValue, CropCycle cropCycle)
    {
        // Different crops prefer different pH levels
        var cropPreference = cropCycle.CropType switch
        {
            CropTypeEnum.WHEAT => 6.4m,
            CropTypeEnum.MAIZE => 6.0m,
            CropTypeEnum.RICE => 5.5m,
            CropTypeEnum.HAZELNUT => 6.5m,
            _ => 6.0m
        };
        
        // Tend towards preferred pH
        return (baseValue + cropPreference) / 2;
    }

    private decimal ApplyRainfallPattern(decimal baseValue)
    {
        // Rainfall is more likely during monsoon months (June-September)
        var month = DateTime.UtcNow.Month;
        var isMonsoon = month >= 6 && month <= 9;
        
        if (!isMonsoon) return baseValue * 0.3m;
        
        // Random chance of rain
        var rainChance = _random.NextDouble();
        return rainChance > 0.7 ? baseValue : 0;
    }

    private async Task<IEnumerable<CropCycle>> GetActiveCropCyclesForFieldAsync(int fieldId, int farmId)
    {
        var allCycles = await _cropCycleRepository.GetAllAsync(farmId);
        return allCycles.Where(c => c.FieldId == fieldId && 
                                     c.Status == TaskStatusEnum.IN_PROGRESS);
    }
}