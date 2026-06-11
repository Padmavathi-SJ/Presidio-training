// Application/Interfaces/IFileStorageService.cs
namespace AgriculturePlatform.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(byte[] fileContent, string fileName, string subDirectory);
    Task<byte[]> GetFileAsync(string filePath);
    Task<bool> DeleteFileAsync(string filePath);
    string GetDownloadUrl(string fileName);
}