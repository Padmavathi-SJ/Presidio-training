// AgriculturePlatform.Application/Services/WeatherService.cs
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
    private readonly IWeatherApiService _weatherApiService;
    private readonly IAuditLogService _auditLogService;
    private readonly IMapper _mapper;
    private readonly ILogger<WeatherService> _logger;

    public WeatherService(
        IWeatherRepository weatherRepository,
        IFieldRepository fieldRepository,
        IWeatherApiService weatherApiService,
        IAuditLogService auditLogService,
        IMapper mapper,
        ILogger<WeatherService> logger)
    {
        _weatherRepository = weatherRepository;
        _fieldRepository = fieldRepository;
        _weatherApiService = weatherApiService;
        _auditLogService = auditLogService;
        _mapper = mapper;
        _logger = logger;
    }

    // =============================================
    // READ OPERATIONS (Both Admin & Worker)
    // =============================================
// AgriculturePlatform.Application/Services/WeatherService.cs
// Ensure the method signature matches exactly

// AgriculturePlatform.Application/Services/WeatherService.cs

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
        // Try to fetch from API
        if (field.Latitude.HasValue && field.Longitude.HasValue)
        {
            try
            {
                var apiWeather = await _weatherApiService.GetCurrentWeatherAsync(field.Latitude.Value, field.Longitude.Value);
                var newWeather = new WeatherData
                {
                    FarmId = farmId,
                    AdminId = adminId ?? 0,  // Use 0 for system/worker if no adminId
                    FieldId = fieldId,
                    Temperature = apiWeather.Temperature,
                    Humidity = apiWeather.Humidity,
                    WindSpeed = apiWeather.WindSpeed,
                    Condition = Enum.Parse<WeatherConditionEnum>(apiWeather.Condition),
                    RecordedAt = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = adminId ?? 0
                };
                weather = await _weatherRepository.CreateAsync(newWeather);
                _logger.LogInformation($"Created new weather record for field {fieldId} with temp {apiWeather.Temperature}°C");
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

    // =============================================
    // ADMIN ONLY OPERATIONS
    // =============================================

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

        weather.Temperature = dto.Temperature ?? weather.Temperature;
        weather.Humidity = dto.Humidity ?? weather.Humidity;
        weather.RainfallMm = dto.RainfallMm ?? weather.RainfallMm;
        weather.WindSpeed = dto.WindSpeed ?? weather.WindSpeed;
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

// In WeatherService.cs, update RefreshWeatherDataAsync to handle null values

public async Task<ApiResponse<bool>> RefreshWeatherDataAsync(int fieldId, int farmId, int adminId)
{
    var field = await _fieldRepository.GetByIdAsync(fieldId, farmId);
    if (field == null)
    {
        return ApiResponse<bool>.Fail($"Field with ID {fieldId} not found");
    }

    if (!field.Latitude.HasValue || !field.Longitude.HasValue)
    {
        return ApiResponse<bool>.Fail("Field location not set");
    }

    try
    {
        var apiWeather = await _weatherApiService.GetCurrentWeatherAsync(field.Latitude.Value, field.Longitude.Value);
        
        // Check if we got valid data
        if (apiWeather.Temperature == 0 && apiWeather.Humidity == 0 && apiWeather.WindSpeed == 0)
        {
            return ApiResponse<bool>.Fail("Received invalid weather data from API");
        }
        
        var weather = new WeatherData
        {
            FarmId = farmId,
            AdminId = adminId,
            FieldId = fieldId,
            Temperature = apiWeather.Temperature,
            Humidity = apiWeather.Humidity,
            WindSpeed = apiWeather.WindSpeed,
            Condition = Enum.Parse<WeatherConditionEnum>(apiWeather.Condition),
            RecordedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = adminId
        };

        await _weatherRepository.CreateAsync(weather);
        
        _logger.LogInformation($"Weather data saved for field {fieldId}: Temp={apiWeather.Temperature}°C, Humidity={apiWeather.Humidity}%");
        
        return ApiResponse<bool>.Ok(true, "Weather data refreshed successfully");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, $"Failed to refresh weather for field {fieldId}");
        return ApiResponse<bool>.Fail($"Failed to refresh: {ex.Message}");
    }
}

public async Task<ApiResponse<bool>> RefreshAllFieldsWeatherAsync(int farmId, int adminId)
{
    var fields = await _fieldRepository.GetAllAsync(farmId);
    var successCount = 0;

    foreach (var field in fields)
    {
        if (field.Latitude.HasValue && field.Longitude.HasValue)
        {
            // FIX: Pass adminId to RefreshWeatherDataAsync
            var result = await RefreshWeatherDataAsync(field.Id, farmId, adminId);
            if (result.Success) successCount++;
        }
    }

    return ApiResponse<bool>.Ok(true, $"Refreshed weather for {successCount} of {fields.Count()} fields");
}
    public async Task<ApiResponse<WeatherApiSettingsDto>> GetApiSettingsAsync(int farmId)
    {
        // Return settings from configuration (or stored in database)
        // For now, return default
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
        // Save settings (you can store in a Settings table or appsettings)
        // For now, just return success
        await Task.CompletedTask;
        return ApiResponse<bool>.Ok(true, "Settings updated successfully");
    }
}