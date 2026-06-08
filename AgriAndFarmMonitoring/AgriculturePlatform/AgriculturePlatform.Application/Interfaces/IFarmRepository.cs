// AgriculturePlatform.Application/Interfaces/IFarmRepository.cs
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Application.Interfaces;

public interface IFarmRepository
{
    Task<Farm?> GetByIdAsync(int id);
    Task<Farm?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
    Task<Farm> CreateAsync(Farm farm);
    Task<bool> ExistsAsync(int id);
    Task<List<Farm>> GetAllActiveAsync();
}