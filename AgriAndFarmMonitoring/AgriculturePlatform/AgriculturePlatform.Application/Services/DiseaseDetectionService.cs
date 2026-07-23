using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AgriculturePlatform.Application.DTOs.AI;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

namespace AgriculturePlatform.Application.Services
{
    public class DiseaseDetectionService : IDiseaseDetectionService
    {
        private readonly IAiService _aiService;
        private readonly IDiseaseRepository _diseaseRepository;
        private readonly IChatRepository _chatRepository;

        public DiseaseDetectionService(IAiService aiService, IDiseaseRepository diseaseRepository, IChatRepository chatRepository)
        {
            _aiService = aiService;
            _diseaseRepository = diseaseRepository;
            _chatRepository = chatRepository;
        }

        public async Task<DiseaseAnalysisResultDto> AnalyzeImageAsync(DiseaseDetectionRequestDto request)
        {
            // Convert byte array to Base64
            var base64Image = Convert.ToBase64String(request.ImageData);

            // Construct context for the AI
            var context = $"Crop Type: {request.CropType ?? "Unknown"}. Growth Stage: {request.GrowthStage ?? "Unknown"}. User Notes: {request.AdditionalSymptoms ?? "None"}.";

            // Call AI Vision API
            var jsonResult = await _aiService.AnalyzePlantImageAsync(base64Image, context);

            // Parse result
            var resultDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonResult);

            if (resultDict == null)
            {
                throw new Exception("Failed to parse AI response.");
            }

            var diseaseName = resultDict.ContainsKey("diseaseName") ? resultDict["diseaseName"].GetString() ?? "Unknown" : "Unknown";
            var category = resultDict.ContainsKey("category") ? resultDict["category"].GetString() ?? "Unknown" : "Unknown";
            var severity = resultDict.ContainsKey("severity") ? resultDict["severity"].GetString() ?? "Unknown" : "Unknown";
            var confidenceScore = resultDict.ContainsKey("confidenceScore") && resultDict["confidenceScore"].ValueKind == JsonValueKind.Number ? resultDict["confidenceScore"].GetInt32() : 0;
            var symptoms = resultDict.ContainsKey("symptoms") ? resultDict["symptoms"].EnumerateArray().Select(e => e.GetString() ?? "").ToList() : new List<string>();
            var treatment = resultDict.ContainsKey("treatment") ? resultDict["treatment"].EnumerateArray().Select(e => e.GetString() ?? "").ToList() : new List<string>();
            var prevention = resultDict.ContainsKey("prevention") ? resultDict["prevention"].EnumerateArray().Select(e => e.GetString() ?? "").ToList() : new List<string>();
            var organicRemedies = resultDict.ContainsKey("organicRemedies") ? resultDict["organicRemedies"].EnumerateArray().Select(e => e.GetString() ?? "").ToList() : new List<string>();
            var additionalInfo = resultDict.ContainsKey("additionalInfo") ? resultDict["additionalInfo"].GetString() ?? "" : "";

            // Hash the image briefly to avoid storing it in DB, just for tracking uniqueness if needed
            var hashBytes = System.Security.Cryptography.SHA256.HashData(request.ImageData);
            var imageHash = Convert.ToHexString(hashBytes);

            var sessionId = $"disease-{Guid.NewGuid().ToString().Substring(0, 8)}";

            // Create ChatSession for disease tracking
            var chatSession = new AgriculturePlatform.Domain.Entities.AI.ChatSession
            {
                SessionId = sessionId,
                FarmId = request.FarmId,
                UserId = request.UserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _chatRepository.CreateSessionAsync(chatSession);

            // Create Entity
            var entity = new DiseaseAnalysisEntity
            {
                FarmId = request.FarmId,
                FieldId = request.FieldId,
                CropCycleId = request.CropCycleId,
                CreatedBy = request.UserId,
                SessionId = sessionId,
                ImageHash = imageHash,
                DiseaseName = diseaseName,
                Category = category,
                Severity = severity,
                ConfidenceScore = confidenceScore,
                Symptoms = JsonSerializer.Serialize(symptoms),
                Treatment = JsonSerializer.Serialize(treatment),
                Prevention = JsonSerializer.Serialize(prevention),
                OrganicRemedies = JsonSerializer.Serialize(organicRemedies),
                AdditionalInfo = additionalInfo,
                IsResolved = false
            };

            await _diseaseRepository.CreateAsync(entity);

            return new DiseaseAnalysisResultDto
            {
                Id = entity.Id,
                DiseaseName = entity.DiseaseName,
                Category = entity.Category,
                Severity = entity.Severity,
                ConfidenceScore = entity.ConfidenceScore,
                Symptoms = symptoms,
                Treatment = treatment,
                Prevention = prevention,
                OrganicRemedies = organicRemedies,
                AdditionalInfo = entity.AdditionalInfo,
                IsResolved = entity.IsResolved,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<List<DiseaseHistoryDto>> GetDiseaseHistoryAsync(int farmId, int fieldId)
        {
            var analyses = await _diseaseRepository.GetByFarmIdAsync(farmId);
            
            return analyses
                .Where(a => a.FieldId == fieldId)
                .Select(a => new DiseaseHistoryDto
                {
                    Id = a.Id,
                    DiseaseName = a.DiseaseName,
                    Category = a.Category,
                    Severity = a.Severity,
                    ConfidenceScore = a.ConfidenceScore,
                    IsResolved = a.IsResolved,
                    CreatedAt = a.CreatedAt
                })
                .ToList();
        }

        public async Task<List<DiseaseHistoryDto>> GetMyDiseaseHistoryAsync(int userId)
        {
            var analyses = await _diseaseRepository.GetByUserIdAsync(userId);
            
            return analyses
                .Select(a => new DiseaseHistoryDto
                {
                    Id = a.Id,
                    DiseaseName = a.DiseaseName,
                    Category = a.Category,
                    Severity = a.Severity,
                    ConfidenceScore = a.ConfidenceScore,
                    IsResolved = a.IsResolved,
                    CreatedAt = a.CreatedAt
                })
                .ToList();
        }

        public async Task<DiseaseAnalysisResultDto?> GetAnalysisByIdAsync(int id)
        {
            var entity = await _diseaseRepository.GetByIdAsync(id);
            if (entity == null) return null;

            return new DiseaseAnalysisResultDto
            {
                Id = entity.Id,
                DiseaseName = entity.DiseaseName,
                Category = entity.Category,
                Severity = entity.Severity,
                ConfidenceScore = entity.ConfidenceScore,
                Symptoms = JsonSerializer.Deserialize<List<string>>(entity.Symptoms) ?? new List<string>(),
                Treatment = JsonSerializer.Deserialize<List<string>>(entity.Treatment) ?? new List<string>(),
                Prevention = JsonSerializer.Deserialize<List<string>>(entity.Prevention) ?? new List<string>(),
                OrganicRemedies = JsonSerializer.Deserialize<List<string>>(entity.OrganicRemedies) ?? new List<string>(),
                AdditionalInfo = entity.AdditionalInfo,
                IsResolved = entity.IsResolved,
                CreatedAt = entity.CreatedAt
            };
        }

        public async Task<string> GetFollowUpAnswerAsync(int analysisId, string question, int userId)
        {
            var analysis = await _diseaseRepository.GetByIdAsync(analysisId);
            if (analysis == null)
            {
                throw new Exception("Analysis not found.");
            }

            var systemPrompt = $"You are assisting a farmer. The farmer recently ran a disease analysis with the following results: \n" +
                             $"Disease: {analysis.DiseaseName} (Category: {analysis.Category}, Severity: {analysis.Severity})\n" +
                             $"Symptoms: {analysis.Symptoms}\n" +
                             $"Treatment: {analysis.Treatment}\n" +
                             $"Please answer the following user question based on this context. Answer concisely.";

            IEnumerable<AgriculturePlatform.Domain.Entities.AI.ChatMessage> history = Array.Empty<AgriculturePlatform.Domain.Entities.AI.ChatMessage>();
            if (!string.IsNullOrEmpty(analysis.SessionId))
            {
                history = await _chatRepository.GetSessionMessagesAsync(analysis.SessionId);
            }
                             
            var aiResponse = await _aiService.GetChatCompletionAsync(question, history, systemPrompt);

            if (!string.IsNullOrEmpty(analysis.SessionId))
            {
                var message = new AgriculturePlatform.Domain.Entities.AI.ChatMessage
                {
                    SessionId = analysis.SessionId,
                    Query = question,
                    Response = aiResponse,
                    Timestamp = DateTime.UtcNow
                };
                await _chatRepository.SaveMessageAsync(message);
            }

            return aiResponse;
        }

        public async Task<IEnumerable<ChatMessageDto>> GetChatHistoryAsync(int analysisId)
        {
            var analysis = await _diseaseRepository.GetByIdAsync(analysisId);
            if (analysis == null || string.IsNullOrEmpty(analysis.SessionId))
            {
                return new List<ChatMessageDto>();
            }

            var messages = await _chatRepository.GetSessionMessagesAsync(analysis.SessionId);
            return messages.Select(m => new ChatMessageDto
            {
                Id = m.Id,
                SessionId = m.SessionId,
                Query = m.Query,
                Response = m.Response,
                Timestamp = m.Timestamp
            }).ToList();
        }
    }
}
