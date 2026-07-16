using System;
using System.Collections.Generic;

namespace AgriculturePlatform.Application.DTOs.AI
{
    public class DiseaseDetectionResponseDto
    {
        public int AnalysisId { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int ConfidenceScore { get; set; }
        
        public List<string> Symptoms { get; set; } = new();
        public List<string> Treatment { get; set; } = new();
        public List<string> Prevention { get; set; } = new();
        public List<string> OrganicRemedies { get; set; } = new();
        public string AdditionalInfo { get; set; } = string.Empty;
        
        public List<string> FollowUpQuestions { get; set; } = new();
        public List<string> Recommendations { get; set; } = new();
        public string ChatContext { get; set; } = string.Empty;
    }

    public class DiseaseAnalysisResultDto
    {
        public int Id { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int ConfidenceScore { get; set; }
        public List<string> Symptoms { get; set; } = new();
        public List<string> Treatment { get; set; } = new();
        public List<string> Prevention { get; set; } = new();
        public List<string> OrganicRemedies { get; set; } = new();
        public string AdditionalInfo { get; set; } = string.Empty;
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DiseaseHistoryDto
    {
        public int Id { get; set; }
        public string DiseaseName { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public int ConfidenceScore { get; set; }
        public bool IsResolved { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
