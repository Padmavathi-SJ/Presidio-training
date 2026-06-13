// AgriculturePlatform.Application/Interfaces/IAlertNotificationService.cs
namespace AgriculturePlatform.Application.Interfaces;
using AgriculturePlatform.Domain.Entities.CropMonitoring;

public interface IAlertNotificationService
{
    Task NotifyNewAlertAsync(int farmId, object alertData);
    Task NotifyAlertResolvedAsync(int farmId, object resolutionData);
    Task NotifySensorReadingAsync(int farmId, object readingData);
    Task SendAlertNotificationsAsync(Alert alert, int farmId);
    Task SendTestAlertEmailAsync(string recipientEmail, string recipientName);
}