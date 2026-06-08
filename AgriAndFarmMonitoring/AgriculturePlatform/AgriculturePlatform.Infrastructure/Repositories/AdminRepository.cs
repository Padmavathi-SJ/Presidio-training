// AgriculturePlatform.Infrastructure/Repositories/AdminRepository.cs
using Microsoft.EntityFrameworkCore;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using AgriculturePlatform.Infrastructure.Context;

namespace AgriculturePlatform.Infrastructure.Repositories;

public class AdminRepository : IAdminRepository
{
    private readonly AppDbContext _context;

    public AdminRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Admin?> GetByEmailAsync(string email)
    {
        return await _context.Admins
            .Include(a => a.Farm)
            .FirstOrDefaultAsync(a => a.Email == email);
    }

    public async Task<Admin?> GetByIdAsync(int id)
    {
        return await _context.Admins
            .Include(a => a.Farm)
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Admins.AnyAsync(a => a.Email == email);
    }

    public async Task<Admin> CreateAsync(Admin admin)
    {
        _context.Admins.Add(admin);
        await _context.SaveChangesAsync();
        return admin;
    }

    public async Task UpdateAsync(Admin admin)
    {
        admin.UpdatedAt = DateTime.UtcNow;
        _context.Admins.Update(admin);
        await _context.SaveChangesAsync();
    }

    // Add this method implementation
    public async Task<List<Admin>> GetByFarmIdAsync(int farmId)
    {
        return await _context.Admins
            .Where(a => a.FarmId == farmId && a.IsActive && !a.IsDeleted)
            .ToListAsync();
    }
}