// AgriculturePlatform.API/BackgroundServices/WeatherUpdateBackgroundService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.API.BackgroundServices;

public class WeatherUpdateBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeatherUpdateBackgroundService> _logger;

    public WeatherUpdateBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WeatherUpdateBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }


    private async Task<List<Farm>> GetActiveFarmsAsync(IServiceScope scope)
    {
        var farmRepository = scope.ServiceProvider.GetRequiredService<IFarmRepository>();
        return await farmRepository.GetAllActiveAsync();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Weather Update Background Service started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var fieldRepository = scope.ServiceProvider.GetRequiredService<IFieldRepository>();
                var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
                var adminRepository = scope.ServiceProvider.GetRequiredService<IAdminRepository>();
                
                var farms = await GetActiveFarmsAsync(scope);
                
                foreach (var farm in farms)
                {
                    var admins = await adminRepository.GetByFarmIdAsync(farm.Id);
                    var admin = admins.FirstOrDefault(a => a.IsActive);
                    
                    if (admin == null)
                    {
                        _logger.LogWarning($"No active admin found for farm {farm.Id}, skipping weather update");
                        continue;
                    }
                    
                    var fields = await fieldRepository.GetAllAsync(farm.Id);
                    
                    foreach (var field in fields.Where(f => f.Latitude.HasValue && f.Longitude.HasValue))
                    {
                        try
                        {
                            await weatherService.RefreshWeatherDataAsync(field.Id, farm.Id, admin.Id);
                            _logger.LogInformation($"Weather updated for field {field.FieldName}");
                            
                            await Task.Delay(1000, stoppingToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to update weather for field {field.Id}");
                        }
                    }
                }
                
                _logger.LogInformation("Weather data update completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in weather update cycle");
            }
            
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
        
        _logger.LogInformation("Weather Update Background Service stopped");
    }
}