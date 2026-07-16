using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces
{
    public interface IDiseaseRepository
    {
        Task<DiseaseAnalysisEntity> CreateAsync(DiseaseAnalysisEntity entity);
        Task<DiseaseAnalysisEntity?> GetByIdAsync(int id);
        Task<List<DiseaseAnalysisEntity>> GetByFarmIdAsync(int farmId);
        Task UpdateAsync(DiseaseAnalysisEntity entity);
        Task DeleteAsync(int id);
    }
}
