// src/app/features/admin/models/sensor.model.ts

export interface SensorReading {
  id: number;
  fieldId: number;
  fieldName: string;
  cropCycleId: number;
  cropType: string | null;
  sensorType: string;
  value: number | null;
  unit: string | null;
  recordedAt: string;
  isThresholdViolation: boolean;
  alertType: string | null;
}

export interface SensorReadingFilter {
  fieldId?: number | null;
  cropCycleId?: number | null;
  sensorType?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
  latestOnly?: boolean | null;
  groupBy?: string | null;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface SensorStatistics {
  period: string;
  dailyStats: Record<string, DailySensorStats>;
  weeklyStats: Record<string, WeeklySensorStats>;
  monthlyStats: Record<string, MonthlySensorStats>;
}

export interface DailySensorStats {
  date: string;
  avgSoilMoisture: number | null;
  avgSoilTemp: number | null;
  avgAirTemp: number | null;
  avgHumidity: number | null;
  readingsCount: number;
  alertCount: number;
}

export interface WeeklySensorStats {
  weekNumber: number;
  year: number;
  avgSoilMoisture: number | null;
  avgSoilTemp: number | null;
  alertCount: number;
}

export interface MonthlySensorStats {
  month: string;
  year: number;
  avgSoilMoisture: number | null;
  avgSoilTemp: number | null;
  alertCount: number;
}

export interface Alert {
  id: number;
  fieldId: number;
  fieldName: string;
  cropCycleId: number | null;
  cropType: string | null;
  alertType: string | null;
  severity: string | null;
  message: string | null;
  isResolved: boolean;
  sensorValue: number | null;
  thresholdValue: number | null;
  createdAt: string;
  resolvedAt: string | null;
}

export interface AlertFilter {
  fieldId?: number | null;
  cropCycleId?: number | null;
  alertType?: string | null;
  severity?: string | null;
  isResolved?: boolean | null;
  fromDate?: string | null;
  toDate?: string | null;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface AlertDashboard {
  totalAlerts: number;
  criticalAlerts: number;
  highAlerts: number;
  mediumAlerts: number;
  lowAlerts: number;
  resolvedAlerts: number;
  unresolvedAlerts: number;
  alertsByField: Record<string, number>;
  alertsByType: Record<string, number>;
  recentAlerts: RecentAlert[];
}

export interface RecentAlert {
  id: number;
  fieldName: string;
  alertType: string;
  severity: string;
  message: string;
  createdAt: string;
  isResolved: boolean;
}

export interface ResolveAlert {
  alertId: number;
  resolutionNotes?: string;
}

// Alert Thresholds
export interface AlertThreshold {
  id: number;
  cropType: string;
  growthStage: string;
  sensorType: string;
  minValue: number;
  maxValue: number;
  severity: string;
  isActive: boolean;
  notificationEmails?: string;
}

export interface CreateAlertThreshold {
  cropType: string;
  growthStage: string;
  sensorType: string;
  minValue: number;
  maxValue: number;
  severity: string;
  notificationEmails?: string;
}

export interface UpdateAlertThreshold {
  minValue?: number;
  maxValue?: number;
  severity?: string;
  isActive?: boolean;
  notificationEmails?: string;
}

// Manual Reading
export interface CreateManualSensorReading {
  fieldId: number;
  cropCycleId: number;
  sensorType: string;
  value: number;
  unit: string;
  recordedAt?: string;
}

// Sensor Type Constants
export const SENSOR_TYPES = [
  'SOIL_MOISTURE',
  'SOIL_TEMP',
  'AIR_TEMP',
  'AIR_HUMIDITY',
  'LIGHT_INTENSITY',
  'SOIL_PH',
  'NPK_NITROGEN',
  'NPK_PHOSPHORUS',
  'NPK_POTASSIUM',
  'WIND_SPEED',
  'RAINFALL',
  'LEAF_WETNESS'
];

export const SENSOR_TYPE_LABELS: Record<string, string> = {
  'SOIL_MOISTURE': 'Soil Moisture',
  'SOIL_TEMP': 'Soil Temperature',
  'AIR_TEMP': 'Air Temperature',
  'AIR_HUMIDITY': 'Air Humidity',
  'LIGHT_INTENSITY': 'Light Intensity',
  'SOIL_PH': 'Soil pH',
  'NPK_NITROGEN': 'Nitrogen (N)',
  'NPK_PHOSPHORUS': 'Phosphorus (P)',
  'NPK_POTASSIUM': 'Potassium (K)',
  'WIND_SPEED': 'Wind Speed',
  'RAINFALL': 'Rainfall',
  'LEAF_WETNESS': 'Leaf Wetness'
};

export const SENSOR_TYPE_ICONS: Record<string, string> = {
  'SOIL_MOISTURE': 'water_drop',
  'SOIL_TEMP': 'thermostat',
  'AIR_TEMP': 'thermostat',
  'AIR_HUMIDITY': 'humidity',
  'LIGHT_INTENSITY': 'light_mode',
  'SOIL_PH': 'science',
  'NPK_NITROGEN': 'grass',
  'NPK_PHOSPHORUS': 'grass',
  'NPK_POTASSIUM': 'grass',
  'WIND_SPEED': 'air',
  'RAINFALL': 'umbrella',
  'LEAF_WETNESS': 'water_drop'
};

export const SENSOR_TYPE_UNITS: Record<string, string> = {
  'SOIL_MOISTURE': '%',
  'SOIL_TEMP': '°C',
  'AIR_TEMP': '°C',
  'AIR_HUMIDITY': '%',
  'LIGHT_INTENSITY': 'lux',
  'SOIL_PH': 'pH',
  'NPK_NITROGEN': 'ppm',
  'NPK_PHOSPHORUS': 'ppm',
  'NPK_POTASSIUM': 'ppm',
  'WIND_SPEED': 'm/s',
  'RAINFALL': 'mm',
  'LEAF_WETNESS': '%'
};

// Alert Severity Colors
export const ALERT_SEVERITY_COLORS: Record<string, string> = {
  'LOW': 'bg-green-100 text-green-700 border-green-400',
  'MEDIUM': 'bg-yellow-100 text-yellow-700 border-yellow-400',
  'HIGH': 'bg-orange-100 text-orange-700 border-orange-400',
  'CRITICAL': 'bg-red-100 text-red-700 border-red-400'
};

export const ALERT_SEVERITY_ICONS: Record<string, string> = {
  'LOW': 'info',
  'MEDIUM': 'warning',
  'HIGH': 'warning',
  'CRITICAL': 'error'
};

export const ALERT_SEVERITY_ORDER = {
  'LOW': 1,
  'MEDIUM': 2,
  'HIGH': 3,
  'CRITICAL': 4
};