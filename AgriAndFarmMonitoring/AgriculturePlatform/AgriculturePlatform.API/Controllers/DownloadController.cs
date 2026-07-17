// API/Controllers/DownloadController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.API.Controllers;

[ApiController]
[Route("api/downloads")]
[AllowAnonymous] // Or [Authorize] if you want to restrict
public class DownloadController : ControllerBase
{
    private readonly IFileStorageService _fileStorageService;

    public DownloadController(IFileStorageService fileStorageService)
    {
        _fileStorageService = fileStorageService;
    }

    [HttpGet("{*filePath}")]
    public async Task<IActionResult> DownloadFile(string filePath)
    {
        try
        {
            var fileBytes = await _fileStorageService.GetFileAsync(filePath);
            
            var extension = Path.GetExtension(filePath).ToLower();
            var contentType = extension switch
            {
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                _ => "application/octet-stream"
            };
            
            return File(fileBytes, contentType, Path.GetFileName(filePath));
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "File not found" });
        }
    }
}