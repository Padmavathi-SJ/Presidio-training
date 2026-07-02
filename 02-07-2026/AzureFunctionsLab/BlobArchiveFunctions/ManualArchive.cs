using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BlobArchiveFunctions;

public class ManualArchive
{
    private readonly BlobArchiveService _archiveService;
    private readonly ILogger<ManualArchive> _logger;

    public ManualArchive(BlobArchiveService archiveService, ILogger<ManualArchive> logger)
    {
        _archiveService = archiveService;
        _logger = logger;
    }

    [Function("ManualArchive")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        int days = 7;
        if (req.Query.TryGetValue("days", out var daysParam))
        {
            int.TryParse(daysParam, out days);
        }

        _logger.LogInformation("ManualArchive triggered - archiving files older than {Days} day(s).", days);

        int count = await _archiveService.ArchiveOldBlobsAsync(olderThanDays: days);

        return new OkObjectResult($"Archive complete. Files moved: {count}");
    }
}