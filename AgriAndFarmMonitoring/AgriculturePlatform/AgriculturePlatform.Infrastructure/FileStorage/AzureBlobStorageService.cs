using Microsoft.Extensions.Configuration;
using AgriculturePlatform.Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System;
using System.IO;
using System.Threading.Tasks;

namespace AgriculturePlatform.Infrastructure.FileStorage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["FileStorage:AzureBlob:ConnectionString"] ?? "UseDevelopmentStorage=true";
        _blobServiceClient = new BlobServiceClient(connectionString);
    }

    private (BlobContainerClient Container, string BlobName) ParseFilePath(string filePath, string defaultContainer = "uploads")
    {
        if (string.IsNullOrEmpty(filePath))
            return (_blobServiceClient.GetBlobContainerClient(defaultContainer.ToLowerInvariant()), string.Empty);

        var parts = filePath.Replace("\\", "/").Split('/', 2);
        if (parts.Length > 1)
        {
            return (_blobServiceClient.GetBlobContainerClient(parts[0].ToLowerInvariant()), parts[1]);
        }
        
        return (_blobServiceClient.GetBlobContainerClient(defaultContainer.ToLowerInvariant()), filePath);
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, string subDirectory)
    {
        var containerName = string.IsNullOrEmpty(subDirectory) ? "uploads" : subDirectory.Split('/')[0].ToLowerInvariant();
        var container = _blobServiceClient.GetBlobContainerClient(containerName);
        
        // Create container as PRIVATE (not public)
        await container.CreateIfNotExistsAsync();
        
        var blobName = string.IsNullOrEmpty(subDirectory) || !subDirectory.Contains('/') 
            ? fileName 
            : $"{subDirectory.Substring(subDirectory.IndexOf('/') + 1)}/{fileName}";
            
        var blobClient = container.GetBlobClient(blobName);
        
        using (var ms = new MemoryStream(fileContent))
        {
            await blobClient.UploadAsync(ms, true);
        }
        
        // Return path in the format "container/blobName" so it works identically to local folders
        return $"{containerName}/{blobName}";
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var (container, blobName) = ParseFilePath(filePath);
        var blobClient = container.GetBlobClient(blobName);
        var downloadInfo = await blobClient.DownloadContentAsync();
        return downloadInfo.Value.Content.ToArray();
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var (container, blobName) = ParseFilePath(filePath);
        var blobClient = container.GetBlobClient(blobName);
        return await blobClient.DeleteIfExistsAsync();
    }

    public string GetDownloadUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;
            
        var (container, blobName) = ParseFilePath(filePath);
        var blobClient = container.GetBlobClient(blobName);
        
        // Generate a SAS token for secure access (valid for 1 hour)
        var sasBuilder = new BlobSasBuilder
        {
            BlobContainerName = container.Name,
            BlobName = blobName,
            Resource = "b",  // b = blob
            ExpiresOn = DateTimeOffset.UtcNow.AddHours(1)
        };
        sasBuilder.SetPermissions(BlobSasPermissions.Read);
        
        var sasUri = blobClient.GenerateSasUri(sasBuilder);
        return sasUri.ToString();
    }

    // ✅ Add FileExistsAsync
    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var (container, blobName) = ParseFilePath(filePath);
        var blobClient = container.GetBlobClient(blobName);
        return await blobClient.ExistsAsync();
    }

    // ✅ Add GetPublicUrl (same as GetDownloadUrl for Azure)
    public string GetPublicUrl(string filePath)
    {
        return GetDownloadUrl(filePath);
    }
}