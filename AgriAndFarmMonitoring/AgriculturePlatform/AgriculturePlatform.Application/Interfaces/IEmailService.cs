// Application/Interfaces/IEmailService.cs
using AgriculturePlatform.Application.DTOs.Email;

namespace AgriculturePlatform.Application.Interfaces;

public interface IEmailService
{
    Task<bool> SendEmailAsync(EmailDto email);
    Task<bool> SendSensorAlertEmailAsync(SensorAlertEmailDto alert, string recipientEmail, string recipientName);
    Task<bool> SendBulkSensorAlertEmailsAsync(SensorAlertEmailDto alert, List<(string Email, string Name)> recipients);
}