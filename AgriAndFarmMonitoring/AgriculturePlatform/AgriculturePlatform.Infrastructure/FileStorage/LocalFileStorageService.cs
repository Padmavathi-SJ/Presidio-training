using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using AgriculturePlatform.Application.Interfaces;
using System.IO;
using System.Threading.Tasks;

namespace AgriculturePlatform.Infrastructure.FileStorage;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _uploadsFolder;
    private readonly string _baseUrl;
    private readonly IWebHostEnvironment _environment;

    public LocalFileStorageService(IConfiguration configuration, IWebHostEnvironment environment)
    {
        _uploadsFolder = configuration["FileStorage:Local:UploadsFolder"] ?? "wwwroot/uploads";
        _baseUrl = configuration["FileStorage:BaseUrl"] ?? "http://localhost:5000";
        _environment = environment;
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, string subDirectory)
    {
        var fullDirectoryPath = Path.Combine(_environment.ContentRootPath, _uploadsFolder, subDirectory);
        if (!Directory.Exists(fullDirectoryPath))
        {
            Directory.CreateDirectory(fullDirectoryPath);
        }

        var relativePath = Path.Combine(subDirectory, fileName).Replace("\\", "/");
        var fullFilePath = Path.Combine(fullDirectoryPath, fileName);

        await File.WriteAllBytesAsync(fullFilePath, fileContent);

        return relativePath;
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, _uploadsFolder, filePath);
        if (!File.Exists(fullPath))
            throw new FileNotFoundException($"File not found: {filePath}");

        return await File.ReadAllBytesAsync(fullPath);
    }

    public Task<bool> DeleteFileAsync(string filePath)
    {
        var fullPath = Path.Combine(_environment.ContentRootPath, _uploadsFolder, filePath);
        if (!File.Exists(fullPath))
            return Task.FromResult(false);

        File.Delete(fullPath);
        return Task.FromResult(true);
    }

    public string GetDownloadUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;

        var relativeUrlPath = filePath.Replace("\\", "/");
        return $"{_baseUrl}/uploads/{relativeUrlPath}";
    }
}
