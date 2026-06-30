// AgriculturePlatform.Application/Events/AlertCreatedEvent.cs
namespace AgriculturePlatform.Application.Events;

public class AlertCreatedEvent
{
    public int FarmId { get; set; }
    public int AlertId { get; set; }
    public string AlertTitle { get; set; } = string.Empty;
    public string FieldName { get; set; } = string.Empty;
    public int FieldId { get; set; }
    public string Severity { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}