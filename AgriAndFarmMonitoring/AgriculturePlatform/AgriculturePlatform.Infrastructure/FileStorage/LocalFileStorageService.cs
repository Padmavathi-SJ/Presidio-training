// Infrastructure/FileStorage/LocalFileStorageService.cs
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AgriculturePlatform.Application.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace AgriculturePlatform.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseUrl;
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000";
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, string subDirectory)
    {
        // ✅ Ensure WebRootPath exists
        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var uploadsPath = Path.Combine(webRootPath, "uploads");
        var fullDirectoryPath = Path.Combine(uploadsPath, subDirectory);
        
        if (!Directory.Exists(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        // ✅ Store relative path WITHOUT "uploads/" prefix
        var relativePath = Path.Combine(subDirectory, fileName).Replace("\\", "/");
        var fullFilePath = Path.Combine(fullDirectoryPath, fileName);

        await File.WriteAllBytesAsync(fullFilePath, fileContent);

        return relativePath;
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var cleanPath = filePath.Replace("uploads/", "");
        var fullPath = Path.Combine(webRootPath, "uploads", cleanPath);
        
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return Task.FromResult(false);

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var cleanPath = filePath.Replace("uploads/", "");
        var fullPath = Path.Combine(webRootPath, "uploads", cleanPath);
        
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public Task<bool> FileExistsAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return Task.FromResult(false);

        var webRootPath = _environment.WebRootPath;
        if (string.IsNullOrEmpty(webRootPath))
        {
            webRootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        }

        var cleanPath = filePath.Replace("uploads/", "");
        var fullPath = Path.Combine(webRootPath, "uploads", cleanPath);
        
        return Task.FromResult(File.Exists(fullPath));
    }

    public string GetDownloadUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        var relativeUrlPath = filePath.Replace("\\", "/");
        
        // ✅ Always prepend /uploads/ for serving static files
        return $"{_baseUrl}/uploads/{relativeUrlPath}";
    }

// Infrastructure/FileStorage/LocalFileStorageService.cs

public string GetPublicUrl(string filePath)
{
    if (string.IsNullOrEmpty(filePath))
        return string.Empty;

    // ✅ If it's already a full URL, return it as-is
    if (filePath.StartsWith("http://") || filePath.StartsWith("https://"))
        return filePath;

    var relativeUrlPath = filePath.Replace("\\", "/");
    
    // ✅ Always prepend /uploads/ for serving static files
    return $"{_baseUrl}/uploads/{relativeUrlPath}";
}

}