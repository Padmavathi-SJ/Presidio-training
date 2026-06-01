namespace AgriculturePlatform.Domain.Enums;

// Quality grades
public enum QualityGradeEnum
{
    A_PLUS,   // Premium quality (best price)
    A,        // Excellent quality
    B,        // Good quality
    C,        // Average quality
    D,        // Poor quality
    REJECTED  // Not suitable for sale
}

// Report types
public enum ReportTypeEnum
{
    DAILY,
    WEEKLY,
    MONTHLY,
    QUARTERLY,
    SEASONAL,
    YEARLY
}

// Harvest method
public enum HarvestMethodEnum
{
    MANUAL,          // Hand-picked
    MECHANICAL,      // Machine harvested
    SEMI_MECHANICAL,
    COMBINE          // Combined harvester
}