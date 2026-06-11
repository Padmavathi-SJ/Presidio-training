// API/BackgroundServices/ScheduledReportBackgroundService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.API.BackgroundServices;

public class ScheduledReportBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ScheduledReportBackgroundService> _logger;

    public ScheduledReportBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<ScheduledReportBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var reportService = scope.ServiceProvider.GetRequiredService<IYieldReportService>();
                
                await reportService.ProcessScheduledReportsAsync();
                
                // Check every hour
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing scheduled reports");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }
    }
}