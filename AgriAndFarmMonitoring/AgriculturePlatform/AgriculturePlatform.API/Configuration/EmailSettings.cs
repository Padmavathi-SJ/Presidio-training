// API/Configuration/EmailSettings.cs
namespace AgriculturePlatform.API.Configuration;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SenderEmail { get; set; } = string.Empty;
    public string SenderPassword { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string SenderName { get; set; } = "Farm Management System";
}