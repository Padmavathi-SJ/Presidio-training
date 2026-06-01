namespace AgriculturePlatform.Domain.Enums;

// Irrigation types
public enum IrrigationTypeEnum
{
    DRIP,          // Drip irrigation
    SPRINKLER,     // Overhead sprinklers
    FLOOD,         // Flood/furrow irrigation
    CENTER_PIVOT,  // Circular irrigation system
    SUBSURFACE     // Underground drip lines
}

// Fertilizer types
public enum FertilizerTypeEnum
{
    UREA,
    DAP,
    NPK_20_20_20,
    POTASH,
    COMPOST,
    MANURE,
    LIQUID_FERTILIZER,
    FOLIAR_SPRAY
}

// Pesticide types
public enum PesticideTypeEnum
{
    INSECTICIDE,
    FUNGICIDE,
    HERBICIDE,
    RODENTICIDE,
    BACTERICIDE,
    NEMATICIDE
}