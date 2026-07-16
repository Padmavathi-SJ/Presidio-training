using System.Collections.Generic;
using System.Threading.Tasks;
using AgriculturePlatform.Application.DTOs.AI;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Interfaces
{
    public interface IDiseaseDetectionService
    {
        Task<DiseaseAnalysisResultDto> AnalyzeImageAsync(DiseaseDetectionRequestDto request);
        Task<List<DiseaseHistoryDto>> GetDiseaseHistoryAsync(int farmId, int fieldId);
        Task<DiseaseAnalysisResultDto?> GetAnalysisByIdAsync(int id);
        Task<string> GetFollowUpAnswerAsync(int analysisId, string question);
    }
}
