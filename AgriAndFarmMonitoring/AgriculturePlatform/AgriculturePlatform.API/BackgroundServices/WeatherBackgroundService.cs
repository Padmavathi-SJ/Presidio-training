// AgriculturePlatform.API/BackgroundServices/WeatherUpdateBackgroundService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.API.BackgroundServices;

public class WeatherUpdateBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WeatherUpdateBackgroundService> _logger;
    private const int SystemAdminId = 1; // Use a default system admin ID

    public WeatherUpdateBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<WeatherUpdateBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Weather Update Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var weatherService = scope.ServiceProvider.GetRequiredService<IWeatherService>();
                var farmRepository = scope.ServiceProvider.GetRequiredService<IFarmRepository>();

                var farms = await farmRepository.GetAllActiveAsync();
                
                foreach (var farm in farms)
                {
                    _logger.LogInformation($"Updating weather data for farm: {farm.FarmName}");
                    // Use the default system admin ID
                    await weatherService.RefreshAllFieldsWeatherAsync(farm.Id, SystemAdminId);
                }

                _logger.LogInformation("Weather data update completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating weather data");
            }

            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }

        _logger.LogInformation("Weather Update Background Service is stopping");
    }
}