// AgriculturePlatform.Infrastructure/Repositories/FarmRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class FarmRepository : IFarmRepository
{
    private readonly AppDbContext _context;

    public FarmRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Farm?> GetByIdAsync(int id)
    {
        return await _context.Farms
            .FirstOrDefaultAsync(f => f.Id == id);
    }

    public async Task<Farm?> GetByEmailAsync(string email)
    {
        return await _context.Farms
            .FirstOrDefaultAsync(f => f.Email == email);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Farms.AnyAsync(f => f.Email == email);
    }

    public async Task<Farm> CreateAsync(Farm farm)
    {
        _context.Farms.Add(farm);
        await _context.SaveChangesAsync();
        return farm;
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Farms.AnyAsync(f => f.Id == id);
    }

    public async Task<List<Farm>> GetAllActiveAsync()
{
    return await _context.Farms
        .Where(f => f.IsActive && !f.IsDeleted)
        .ToListAsync();
}
}