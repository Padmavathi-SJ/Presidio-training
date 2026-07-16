using System;
using System.ComponentModel.DataAnnotations;

namespace AgriculturePlatform.Application.DTOs.AI
{
    public class DiseaseDetectionRequestDto
    {
        public int FarmId { get; set; }
        public int FieldId { get; set; }
        public int? CropCycleId { get; set; }
        public int UserId { get; set; }
        
        public string? CropType { get; set; }
        public string? GrowthStage { get; set; }
        public string? AdditionalSymptoms { get; set; }
        
        public byte[] ImageData { get; set; } = Array.Empty<byte>();
    }
}
