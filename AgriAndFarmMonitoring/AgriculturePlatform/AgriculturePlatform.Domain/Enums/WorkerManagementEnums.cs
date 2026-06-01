namespace AgriculturePlatform.Domain.Enums;

// Task types
public enum TaskTypeEnum
{
    IRRIGATION,
    FERTILIZING,
    PEST_CONTROL,
    WEEDING,
    PRUNING,
    HARVESTING,
    MONITORING,
    MAINTENANCE,
    SOIL_PREPARATION,
    PLANTING,
    QUALITY_CHECK
}

// Task priority
public enum TaskPriorityEnum
{
    LOW,       // Can be done anytime
    MEDIUM,    // Normal priority
    HIGH,      // Should be done soon
    URGENT     // Must be done immediately
}

// Task status
public enum TaskStatusEnum
{
    PENDING,      // Not started
    IN_PROGRESS,  // Work in progress
    COMPLETED,    // Successfully finished
    CANCELLED,    // Task cancelled
    OVERDUE,      // Past due date
    REASSIGNED    // Moved to another worker
}