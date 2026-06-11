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

    [HttpGet("reports/{fileName}")]
    public async Task<IActionResult> DownloadReportFile(string fileName)
    {
        try
        {
            var fileBytes = await _fileStorageService.GetFileAsync($"Reports/YieldReports/{fileName}");
            
            var extension = Path.GetExtension(fileName).ToLower();
            var contentType = extension switch
            {
                ".csv" => "text/csv",
                ".json" => "application/json",
                ".pdf" => "application/pdf",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };
            
            return File(fileBytes, contentType, fileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { message = "File not found" });
        }
    }
}