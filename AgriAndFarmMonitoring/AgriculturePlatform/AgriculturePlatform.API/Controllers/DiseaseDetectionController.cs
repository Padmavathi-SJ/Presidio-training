using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AgriculturePlatform.Application.DTOs.AI;
using AgriculturePlatform.Application.Interfaces;

namespace AgriculturePlatform.API.Controllers
{
    [ApiController]
    [Route("api/disease")]
    [Authorize]
    public class DiseaseDetectionController : ControllerBase
    {
        private readonly IDiseaseDetectionService _diseaseDetectionService;

        public DiseaseDetectionController(IDiseaseDetectionService diseaseDetectionService)
        {
            _diseaseDetectionService = diseaseDetectionService;
        }

        [HttpPost("detect")]
        [RequestSizeLimit(10 * 1024 * 1024)] // 10 MB limit
        public async Task<IActionResult> DetectDisease([FromForm] IFormFile image, [FromForm] int farmId, [FromForm] int fieldId, [FromForm] int? cropCycleId, [FromForm] string? cropType, [FromForm] string? growthStage, [FromForm] string? additionalSymptoms)
        {
            if (image == null || image.Length == 0)
            {
                return BadRequest("No image provided.");
            }
            
            // Get user id from claims
            var userIdClaim = User.FindFirst("workerId")?.Value ?? User.FindFirst("adminId")?.Value ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            int userId = 0;
            if (!string.IsNullOrEmpty(userIdClaim)) int.TryParse(userIdClaim, out userId);
            
            using var memoryStream = new MemoryStream();
            await image.CopyToAsync(memoryStream);
            var imageBytes = memoryStream.ToArray();

            var request = new DiseaseDetectionRequestDto
            {
                FarmId = farmId,
                FieldId = fieldId,
                CropCycleId = cropCycleId,
                UserId = userId,
                CropType = cropType,
                GrowthStage = growthStage,
                AdditionalSymptoms = additionalSymptoms,
                ImageData = imageBytes
            };

            var result = await _diseaseDetectionService.AnalyzeImageAsync(request);
            return Ok(result);
        }

        [HttpGet("history")]
        public async Task<IActionResult> GetHistory([FromQuery] int farmId, [FromQuery] int fieldId)
        {
            var history = await _diseaseDetectionService.GetDiseaseHistoryAsync(farmId, fieldId);
            return Ok(history);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _diseaseDetectionService.GetAnalysisByIdAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("chat-with-context")]
        public async Task<IActionResult> ChatWithContext([FromBody] ChatContextRequest request)
        {
            var answer = await _diseaseDetectionService.GetFollowUpAnswerAsync(request.AnalysisId, request.Question);
            return Ok(new { Answer = answer });
        }
    }

    public class ChatContextRequest
    {
        public int AnalysisId { get; set; }
        public string Question { get; set; } = string.Empty;
    }
}
