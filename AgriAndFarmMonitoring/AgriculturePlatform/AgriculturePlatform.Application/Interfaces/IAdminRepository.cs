// AgriculturePlatform.Application/Interfaces/IAdminRepository.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Interfaces;

public interface IAdminRepository
{
    Task<Admin?> GetByEmailAsync(string email);
    Task<Admin?> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email);
    Task<Admin> CreateAsync(Admin admin);
    Task UpdateAsync(Admin admin);

    Task<List<Admin>> GetByFarmIdAsync(int farmId);
}