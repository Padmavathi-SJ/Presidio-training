using System;
using System.Threading.Tasks;
using AgriculturePlatform.Application.DTOs.AI;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.AI;

namespace AgriculturePlatform.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepository;
        private readonly IAiService _aiService;
        private readonly IWorkerRepository _workerRepository;
        private readonly IFieldRepository _fieldRepository;
        private readonly ICropCycleRepository _cropCycleRepository;
        private readonly IHarvestRepository _harvestRepository;
        private readonly IQualityCheckRepository _qualityCheckRepository;
        private readonly IObservationRepository _observationRepository;
        private readonly ITaskRepository _taskRepository;

        public ChatService(
            IChatRepository chatRepository, 
            IAiService aiService,
            IWorkerRepository workerRepository,
            IFieldRepository fieldRepository,
            ICropCycleRepository cropCycleRepository,
            IHarvestRepository harvestRepository,
            IQualityCheckRepository qualityCheckRepository,
            IObservationRepository observationRepository,
            ITaskRepository taskRepository)
        {
            _chatRepository = chatRepository;
            _aiService = aiService;
            _workerRepository = workerRepository;
            _fieldRepository = fieldRepository;
            _cropCycleRepository = cropCycleRepository;
            _harvestRepository = harvestRepository;
            _qualityCheckRepository = qualityCheckRepository;
            _observationRepository = observationRepository;
            _taskRepository = taskRepository;
        }

        public async Task<ChatResponseDto> ProcessChatAsync(ChatRequestDto request)
        {
            string sessionId = string.IsNullOrEmpty(request.SessionId) 
                ? Guid.NewGuid().ToString() 
                : request.SessionId;

            var session = await _chatRepository.GetSessionAsync(sessionId);

            if (session == null)
            {
                session = new ChatSession
                {
                    SessionId = sessionId,
                    FarmId = request.FarmId,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _chatRepository.CreateSessionAsync(session);
            }

            var history = await _chatRepository.GetSessionMessagesAsync(sessionId);

            // Context Injection (Simple Keyword-based RAG)
            string systemPrompt = "You are an intelligent agricultural assistant for AgriMonitor. Answer questions about farming, crops, weather, and farm management.\n";
            string lowerMessage = request.Message.ToLower();

            if (lowerMessage.Contains("worker") || lowerMessage.Contains("staff"))
            {
                var workers = await _workerRepository.GetAllAsync(request.FarmId);
                systemPrompt += "\n[INJECTED CONTEXT: WORKERS]\nHere are the workers currently on the farm:\n";
                foreach (var worker in workers)
                {
                    string status = worker.IsActive ? "Active" : "Inactive";
                    systemPrompt += $"- {worker.Name} (Role: {worker.Role}) - {status} - Email: {worker.Email}\n";
                }
            }

            if (lowerMessage.Contains("field") || lowerMessage.Contains("crop") || lowerMessage.Contains("soil"))
            {
                var fields = await _fieldRepository.GetAllAsync(request.FarmId);
                var cropCycles = await _cropCycleRepository.GetAllAsync(request.FarmId);

                systemPrompt += "\n[INJECTED CONTEXT: FIELDS & CROP CYCLES]\nHere are the fields and their active crop cycles on the farm:\n";
                foreach (var field in fields)
                {
                    systemPrompt += $"- Field: {field.FieldName} (ID: {field.Id}, Status: {field.Status}, Area: {field.AreaHectares} ha, Soil: {field.SoilType})\n";
                }
                systemPrompt += "Crop Cycles:\n";
                foreach (var crop in cropCycles)
                {
                    systemPrompt += $"- Crop: {crop.CropType} in Field ID {crop.FieldId} (Stage: {crop.GrowthStage}, Status: {crop.Status}, Planting Date: {crop.PlantingDate})\n";
                }
            }

            if (lowerMessage.Contains("harvest") || lowerMessage.Contains("yield"))
            {
                var harvests = await _harvestRepository.GetByDateRangeAsync(request.FarmId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
                systemPrompt += "\n[INJECTED CONTEXT: HARVESTS]\nHere are the harvests from the last 30 days:\n";
                foreach (var h in harvests)
                {
                    systemPrompt += $"- Harvest ID: {h.Id}, CropCycle ID: {h.CropCycleId}, Quantity: {h.QuantityKg}kg, Quality: {h.QualityGrade}, Date: {h.HarvestDate}, Worker: {h.Harvester?.Name ?? "Unknown"}\n";
                }
            }

            if (lowerMessage.Contains("quality") || lowerMessage.Contains("check") || lowerMessage.Contains("grade"))
            {
                var qualityChecks = await _qualityCheckRepository.GetPendingApprovalsAsync(request.FarmId);
                systemPrompt += "\n[INJECTED CONTEXT: QUALITY CHECKS]\nHere are the pending quality checks needing approval:\n";
                foreach (var q in qualityChecks)
                {
                    systemPrompt += $"- QC ID: {q.Id} for Harvest ID: {q.HarvestId}, Grade: {q.FinalGrade}, Defect: {q.DefectPct}%, Status: {q.ApprovalStatus}, Checker: {q.Checker?.Name ?? "Unknown"}\n";
                }
            }

            if (lowerMessage.Contains("observation") || lowerMessage.Contains("pest") || lowerMessage.Contains("disease") || lowerMessage.Contains("health"))
            {
                var observations = await _observationRepository.GetByDateRangeAsync(request.FarmId, DateTime.UtcNow.AddDays(-30), DateTime.UtcNow);
                systemPrompt += "\n[INJECTED CONTEXT: OBSERVATIONS]\nHere are the crop health observations from the last 30 days:\n";
                foreach (var o in observations)
                {
                    systemPrompt += $"- Obs ID: {o.Id} for Field ID: {o.FieldId}, Health: {o.CropHealth}, Pest: {o.PestType}, Status: {o.ValidationStatus}, Worker: {o.Worker?.Name ?? "Unknown"}\n";
                }
            }

            if (lowerMessage.Contains("task") || lowerMessage.Contains("work") || lowerMessage.Contains("todo"))
            {
                var pendingTasks = await _taskRepository.GetTasksByStatusAsync(request.FarmId, "PENDING");
                var inProgressTasks = await _taskRepository.GetTasksByStatusAsync(request.FarmId, "IN_PROGRESS");
                systemPrompt += "\n[INJECTED CONTEXT: TASKS]\nHere are the pending and in-progress tasks:\n";
                foreach (var t in pendingTasks)
                {
                    systemPrompt += $"- Task ID: {t.Id}, Name: {t.TaskName}, Priority: {t.Priority}, Status: {t.Status}, Worker: {t.Worker?.Name ?? "Unknown"} (ID: {t.WorkerId})\n";
                }
                foreach (var t in inProgressTasks)
                {
                    systemPrompt += $"- Task ID: {t.Id}, Name: {t.TaskName}, Priority: {t.Priority}, Status: {t.Status}, Worker: {t.Worker?.Name ?? "Unknown"} (ID: {t.WorkerId})\n";
                }
            }

            var aiResponse = await _aiService.GetChatCompletionAsync(request.Message, history, systemPrompt);

            var message = new ChatMessage
            {
                SessionId = sessionId,
                Query = request.Message,
                Response = aiResponse,
                Timestamp = DateTime.UtcNow
            };

            await _chatRepository.SaveMessageAsync(message);

            return new ChatResponseDto
            {
                SessionId = sessionId,
                Message = aiResponse,
                Timestamp = message.Timestamp
            };
        }
    }
}
