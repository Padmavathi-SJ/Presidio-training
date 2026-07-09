// Application/Interfaces/INotificationService.cs
namespace AgriculturePlatform.Application.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(int farmId, int? adminId, int? workerId, string title, string message, string type, string? actionUrl = null);
    Task CreateAlertAggregateNotificationAsync(int farmId, int? adminId, string title, string type, string listUrl, string? singleItemUrl = null);
}