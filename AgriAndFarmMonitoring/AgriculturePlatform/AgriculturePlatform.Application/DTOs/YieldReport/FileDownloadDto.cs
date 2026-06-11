// Application/DTOs/YieldReport/FileDownloadDto.cs
namespace AgriculturePlatform.Application.DTOs.YieldReport;

public class FileDownloadDto
{
    public int ReportId { get; set; }
    public string ReportName { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string DownloadUrl { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public DateTime ExportedAt { get; set; }
    public string FormattedFileSize => FormatFileSize(FileSize);
    
    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}