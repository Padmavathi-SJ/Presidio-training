// AgriculturePlatform.Application/Interfaces/IAlertNotificationService.cs
namespace AgriculturePlatform.Application.Interfaces;

public interface IAlertNotificationService
{
    Task NotifyNewAlertAsync(int farmId, object alertData);
    Task NotifyAlertResolvedAsync(int farmId, object resolutionData);
    Task NotifySensorReadingAsync(int farmId, object readingData);
}