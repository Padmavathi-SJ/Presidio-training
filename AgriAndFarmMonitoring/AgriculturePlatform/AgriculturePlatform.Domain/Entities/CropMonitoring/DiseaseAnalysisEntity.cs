using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AgriculturePlatform.Domain.Common;
using AgriculturePlatform.Domain.Entities.AdminEntities;

namespace AgriculturePlatform.Domain.Entities.CropMonitoring
{
    public class DiseaseAnalysisEntity : BaseEntity
    {
        public int FarmId { get; set; }
        public int FieldId { get; set; }
        public int? CropCycleId { get; set; }
        public int CreatedBy { get; set; }
        
        [MaxLength(100)]
        public string ImageHash { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string DiseaseName { get; set; } = string.Empty;
        
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;
        
        [MaxLength(20)]
        public string Severity { get; set; } = string.Empty;
        
        public int ConfidenceScore { get; set; }
        
        [Column(TypeName = "jsonb")]
        public string Symptoms { get; set; } = "[]";
        
        [Column(TypeName = "jsonb")]
        public string Treatment { get; set; } = "[]";
        
        [Column(TypeName = "jsonb")]
        public string Prevention { get; set; } = "[]";
        
        [Column(TypeName = "jsonb")]
        public string OrganicRemedies { get; set; } = "[]";
        
        [MaxLength(2000)]
        public string AdditionalInfo { get; set; } = string.Empty;
        
        public bool IsResolved { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        public virtual Farm? Farm { get; set; }
        public virtual Field? Field { get; set; }
    }
}
