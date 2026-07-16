using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriculturePlatform.Domain.Entities.AI;

public class ChatMessage
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(1000)]
    public string Query { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(4000)]
    public string Response { get; set; } = string.Empty;
    
    [Required]
    public DateTime Timestamp { get; set; }
    
    // Navigation property
    [ForeignKey("SessionId")]
    public virtual ChatSession ChatSession { get; set; } = null!;
}
