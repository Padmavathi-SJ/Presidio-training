using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AgriculturePlatform.Domain.Entities.AI;

public class ChatSession
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    public int FarmId { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string SessionId { get; set; } = string.Empty;
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public bool IsActive { get; set; }

    [Required]
    public int UserId { get; set; }
    
    // Navigation property
    [ForeignKey("FarmId")]
    public virtual AgriculturePlatform.Domain.Entities.AdminEntities.Farm Farm { get; set; } = null!;
    
    public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}
