// API/Configuration/FileStorageSettings.cs
namespace AgriculturePlatform.API.Configuration;

public class FileStorageSettings
{
    public string ReportsPath { get; set; } = "Reports/YieldReports";
    public string BaseUrl { get; set; } = string.Empty;
}