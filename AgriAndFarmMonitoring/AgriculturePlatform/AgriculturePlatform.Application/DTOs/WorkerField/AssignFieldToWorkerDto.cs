// AgriculturePlatform.Application/DTOs/WorkerField/AssignFieldToWorkerDto.cs
namespace AgriculturePlatform.Application.DTOs.WorkerField;

public class AssignFieldToWorkerDto
{
    /// <summary>
    /// ID of the worker to assign the field to
    /// </summary>
    public int WorkerId { get; set; }

    /// <summary>
    /// ID of the field to assign to the worker
    /// </summary>
    public int FieldId { get; set; }

    /// <summary>
    /// Date when the assignment starts (defaults to current date if not provided)
    /// </summary>
    public DateTime? AssignedDate { get; set; }

    /// <summary>
    /// Date when the assignment ends (optional - null means ongoing)
    /// </summary>
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Additional notes about the assignment
    /// </summary>
    public string? Notes { get; set; }
}