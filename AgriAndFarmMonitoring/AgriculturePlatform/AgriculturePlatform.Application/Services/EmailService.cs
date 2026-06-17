// Infrastructure/Services/EmailService.cs
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using AgriculturePlatform.Application.DTOs.Email;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly SmtpClient _smtpClient;
    private readonly string _senderEmail;
    private readonly string _senderName;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
        
        var smtpServer = configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
        var smtpPort = int.Parse(configuration["EmailSettings:SmtpPort"] ?? "587");
        _senderEmail = configuration["EmailSettings:SenderEmail"] ?? string.Empty;
        var senderPassword = configuration["EmailSettings:SenderPassword"] ?? string.Empty;
        _senderName = configuration["EmailSettings:SenderName"] ?? "Farm Management System";
        var enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"] ?? "true");
        
        _smtpClient = new SmtpClient(smtpServer, smtpPort)
        {
            EnableSsl = enableSsl,
            Credentials = string.IsNullOrEmpty(senderPassword) 
                ? CredentialCache.DefaultNetworkCredentials 
                : new NetworkCredential(_senderEmail, senderPassword)
        };
    }

    public async Task<bool> SendEmailAsync(EmailDto email)
    {
        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(_senderEmail, _senderName);
            message.To.Add(new MailAddress(email.To, email.ToName));
            message.Subject = email.Subject;
            message.Body = email.Body;
            message.IsBodyHtml = email.IsHtml;
            
            if (email.Attachments != null)
            {
                foreach (var attachment in email.Attachments)
                {
                    if (File.Exists(attachment))
                    {
                        message.Attachments.Add(new Attachment(attachment));
                    }
                }
            }
            
            await _smtpClient.SendMailAsync(message);
            _logger.LogInformation($"Email sent to {email.To}: {email.Subject}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {email.To}");
            return false;
        }
    }

    public async Task<bool> SendSensorAlertEmailAsync(SensorAlertEmailDto alert, string recipientEmail, string recipientName)
    {
        var severityColor = alert.Severity.ToLower() switch
        {
            "critical" => "#dc3545",
            "high" => "#fd7e14",
            "medium" => "#ffc107",
            _ => "#17a2b8"
        };
        
        var severityLabel = alert.Severity.ToUpper() switch
        {
            "CRITICAL" => "⚠ CRITICAL",
            "HIGH" => "⚠ HIGH",
            "MEDIUM" => "⚠ MEDIUM",
            _ => "ℹ INFO"
        };
        
        var severityText = alert.Severity.ToUpper() switch
        {
            "CRITICAL" => "CRITICAL ALERT - Immediate Action Required",
            "HIGH" => "HIGH ALERT - Action Required Soon",
            "MEDIUM" => "MEDIUM ALERT - Monitor Closely",
            _ => "INFORMATIONAL - No Immediate Action Required"
        };
        
        var emailBody = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background: linear-gradient(135deg, #2d6a4f, #1b4332); color: white; padding: 20px; text-align: center; border-radius: 10px 10px 0 0; }}
        .content {{ background: #f8f9fa; padding: 20px; border-radius: 0 0 10px 10px; }}
        .alert-card {{ border-left: 4px solid {severityColor}; background: white; padding: 15px; margin: 15px 0; border-radius: 8px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
        .severity-badge {{ display: inline-block; padding: 5px 15px; border-radius: 20px; font-size: 13px; font-weight: bold; background: {severityColor}; color: white; letter-spacing: 0.5px; }}
        .value {{ font-size: 24px; font-weight: bold; color: {severityColor}; }}
        .metric {{ margin: 10px 0; }}
        .footer {{ text-align: center; padding: 20px; font-size: 12px; color: #666; }}
        .button {{ display: inline-block; padding: 12px 25px; background: #2d6a4f; color: white; text-decoration: none; border-radius: 5px; font-weight: bold; }}
        .alert-title {{ font-size: 18px; font-weight: bold; margin-bottom: 10px; }}
        .divider {{ border-top: 1px solid #e9ecef; margin: 15px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>[ALERT] {severityLabel}</h2>
            <p style='font-size: 14px; opacity: 0.9;'>{alert.FarmName} - {alert.FieldName}</p>
        </div>
        <div class='content'>
            <div class='alert-card'>
                <div class='alert-title' style='color: {severityColor};'>⚠ {severityText}</div>
                <div class='divider'></div>
                <p style='font-size: 16px;'><strong>Message:</strong> {alert.Message}</p>
                
                <div class='divider'></div>
                
                <div class='metric'><strong>Sensor Type:</strong> {FormatSensorType(alert.SensorType)}</div>
                <div class='metric'><strong>Current Reading:</strong> <span class='value'>{alert.CurrentValue}</span> {GetUnit(alert.SensorType)}</div>
                <div class='metric'><strong>Threshold Value:</strong> {alert.ThresholdValue} {GetUnit(alert.SensorType)}</div>
                <div class='metric'><strong>Crop Type:</strong> {alert.CropType}</div>
                <div class='metric'><strong>Alert Time:</strong> {alert.AlertTime:yyyy-MM-dd HH:mm:ss} UTC</div>
                <div class='metric'><strong>Severity Level:</strong> <span class='severity-badge'>{alert.Severity}</span></div>
            </div>
            
            <div style='margin: 20px 0; padding: 15px; background: #e9ecef; border-radius: 8px;'>
                <h3 style='margin-top: 0;'>Recommended Action</h3>
                <p style='margin-bottom: 0;'>{alert.RecommendedAction}</p>
            </div>
            
            <div style='text-align: center; margin-top: 20px;'>
                <a href='{alert.DashboardLink}' class='button'>View Dashboard</a>
            </div>
        </div>
        <div class='footer'>
            <p>This is an automated alert from your Farm Management System.</p>
            <p style='font-weight: bold; color: #dc3545;'>Please take appropriate action immediately.</p>
        </div>
    </div>
</body>
</html>";

        return await SendEmailAsync(new EmailDto
        {
            To = recipientEmail,
            ToName = recipientName,
            Subject = $"[{alert.Severity}] {FormatSensorType(alert.SensorType)} Alert - {alert.FieldName}",
            Body = emailBody,
            IsHtml = true
        });
    }

    public async Task<bool> SendBulkSensorAlertEmailsAsync(SensorAlertEmailDto alert, List<(string Email, string Name)> recipients)
    {
        var success = true;
        foreach (var recipient in recipients)
        {
            var result = await SendSensorAlertEmailAsync(alert, recipient.Email, recipient.Name);
            if (!result) success = false;
        }
        return success;
    }

    private string FormatSensorType(string sensorType)
    {
        return sensorType switch
        {
            "SOIL_MOISTURE" => "Soil Moisture",
            "SOIL_TEMP" => "Soil Temperature",
            "AIR_TEMP" => "Air Temperature",
            "AIR_HUMIDITY" => "Air Humidity",
            "SOIL_PH" => "Soil pH",
            "NPK_NITROGEN" => "Nitrogen Level",
            "NPK_PHOSPHORUS" => "Phosphorus Level",
            "NPK_POTASSIUM" => "Potassium Level",
            "WIND_SPEED" => "Wind Speed",
            "RAINFALL" => "Rainfall",
            "LIGHT_INTENSITY" => "Light Intensity",
            "LEAF_WETNESS" => "Leaf Wetness",
            _ => sensorType
        };
    }

    private string GetUnit(string sensorType)
    {
        return sensorType switch
        {
            "SOIL_MOISTURE" => "%",
            "SOIL_TEMP" => "°C",
            "AIR_TEMP" => "°C",
            "AIR_HUMIDITY" => "%",
            "SOIL_PH" => "pH",
            "NPK_NITROGEN" => "ppm",
            "NPK_PHOSPHORUS" => "ppm",
            "NPK_POTASSIUM" => "ppm",
            "WIND_SPEED" => "m/s",
            "RAINFALL" => "mm",
            "LIGHT_INTENSITY" => "lux",
            "LEAF_WETNESS" => "%",
            _ => ""
        };
    }
}