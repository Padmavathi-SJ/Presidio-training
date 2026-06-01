namespace AgriculturePlatform.Domain.Enums;

// Crop types
public enum CropTypeEnum
{
    WHEAT,
    MAIZE,
    RICE,
    BARLEY,
    SOYBEAN,
    COTTON,
    HAZELNUT,
    POTATO,
    TOMATO,
    ONION,
    GRAPE,
    APPLE
}

// Growth stages
public enum GrowthStageEnum
{
    GERMINATION,    // Seed sprouting (0-10 days)
    SEEDLING,       // Early growth (10-20 days)
    VEGETATIVE,     // Leaf/stem growth (20-60 days)
    FLOWERING,      // Flower/bloom stage (60-80 days)
    FRUITING,       // Fruit development (80-100 days)
    MATURITY,       // Ready for harvest (100-120 days)
    HARVESTED       // Harvest completed
}

// Field status
public enum FieldStatusEnum
{
    ACTIVE,         // Currently in use
    FALLOW,         // Resting/not planted
    PREPARING,      // Being prepared for planting
    MAINTENANCE,    // Under maintenance
    RETIRED         // No longer in use
}

// Soil types
public enum SoilTypeEnum
{
    CLAY,           // Heavy, water-retentive
    SANDY,          // Light, well-draining
    SILTY,          // Fertile, moisture-retentive
    LOAMY,          // Ideal, balanced
    PEATY,          // High organic matter
    CHALKY          // Alkaline, stony
}

// Sensor types
public enum SensorTypeEnum
{
    SOIL_MOISTURE,   // Soil water content (%)
    SOIL_TEMP,       // Soil temperature (°C)
    AIR_TEMP,        // Air temperature (°C)
    AIR_HUMIDITY,    // Relative humidity (%)
    LIGHT_INTENSITY, // Light/lux level (lux)
    SOIL_PH,         // Soil acidity (pH)
    NPK_NITROGEN,    // Nitrogen level (ppm)
    NPK_PHOSPHORUS,  // Phosphorus level (ppm)
    NPK_POTASSIUM,   // Potassium level (ppm)
    WIND_SPEED,      // Wind velocity (m/s)
    RAINFALL,        // Precipitation (mm)
    LEAF_WETNESS     // Leaf moisture (%)
}

// Alert types
public enum AlertTypeEnum
{
    DROUGHT_STRESS,        // Low soil moisture
    WATERLOGGED,           // Excess water
    HEAT_STRESS,           // High temperature
    COLD_STRESS,           // Low temperature/frost
    NUTRIENT_DEFICIENCY,   // Low NPK levels
    PEST_INFESTATION,      // Pest detected
    DISEASE_OUTBREAK,      // Crop disease
    WEED_PRESSURE,         // High weed growth
    SOIL_PH_ALERT,         // pH out of range
    HARVEST_READY,         // Crop ready for harvest
    IRRIGATION_NEEDED,     // Water required
    FERTILIZER_NEEDED      // Fertilizer required
}

// Alert severity levels
public enum AlertSeverityEnum
{
    LOW,       // Informational, monitor only
    MEDIUM,    // Action recommended
    HIGH,      // Action required soon
    CRITICAL   // Immediate action required
}

// Crop health status
public enum CropHealthEnum
{
    EXCELLENT,   // Perfect condition
    GOOD,        // Slightly below optimal
    AVERAGE,     // Acceptable condition
    POOR,        // Significant issues
    CRITICAL     // Severe problems
}

// Weather conditions
public enum WeatherConditionEnum
{
    CLEAR,
    CLOUDY,
    RAINY,
    STORMY,
    SNOWY,
    FOGGY,
    WINDY
}