using Microsoft.Extensions.Configuration;
using AgriculturePlatform.Application.Interfaces;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.IO;
using System.Threading.Tasks;

namespace AgriculturePlatform.Infrastructure.FileStorage;

public class AzureBlobStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _containerClient;

    public AzureBlobStorageService(IConfiguration configuration)
    {
        var connectionString = configuration["FileStorage:AzureBlob:ConnectionString"] ?? "UseDevelopmentStorage=true";
        var containerName = configuration["FileStorage:AzureBlob:ContainerName"] ?? "observations";
        _blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
    }

    public async Task<string> SaveFileAsync(byte[] fileContent, string fileName, string subDirectory)
    {
        await _containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);
        
        var blobPath = string.IsNullOrEmpty(subDirectory) ? fileName : $"{subDirectory}/{fileName}";
        var blobClient = _containerClient.GetBlobClient(blobPath);
        
        using (var ms = new MemoryStream(fileContent))
        {
            await blobClient.UploadAsync(ms, true);
        }
        
        return blobPath;
    }

    public async Task<byte[]> GetFileAsync(string filePath)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);
        var downloadInfo = await blobClient.DownloadContentAsync();
        return downloadInfo.Value.Content.ToArray();
    }

    public async Task<bool> DeleteFileAsync(string filePath)
    {
        var blobClient = _containerClient.GetBlobClient(filePath);
        return await blobClient.DeleteIfExistsAsync();
    }

    public string GetDownloadUrl(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return string.Empty;
            
        var blobClient = _containerClient.GetBlobClient(filePath);
        return blobClient.Uri.AbsoluteUri;
    }

    // ✅ Add FileExistsAsync
    public async Task<bool> FileExistsAsync(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return false;

        var blobClient = _containerClient.GetBlobClient(filePath);
        return await blobClient.ExistsAsync();
    }

    // ✅ Add GetPublicUrl (same as GetDownloadUrl for Azure)
    public string GetPublicUrl(string filePath)
    {
        return GetDownloadUrl(filePath);
    }

}