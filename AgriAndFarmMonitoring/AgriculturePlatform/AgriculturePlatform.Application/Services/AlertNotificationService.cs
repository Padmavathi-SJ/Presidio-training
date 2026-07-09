// Application/Services/AlertNotificationService.cs
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.DTOs.Email;
using AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;
using AgriculturePlatform.Domain.Entities.WorkerManagement;
using AgriculturePlatform.Domain.Entities.AdminEntities;
using System.Text.Json;

namespace AgriculturePlatform.Application.Services;

public class AlertNotificationService : IAlertNotificationService
{
    private readonly IEmailService _emailService;
    private readonly IWorkerRepository _workerRepository;
    private readonly IAdminRepository _adminRepository;
    private readonly IFieldRepository _fieldRepository;
    private readonly ICropCycleRepository _cropCycleRepository;
    private readonly IWorkerFieldAssignmentRepository _assignmentRepository;
    private readonly INotificationService _inAppNotificationService;
    private readonly ILogger<AlertNotificationService> _logger;
    private readonly Dictionary<string, DateTime> _lastNotificationSent = new();

    public AlertNotificationService(
        IEmailService emailService,
        IWorkerRepository workerRepository,
        IAdminRepository adminRepository,
        IFieldRepository fieldRepository,
        ICropCycleRepository cropCycleRepository,
        IWorkerFieldAssignmentRepository assignmentRepository,
        INotificationService inAppNotificationService,
        ILogger<AlertNotificationService> logger)
    {
        _emailService = emailService;
        _workerRepository = workerRepository;
        _adminRepository = adminRepository;
        _fieldRepository = fieldRepository;
        _cropCycleRepository = cropCycleRepository;
        _assignmentRepository = assignmentRepository;
        _inAppNotificationService = inAppNotificationService;
        _logger = logger;
    }

    // ✅ IMPLEMENT MISSING METHODS
    public async Task NotifyNewAlertAsync(int farmId, object alertData)
    {
        _logger.LogInformation($"New alert created for farm {farmId}: {JsonSerializer.Serialize(alertData)}");
        
        // You can add additional notification logic here (SignalR, WebSocket, etc.)
        await Task.CompletedTask;
    }

    public async Task NotifyAlertResolvedAsync(int farmId, object resolutionData)
    {
        _logger.LogInformation($"Alert resolved for farm {farmId}: {JsonSerializer.Serialize(resolutionData)}");
        
        // You can add additional notification logic here (SignalR, WebSocket, etc.)
        await Task.CompletedTask;
    }

    public async Task NotifySensorReadingAsync(int farmId, object readingData)
    {
        _logger.LogInformation($"New sensor reading for farm {farmId}: {JsonSerializer.Serialize(readingData)}");
        
        // You can add additional notification logic here (SignalR, WebSocket, etc.)
        await Task.CompletedTask;
    }

    // ✅ EXISTING METHODS
    public async Task SendAlertNotificationsAsync(Alert alert, int farmId)
    {
        var notificationKey = $"{farmId}_{alert.FieldId}_{alert.AlertType}";
        
        // Check if we've sent a notification recently (cooldown period)
        if (_lastNotificationSent.TryGetValue(notificationKey, out var lastSent) &&
            (DateTime.UtcNow - lastSent).TotalMinutes < 30)
        {
            _logger.LogInformation($"Skipping notification for {notificationKey} - cooldown active");
            return;
        }
        
        var field = await _fieldRepository.GetByIdAsync(alert.FieldId, farmId);
        var cropCycle = alert.CropCycleId.HasValue 
            ? await _cropCycleRepository.GetByIdAsync(alert.CropCycleId.Value, farmId) 
            : null;
        
        // Get assigned workers for this field
        var assignedWorkers = await GetAssignedWorkersForFieldAsync(alert.FieldId, farmId);
        
        // Get admins for this farm
        var admins = await _adminRepository.GetByFarmIdAsync(farmId);
        
        var emailAlert = new SensorAlertEmailDto
        {
            FarmName = field?.Farm?.FarmName ?? "Farm",
            FieldName = field?.FieldName ?? "Unknown Field",
            CropType = cropCycle?.CropType?.ToString() ?? "Unknown",
            SensorType = alert.AlertType?.ToString() ?? "Unknown",
            CurrentValue = alert.SensorValue ?? 0,
            ThresholdValue = alert.ThresholdValue ?? 0,
            Severity = alert.Severity?.ToString() ?? "MEDIUM",
            Message = alert.Message ?? "Sensor reading exceeded threshold",
            AlertTime = alert.CreatedAt,
            RecommendedAction = GetRecommendedAction(alert),
            DashboardLink = "http://localhost:5000/dashboard"
        };
        
        var recipients = new List<(string Email, string Name)>();
        
        // Add admins
        foreach (var admin in admins)
        {
            recipients.Add((admin.Email, admin.Name));
        }
        
        // Add assigned workers
        foreach (var worker in assignedWorkers)
        {
            recipients.Add((worker.Email, worker.Name));
        }
        
        if (recipients.Any())
        {
            await _emailService.SendBulkSensorAlertEmailsAsync(emailAlert, recipients);
            _lastNotificationSent[notificationKey] = DateTime.UtcNow;
            _logger.LogInformation($"Sent {recipients.Count} alert notifications for field {alert.FieldId}");
        }

        // Send In-App Notifications
        foreach (var admin in admins)
        {
            await _inAppNotificationService.CreateAlertAggregateNotificationAsync(
                farmId,
                admin.Id,
                "Sensor Alerts",
                "SensorAlert",
                "/admin/sensors/alerts",
                $"/admin/sensors/alerts?fieldId={alert.FieldId}"
            );
        }

        foreach (var worker in assignedWorkers)
        {
            await _inAppNotificationService.CreateNotificationAsync(
                farmId,
                null,
                worker.Id,
                "Sensor Alert",
                $"[{alert.Severity}] {alert.Message} (Field: {field?.FieldName})",
                "SensorAlert",
                $"/worker/sensors/alerts?fieldId={alert.FieldId}"
            );
        }
    }

    public async Task SendTestAlertEmailAsync(string recipientEmail, string recipientName)
    {
        var testAlert = new SensorAlertEmailDto
        {
            FarmName = "Test Farm",
            FieldName = "Test Field",
            CropType = "WHEAT",
            SensorType = "SOIL_MOISTURE",
            CurrentValue = 85,
            ThresholdValue = 70,
            Severity = "CRITICAL",
            Message = "This is a test alert message",
            AlertTime = DateTime.UtcNow,
            RecommendedAction = "Please check the system configuration",
            DashboardLink = "http://localhost:5000/dashboard"
        };
        
        await _emailService.SendSensorAlertEmailAsync(testAlert, recipientEmail, recipientName);
    }

    private async Task<List<Worker>> GetAssignedWorkersForFieldAsync(int fieldId, int farmId)
    {
        var assignments = await _assignmentRepository.GetWorkerFieldAssignmentsByFieldAsync(fieldId, farmId);
        var workers = new List<Worker>();
        
        foreach (var assignment in assignments)
        {
            var worker = await _workerRepository.GetByIdAsync(assignment.WorkerId, farmId);
            if (worker != null)
                workers.Add(worker);
        }
        
        return workers;
    }

    private string GetRecommendedAction(Alert alert)
    {
        return alert.AlertType?.ToString() switch
        {
            "DROUGHT_STRESS" => "Increase irrigation frequency. Check soil moisture levels daily.",
            "WATERLOGGED" => "Reduce irrigation. Ensure proper drainage systems are working.",
            "HEAT_STRESS" => "Increase shade coverage if possible. Ensure adequate irrigation during peak heat.",
            "COLD_STRESS" => "Consider frost protection measures. Delay planting if expecting freeze.",
            "NUTRIENT_DEFICIENCY" => "Apply recommended fertilizer. Conduct soil test for specific deficiencies.",
            "PEST_INFESTATION" => "Apply appropriate pesticide. Monitor pest population daily.",
            "DISEASE_OUTBREAK" => "Apply fungicide. Remove infected plants immediately.",
            "SOIL_PH_ALERT" => "Apply lime to raise pH or sulfur to lower pH. Retest soil after treatment.",
            _ => "Monitor the situation closely. Consult with agricultural expert if needed."
        };
    }
}