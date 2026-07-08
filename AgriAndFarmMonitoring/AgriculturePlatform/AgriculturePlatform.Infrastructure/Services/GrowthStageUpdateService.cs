// Infrastructure/Services/GrowthStageUpdateService.cs
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Infrastructure.Context;
using AgriculturePlatform.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgriculturePlatform.Infrastructure.Services;

public class GrowthStageUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<GrowthStageUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromHours(6); // Run every 6 hours

    public GrowthStageUpdateService(
        IServiceProvider serviceProvider,
        ILogger<GrowthStageUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Growth Stage Update Service started");

        // Run immediately on startup
        await UpdateAllGrowthStages(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_updateInterval, stoppingToken);
                await UpdateAllGrowthStages(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating growth stages");
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        _logger.LogInformation("Growth Stage Update Service stopped");
    }

    private async Task UpdateAllGrowthStages(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<GrowthStageUpdateService>>();

        try
        {
            // Get all active crop cycles that need updating
            var cropCycles = await context.CropCycles
                .Where(c => c.AutoUpdateGrowthStage &&
                            c.Status != TaskStatusEnum.COMPLETED &&
                            c.Status != TaskStatusEnum.CANCELLED &&
                            c.PlantingDate.HasValue &&
                            !c.IsDeleted)
                .ToListAsync(stoppingToken);

            if (!cropCycles.Any())
                return;

            var updateCount = 0;
            var stageChangeCount = 0;

            foreach (var cropCycle in cropCycles)
            {
                try
                {
                    var oldStage = cropCycle.GrowthStage;
                    var updated = cropCycle.UpdateGrowthStage();

                    if (updated)
                    {
                        updateCount++;
                        if (oldStage != cropCycle.GrowthStage)
                        {
                            stageChangeCount++;
                            logger.LogInformation(
                                "CropCycle {Id} ({CropType}) stage changed from {OldStage} to {NewStage}",
                                cropCycle.Id,
                                cropCycle.CropType,
                                oldStage,
                                cropCycle.GrowthStage);

                            // TODO: Send notification when stage changes
                            // await SendStageChangeNotification(cropCycle, oldStage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error updating crop cycle {CropCycleId}", cropCycle.Id);
                }
            }

            if (updateCount > 0)
            {
                await context.SaveChangesAsync(stoppingToken);
                logger.LogInformation(
                    "Updated {UpdateCount} crop cycles, {StageChangeCount} stage changes detected",
                    updateCount,
                    stageChangeCount);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in UpdateAllGrowthStages");
            throw;
        }
    }
}