// AgriculturePlatform.Application/Services/IoTSimulatorService.cs
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class IoTSimulatorService : IIoTSimulatorService
{
    private readonly ISensorReadingRepository _sensorRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IFarmRepository _farmRepository;
    private readonly IAlertService _alertService;
    private readonly IAlertRepository _alertRepository;
     private readonly IAlertNotificationService _alertNotificationService;
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
        IAlertRepository alertRepository, 
        IAlertNotificationService alertNotificationService,
        ILogger<IoTSimulatorService> logger)
    {
        _sensorRepository = sensorRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _farmRepository = farmRepository;
        _alertService = alertService;
        _alertRepository = alertRepository;
        _alertNotificationService = alertNotificationService;
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
            GrowthStageEnum.MATURE => 0.65m,
            GrowthStageEnum.READY_FOR_HARVEST => 0.6m, // ✅ ADDED
        GrowthStageEnum.HARVESTED => 0.5m,          // ✅ ADDED
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
// Application/Services/IoTSimulatorService.cs (Add test alert generation)

public async Task GenerateTestCriticalAlertsAsync(int farmId, int adminId)
{
    _logger.LogInformation($"Generating test critical alerts for farm {farmId}");
    
    var fields = await _fieldRepository.GetAllAsync(farmId);
    var alertTypes = new[] { "DROUGHT_STRESS", "HEAT_STRESS", "PEST_INFESTATION", "SOIL_PH_ALERT" };
    
    foreach (var field in fields)
    {
        var activeCropCycles = await GetActiveCropCyclesForFieldAsync(field.Id, farmId);
        
        foreach (var cropCycle in activeCropCycles)
        {
            // Generate a critical alert for each crop cycle
            var randomAlertType = alertTypes[_random.Next(alertTypes.Length)];
            var criticalValue = randomAlertType switch
            {
                "DROUGHT_STRESS" => 10m, // Very low moisture
                "HEAT_STRESS" => 42m,     // Very high temperature
                "PEST_INFESTATION" => 100m, // Severe infestation
                "SOIL_PH_ALERT" => 8.5m,    // High pH
                _ => 0
            };
            
            var alert = new Alert
            {
                FarmId = farmId,
                AdminId = adminId,
                FieldId = field.Id,
                CropCycleId = cropCycle.Id,
                AlertType = Enum.Parse<AlertTypeEnum>(randomAlertType),
                Severity = AlertSeverityEnum.CRITICAL,
                Message = $"TEST ALERT: {randomAlertType} - CRITICAL level detected! Immediate action required!",
                SensorValue = criticalValue,
                ThresholdValue = randomAlertType == "DROUGHT_STRESS" ? 25m : 
                                randomAlertType == "HEAT_STRESS" ? 35m :
                                randomAlertType == "SOIL_PH_ALERT" ? 7.0m : 0,
                IsResolved = false,
                CreatedAt = DateTime.UtcNow
            };
            
            await _alertRepository.CreateAsync(alert);
            
            // Send email notifications for test critical alerts
            await _alertNotificationService.SendAlertNotificationsAsync(alert, farmId);
            
            _logger.LogWarning($"Generated CRITICAL test alert for field {field.FieldName}: {randomAlertType}");
        }
    }
    
    _logger.LogInformation($"Test critical alerts generation completed for farm {farmId}");
}

public async Task GenerateRandomSeverityReadingsAsync(int farmId, int adminId)
{
    _logger.LogInformation($"Generating random severity readings for farm {farmId}");
    
    var fields = await _fieldRepository.GetAllAsync(farmId);
    var severities = new[] { "LOW", "MEDIUM", "HIGH", "CRITICAL" };
    
    foreach (var field in fields)
    {
        var activeCropCycles = await GetActiveCropCyclesForFieldAsync(field.Id, farmId);
        
        foreach (var cropCycle in activeCropCycles)
        {
            // Generate readings with different severity levels
            foreach (var sensorType in _sensorTypes)
            {
                var severity = severities[_random.Next(severities.Length)];
                var reading = GenerateReadingWithSeverity(sensorType, severity);
                
                var sensorReading = new SensorReading
                {
                    FarmId = farmId,
                    AdminId = adminId,
                    FieldId = field.Id,
                    CropCycleId = cropCycle.Id,
                    SensorType = Enum.Parse<SensorTypeEnum>(sensorType),
                    Value = reading.Value,
                    Unit = reading.Unit,
                    RecordedAt = DateTime.UtcNow
                };
                
                await _sensorRepository.CreateAsync(sensorReading);
                
                // Create alert for high/medium severity
                if (severity == "HIGH" || severity == "CRITICAL")
                {
                    var alert = new Alert
                    {
                        FarmId = farmId,
                        AdminId = adminId,
                        FieldId = field.Id,
                        CropCycleId = cropCycle.Id,
                        AlertType = GetAlertTypeForSensor(sensorType),
                        Severity = Enum.Parse<AlertSeverityEnum>(severity),
                        Message = $"{severity} severity alert: {sensorType} reading is {reading.Value} {reading.Unit}",
                        SensorValue = reading.Value,
                        ThresholdValue = GetThresholdForSeverity(sensorType, severity),
                        IsResolved = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    await _alertRepository.CreateAsync(alert);
                    await _alertNotificationService.SendAlertNotificationsAsync(alert, farmId);
                }
                
                _logger.LogInformation($"Generated {severity} reading for {field.FieldName} - {sensorType}: {reading.Value} {reading.Unit}");
            }
        }
    }
}

private (decimal Value, string Unit) GenerateReadingWithSeverity(string sensorType, string severity)
{
    var range = _sensorRanges[sensorType];
    decimal value;
    
    switch (severity)
    {
        case "CRITICAL":
            value = range.Min + (range.Max - range.Min) * 0.9m; // Near max
            break;
        case "HIGH":
            value = range.Min + (range.Max - range.Min) * 0.8m;
            break;
        case "MEDIUM":
            value = range.Min + (range.Max - range.Min) * 0.6m;
            break;
        default:
            value = range.Min + (range.Max - range.Min) * 0.3m;
            break;
    }
    
    return (Math.Round(value, 2), range.Unit);
}

private decimal GetThresholdForSeverity(string sensorType, string severity)
{
    var range = _sensorRanges[sensorType];
    return severity == "CRITICAL" ? range.Max - 5 : range.Max - 10;
}

private AlertTypeEnum GetAlertTypeForSensor(string sensorType)
{
    return sensorType switch
    {
        "SOIL_MOISTURE" => AlertTypeEnum.DROUGHT_STRESS,
        "SOIL_TEMP" => AlertTypeEnum.HEAT_STRESS,
        "AIR_TEMP" => AlertTypeEnum.HEAT_STRESS,
        "AIR_HUMIDITY" => AlertTypeEnum.DROUGHT_STRESS,
        "SOIL_PH" => AlertTypeEnum.SOIL_PH_ALERT,
        "NPK_NITROGEN" => AlertTypeEnum.NUTRIENT_DEFICIENCY,
        "NPK_PHOSPHORUS" => AlertTypeEnum.NUTRIENT_DEFICIENCY,
        "NPK_POTASSIUM" => AlertTypeEnum.NUTRIENT_DEFICIENCY,
        _ => AlertTypeEnum.DROUGHT_STRESS
    };
}

public async Task GenerateHourlyRandomAlertAsync(int farmId, int adminId)
{
    _logger.LogInformation($"Generating hourly random alert for farm {farmId}");
    
    var fields = await _fieldRepository.GetAllAsync(farmId);
    if (!fields.Any()) return;

    // Pick a random field
    var randomField = fields.OrderBy(f => _random.Next()).FirstOrDefault();
    if (randomField == null) return;

    var activeCropCycles = await GetActiveCropCyclesForFieldAsync(randomField.Id, farmId);
    var activeCycle = activeCropCycles.FirstOrDefault();
    if (activeCycle == null) return;

    var alertTypes = new[] { "DROUGHT_STRESS", "HEAT_STRESS", "PEST_INFESTATION", "SOIL_PH_ALERT" };
    var randomAlertType = alertTypes[_random.Next(alertTypes.Length)];
    
    decimal criticalValue = randomAlertType switch
    {
        "DROUGHT_STRESS" => 10m,
        "HEAT_STRESS" => 42m,
        "PEST_INFESTATION" => 100m,
        "SOIL_PH_ALERT" => 8.5m,
        _ => 0
    };

    var alert = new Alert
    {
        FarmId = farmId,
        AdminId = adminId,
        FieldId = randomField.Id,
        CropCycleId = activeCycle.Id,
        AlertType = Enum.Parse<AlertTypeEnum>(randomAlertType),
        Severity = AlertSeverityEnum.CRITICAL,
        Message = $"HOURLY ALERT: {randomAlertType} detected in {randomField.FieldName}! Immediate action required.",
        SensorValue = criticalValue,
        ThresholdValue = randomAlertType == "DROUGHT_STRESS" ? 25m : 
                        randomAlertType == "HEAT_STRESS" ? 35m :
                        randomAlertType == "SOIL_PH_ALERT" ? 7.0m : 0,
        IsResolved = false,
        CreatedAt = DateTime.UtcNow
    };
    
    await _alertRepository.CreateAsync(alert);
    await _alertNotificationService.SendAlertNotificationsAsync(alert, farmId);
    
    _logger.LogWarning($"Generated hourly random CRITICAL alert for field {randomField.FieldName}: {randomAlertType}");
}

}