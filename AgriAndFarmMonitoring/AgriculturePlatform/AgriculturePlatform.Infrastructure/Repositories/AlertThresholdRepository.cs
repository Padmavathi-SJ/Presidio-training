// AgriculturePlatform.Infrastructure/Repositories/AlertThresholdRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class AlertThresholdRepository : IAlertThresholdRepository
{
    private readonly AppDbContext _context;

    public AlertThresholdRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AlertThreshold?> GetByIdAsync(int id, int farmId)
    {
        return await _context.Set<AlertThreshold>()
            .FirstOrDefaultAsync(t => t.Id == id && t.FarmId == farmId && !t.IsDeleted);
    }

    public async Task<IEnumerable<AlertThreshold>> GetAllAsync(int farmId)
    {
        return await _context.Set<AlertThreshold>()
            .Where(t => t.FarmId == farmId && t.IsActive && !t.IsDeleted)
            .ToListAsync();
    }

    public async Task<AlertThreshold?> GetByCropAndStageAsync(string cropType, string growthStage, string sensorType, int farmId)
    {
        return await _context.Set<AlertThreshold>()
            .FirstOrDefaultAsync(t => t.FarmId == farmId && 
                                       t.CropType == cropType && 
                                       t.GrowthStage == growthStage &&
                                       t.SensorType == sensorType &&
                                       t.IsActive && 
                                       !t.IsDeleted);
    }

    public async Task<AlertThreshold> CreateAsync(AlertThreshold threshold)
    {
        threshold.CreatedAt = DateTime.UtcNow;
        await _context.Set<AlertThreshold>().AddAsync(threshold);
        await _context.SaveChangesAsync();
        return threshold;
    }

    public async Task UpdateAsync(AlertThreshold threshold)
    {
        threshold.UpdatedAt = DateTime.UtcNow;
        _context.Set<AlertThreshold>().Update(threshold);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(AlertThreshold threshold)
    {
        threshold.IsDeleted = true;
        threshold.DeletedAt = DateTime.UtcNow;
        _context.Set<AlertThreshold>().Update(threshold);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(int id, int farmId)
    {
        return await _context.Set<AlertThreshold>()
            .AnyAsync(t => t.Id == id && t.FarmId == farmId && !t.IsDeleted);
    }
}