// Infrastructure/Services/FileStorageService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.Infrastructure.FileStorage;

public class FileStorageService : IFileStorageService
{
    private readonly string _reportsPath;
    private readonly string _baseUrl;
    private readonly IWebHostEnvironment _environment;

    public FileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _reportsPath = configuration["FileStorage:ReportsPath"] ?? "Reports/YieldReports";
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000";
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, string subDirectory)
    {
        var fullDirectoryPath = Path.Combine(_environment.ContentRootPath, _reportsPath, subDirectory);
        if (!Directory.Exists(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        var relativePath = Path.Combine(_reportsPath, subDirectory, fileName).Replace("\\", "/");
        var fullPath = Path.Combine(_environment.ContentRootPath, relativePath);
        
        await File.WriteAllBytesAsync(fullPath, fileContent);
        
        return relativePath;
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");
        
        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, filePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);
        
        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public string GetDownloadUrl(string fileName)
    {
        return $"{_baseUrl}/api/downloads/reports/{fileName}";
    }
}