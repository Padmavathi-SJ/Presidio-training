using AutoMapper;
using Microsoft.Extensions.Logging; 
using AgriculturePlatform.Application.Common;
using AgriculturePlatform.Application.DTOs.Weather;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Enums;

namespace AgriculturePlatform.Application.Services;

public class WeatherService : IWeatherService
{
    private readonly IWeatherRepository _weatherRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly IAdminRepository _adminRepository;  
    private readonly IWeatherApiService _weatherApiService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IWeatherRepository weatherRepository,
        IFieldRepository fieldRepository,
        IAdminRepository adminRepository, 
        IWeatherApiService weatherApiService,
        IAuditLogService auditLogService,
        IMapper mapper,
        ILogger<WeatherService> logger)
    {
        _weatherRepository = weatherRepository;
        _fieldRepository = fieldRepository;
        _adminRepository = adminRepository;  
        _weatherApiService = weatherApiService;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ApiResponse<WeatherDataDto>> GetCurrentWeatherAsync(int fieldId, int farmId, int? adminId = null)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, farmId);
        if (field == null)
        {
            return ApiResponse<WeatherDataDto>.Fail($"Field with ID {fieldId} not found");
        }

        var weather = await _weatherRepository.GetLatestByFieldAsync(fieldId, farmId);
        
        if (weather == null)
        {
            if (field.Latitude.HasValue && field.Longitude.HasValue)
            {
                try
                {
                    var apiWeather = await _weatherApiService.GetCurrentWeatherAsync(field.Latitude.Value, field.Longitude.Value);
                    
                    WeatherConditionEnum? condition = null;
                    if (!string.IsNullOrWhiteSpace(apiWeather.Condition))
                    {
                        try
                        {
                            condition = Enum.Parse<WeatherConditionEnum>(apiWeather.Condition, true);
                        }
                        catch (ArgumentException)
                        {
                            _logger.LogWarning($"Unknown weather condition: {apiWeather.Condition}");
                        }
                    }
                    
                    var newWeather = new WeatherData
                    {
                        FarmId = farmId,
                        AdminId = adminId ?? 0,
                        FieldId = fieldId,
                        Temperature = apiWeather.Temperature,
                        Humidity = apiWeather.Humidity,
                        WindSpeed = apiWeather.WindSpeed,
                        Condition = condition,
                        RecordedAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = adminId ?? 0
                    };
                    weather = await _weatherRepository.CreateAsync(newWeather);
                    _logger.LogInformation($"Created new weather record for field {fieldId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to fetch weather for field {fieldId}");
                    return ApiResponse<WeatherDataDto>.Fail($"Weather data not available: {ex.Message}");
                }
            }
            else
            {
                return ApiResponse<WeatherDataDto>.Fail("Field location not set. Please add latitude/longitude.");
            }
        }
        
        var result = _mapper.Map<WeatherDataDto>(weather);
        result.FieldName = field.FieldName;
        
        return ApiResponse<WeatherDataDto>.Ok(result);
    }

    public async Task<ApiResponse<WeatherForecastDto>> GetForecastAsync(int fieldId, int farmId)
    {
        var field = await _fieldRepository.GetByIdAsync(fieldId, farmId);
        if (field == null)
        {
            return ApiResponse<WeatherForecastDto>.Fail($"Field with ID {fieldId} not found");
        }

        if (!field.Latitude.HasValue || !field.Longitude.HasValue)
        {
            return ApiResponse<WeatherForecastDto>.Fail("Field location not set. Please add latitude/longitude.");
        }

        try
        {
            var forecast = await _weatherApiService.GetWeatherForecastAsync(field.Latitude.Value, field.Longitude.Value);
            forecast.FieldId = fieldId;
            forecast.FieldName = field.FieldName;
            return ApiResponse<WeatherForecastDto>.Ok(forecast);
        }
        catch (Exception ex)
        {
            return ApiResponse<WeatherForecastDto>.Fail($"Failed to get forecast: {ex.Message}");
        }
    }

    public async Task<ApiResponse<PagedResult<WeatherDataDto>>> GetWeatherHistoryAsync(WeatherHistoryFilterDto filter, int farmId)
    {
        var paginationParams = new PaginationParams
        {
            Page = filter.Page ?? 1,
            PageSize = filter.PageSize ?? 30
        };

        var pagedResult = await _weatherRepository.GetPagedHistoryAsync(
            farmId, filter.FieldId, filter.FromDate, filter.ToDate, paginationParams);

        var dtos = _mapper.Map<List<WeatherDataDto>>(pagedResult.Items);
        
        var result = new PagedResult<WeatherDataDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };

        return ApiResponse<PagedResult<WeatherDataDto>>.Ok(result);
    }

    public async Task<ApiResponse<List<WeatherAlertDto>>> GetActiveWeatherAlertsAsync(int farmId)
    {
        var fields = await _fieldRepository.GetAllAsync(farmId);
        var allAlerts = new List<WeatherAlertDto>();

        foreach (var field in fields)
        {
            if (field.Latitude.HasValue && field.Longitude.HasValue)
            {
                var alerts = await _weatherApiService.GetWeatherAlertsAsync(field.Latitude.Value, field.Longitude.Value);
                foreach (var alert in alerts)
                {
                    alert.FieldId = field.Id;
                    alert.FieldName = field.FieldName;
                    allAlerts.Add(alert);
                }
            }
        }

        return ApiResponse<List<WeatherAlertDto>>.Ok(allAlerts);
    }

    public async Task<ApiResponse<WeatherDataDto>> AddManualWeatherEntryAsync(ManualWeatherEntryDto dto, int farmId, int adminId)
    {
        var field = await _fieldRepository.GetByIdAsync(dto.FieldId, farmId);
        if (field == null)
        {
            return ApiResponse<WeatherDataDto>.Fail($"Field with ID {dto.FieldId} not found");
        }

        var weather = new WeatherData
        {
            FarmId = farmId,
            AdminId = adminId,
            FieldId = dto.FieldId,
            Temperature = dto.Temperature,
            Humidity = dto.Humidity,
            RainfallMm = dto.RainfallMm,
            WindSpeed = dto.WindSpeed,
            Condition = !string.IsNullOrWhiteSpace(dto.Condition) 
                ? Enum.Parse<WeatherConditionEnum>(dto.Condition, true) 
                : null,
            RecordedAt = dto.RecordedAt,
            CreatedBy = adminId,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _weatherRepository.CreateAsync(weather);
        
        await _auditLogService.LogCreateAsync(farmId, adminId, "WeatherData", created.Id, created, null, null);

        var result = _mapper.Map<WeatherDataDto>(created);
        result.FieldName = field.FieldName;
        
        return ApiResponse<WeatherDataDto>.Ok(result, "Weather data added successfully");
    }

    public async Task<ApiResponse<bool>> UpdateWeatherDataAsync(int id, ManualWeatherEntryDto dto, int farmId, int adminId)
    {
        var weather = await _weatherRepository.GetByIdAsync(id, farmId);
        if (weather == null)
        {
            return ApiResponse<bool>.Fail($"Weather record with ID {id} not found");
        }

        var oldWeather = _mapper.Map<WeatherData>(weather);

        if (dto.Temperature.HasValue)
            weather.Temperature = dto.Temperature;
        if (dto.Humidity.HasValue)
            weather.Humidity = dto.Humidity;
        if (dto.RainfallMm.HasValue)
            weather.RainfallMm = dto.RainfallMm;
        if (dto.WindSpeed.HasValue)
            weather.WindSpeed = dto.WindSpeed;
        if (!string.IsNullOrWhiteSpace(dto.Condition))
            weather.Condition = Enum.Parse<WeatherConditionEnum>(dto.Condition, true);
        
        weather.UpdatedAt = DateTime.UtcNow;
        weather.UpdatedBy = adminId;

        await _weatherRepository.UpdateAsync(weather);

        await _auditLogService.LogUpdateAsync(farmId, adminId, "WeatherData", weather.Id, oldWeather, weather, null, null);

        return ApiResponse<bool>.Ok(true, "Weather data updated successfully");
    }

    public async Task<ApiResponse<bool>> DeleteWeatherDataAsync(int id, int farmId, int adminId)
    {
        var weather = await _weatherRepository.GetByIdAsync(id, farmId);
        if (weather == null)
        {
            return ApiResponse<bool>.Fail($"Weather record with ID {id} not found");
        }

        await _weatherRepository.DeleteAsync(weather);
        
        await _auditLogService.LogDeleteAsync(farmId, adminId, "WeatherData", weather.Id, weather, null, null);

        return ApiResponse<bool>.Ok(true, "Weather data deleted successfully");
    }

    public async Task<ApiResponse<bool>> RefreshWeatherDataAsync(int fieldId, int farmId, int adminId)
    {
        try
        {
            var field = await _fieldRepository.GetByIdAsync(fieldId, farmId);
            if (field == null || !field.Latitude.HasValue || !field.Longitude.HasValue)
            {
                return ApiResponse<bool>.Fail($"Field {fieldId} has no coordinates");
            }

            var admin = await _adminRepository.GetByIdAsync(adminId);
            if (admin == null)
            {
                var admins = await _adminRepository.GetByFarmIdAsync(farmId);
                admin = admins.FirstOrDefault(a => a.IsActive);
                if (admin == null)
                {
                    return ApiResponse<bool>.Fail($"No active admin found for farm {farmId}");
                }
                adminId = admin.Id;
            }

            var weatherData = await _weatherApiService.GetCurrentWeatherAsync(field.Latitude.Value, field.Longitude.Value);
            
            WeatherConditionEnum? condition = null;
            if (!string.IsNullOrWhiteSpace(weatherData.Condition))
            {
                try
                {
                    condition = Enum.Parse<WeatherConditionEnum>(weatherData.Condition, true);
                }
                catch (ArgumentException)
                {
                    _logger.LogWarning($"Unknown weather condition: {weatherData.Condition}");
                }
            }
            
            var weatherRecord = new WeatherData
            {
                FarmId = farmId,
                AdminId = adminId,
                FieldId = fieldId,
                Temperature = weatherData.Temperature,
                Humidity = weatherData.Humidity,
                RainfallMm = weatherData.RainfallMm,
                WindSpeed = weatherData.WindSpeed,
                Condition = condition,
                RecordedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = adminId
            };
            
            await _weatherRepository.CreateAsync(weatherRecord);
            await CheckAndCreateWeatherAlertsAsync(weatherRecord, farmId, adminId);
            
            _logger.LogInformation($"Weather data refreshed for field {fieldId}");
            return ApiResponse<bool>.Ok(true, "Weather data refreshed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to refresh weather for field {fieldId}");
            return ApiResponse<bool>.Fail($"Failed to refresh weather: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> RefreshAllFieldsWeatherAsync(int farmId, int adminId)
    {
        try
        {
            var fields = await _fieldRepository.GetAllAsync(farmId);
            var successCount = 0;

            foreach (var field in fields)
            {
                if (field.Latitude.HasValue && field.Longitude.HasValue)
                {
                    var result = await RefreshWeatherDataAsync(field.Id, farmId, adminId);
                    if (result.Success) successCount++;
                }
            }

            return ApiResponse<bool>.Ok(true, $"Refreshed weather for {successCount} of {fields.Count()} fields");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to refresh all fields weather for farm {farmId}");
            return ApiResponse<bool>.Fail($"Failed to refresh weather: {ex.Message}");
        }
    }

    public async Task<ApiResponse<WeatherApiSettingsDto>> GetApiSettingsAsync(int farmId)
    {
        var settings = new WeatherApiSettingsDto
        {
            ApiProvider = "OpenWeatherMap",
            UpdateIntervalMinutes = 60,
            AutoUpdateEnabled = true
        };
        
        return ApiResponse<WeatherApiSettingsDto>.Ok(settings);
    }

    public async Task<ApiResponse<bool>> UpdateApiSettingsAsync(WeatherApiSettingsDto dto, int farmId)
    {
        await Task.CompletedTask;
        return ApiResponse<bool>.Ok(true, "Settings updated successfully");
    }

    private async Task CheckAndCreateWeatherAlertsAsync(WeatherData weather, int farmId, int adminId)
    {
        var alerts = new List<WeatherAlert>();
        
        if (weather.Temperature > 35)
        {
            alerts.Add(new WeatherAlert
            {
                FarmId = farmId,
                AdminId = adminId,
                FieldId = weather.FieldId,
                AlertType = WeatherAlertTypeEnum.HEAT_WAVE,
                Severity = WeatherAlertSeverityEnum.WARNING,
                Title = "High Temperature Alert",
                Message = $"Temperature reached {weather.Temperature}°C which exceeds safe threshold of 35°C",
                Temperature = weather.Temperature,
                AlertTime = DateTime.UtcNow
            });
        }
        
        if (weather.RainfallMm > 50)
        {
            alerts.Add(new WeatherAlert
            {
                FarmId = farmId,
                AdminId = adminId,
                FieldId = weather.FieldId,
                AlertType = WeatherAlertTypeEnum.HEAVY_RAIN,
                Severity = WeatherAlertSeverityEnum.WARNING,
                Title = "Heavy Rainfall Alert",
                Message = $"Rainfall of {weather.RainfallMm}mm detected",
                RainfallMm = weather.RainfallMm,
                AlertTime = DateTime.UtcNow
            });
        }
        
        if (weather.WindSpeed > 15)
        {
            alerts.Add(new WeatherAlert
            {
                FarmId = farmId,
                AdminId = adminId,
                FieldId = weather.FieldId,
                AlertType = WeatherAlertTypeEnum.HIGH_WIND,
                Severity = WeatherAlertSeverityEnum.ADVISORY,
                Title = "High Wind Alert",
                Message = $"Wind speed reached {weather.WindSpeed} m/s",
                WindSpeed = weather.WindSpeed,
                AlertTime = DateTime.UtcNow
            });
        }
    }
}