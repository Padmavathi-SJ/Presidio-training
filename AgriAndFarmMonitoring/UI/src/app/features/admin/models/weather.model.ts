// src/app/features/admin/models/weather.model.ts
export interface WeatherData {
  id: number;
  fieldId: number;
  fieldName: string;
  temperature: number | null;
  humidity: number | null;
  rainfallMm: number | null;
  windSpeed: number | null;
  condition: string | null;
  recordedAt: string;
  createdAt: string;
}

export interface WeatherAlert {
  id: number;
  fieldId: number;
  fieldName: string;
  alertType: 'STORM' | 'FROST' | 'HEAT_WAVE' | 'HEAVY_RAIN' | 'HIGH_WIND' | 'DROUGHT' | 'HAIL' | 'TORNADO' | 'FLOOD' | 'WILDFIRE';
  severity: 'ADVISORY' | 'WATCH' | 'WARNING' | 'EMERGENCY';
  title: string;
  message: string;
  temperature: number | null;
  windSpeed: number | null;
  rainfallMm: number | null;
  isAcknowledged: boolean;
  alertTime: string;
  expiresAt: string | null;
  createdAt: string;
}

export interface WeatherForecast {
  fieldId: number;
  fieldName: string;
  dailyForecasts: DailyForecast[];
  currentWeather: CurrentWeather;
}

export interface DailyForecast {
  date: string;
  maxTemp: number;
  minTemp: number;
  condition: string;
  chanceOfRain: number;
  rainfallMm: number | null;
  humidity: number;
  windSpeed: number;
  alert: string | null;
}

export interface CurrentWeather {
  temperature: number;
  humidity: number;
  windSpeed: number;
  condition: string;
  rainfallMm: number | null;
  observedAt: string;
}

export interface ManualWeatherEntry {
  fieldId: number;
  temperature?: number | null;
  humidity?: number | null;
  rainfallMm?: number | null;
  windSpeed?: number | null;
  condition?: string | null;
  recordedAt: string;
  notes?: string | null;
}

export interface WeatherAlertCreate {
  fieldId: number;
  alertType: string;
  severity: string;
  title: string;
  message: string;
  temperature?: number | null;
  windSpeed?: number | null;
  rainfallMm?: number | null;
  expiresAt?: string | null;
}

export interface WeatherAlertUpdate {
  severity?: string;
  title?: string;
  message?: string;
  isAcknowledged?: boolean;
  expiresAt?: string | null;
}

export interface WeatherAlertFilter {
  fieldId?: number | null;
  severity?: string | null;
  isAcknowledged?: boolean | null;
  isActive?: boolean | null;
  fromDate?: string | null;
  toDate?: string | null;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  isDescending?: boolean;
}

export interface WeatherHistoryFilter {
  fieldId?: number | null;
  fromDate?: string | null;
  toDate?: string | null;
  page?: number;
  pageSize?: number;
}

export interface WeatherApiSettings {
  apiProvider: string;
  apiKey: string;
  baseUrl: string;
  updateIntervalMinutes: number;
  autoUpdateEnabled: boolean;
}

export interface WeatherStatistics {
  totalRecords: number;
  fieldsWithData: number;
  averageTemperature: number;
  averageHumidity: number;
  totalRainfall: number;
  activeAlerts: number;
  criticalAlerts: number;
  lastUpdated: string;
}

// Alert Severity Colors
export const ALERT_SEVERITY_COLORS: Record<string, string> = {
  'ADVISORY': 'bg-yellow-100 text-yellow-800 border-yellow-400',
  'WATCH': 'bg-orange-100 text-orange-800 border-orange-400',
  'WARNING': 'bg-red-100 text-red-800 border-red-400',
  'EMERGENCY': 'bg-red-800 text-white border-red-900'
};

export const ALERT_SEVERITY_ICONS: Record<string, string> = {
  'ADVISORY': 'info',
  'WATCH': 'warning',
  'WARNING': 'warning',
  'EMERGENCY': 'error'
};

export const WEATHER_CONDITIONS = [
  'CLEAR', 'CLOUDY', 'RAINY', 'STORMY', 'SNOWY', 'FOGGY', 'WINDY'
];

export const WEATHER_ALERT_TYPES = [
  'STORM', 'FROST', 'HEAT_WAVE', 'HEAVY_RAIN', 'HIGH_WIND', 
  'DROUGHT', 'HAIL', 'TORNADO', 'FLOOD', 'WILDFIRE'
];

export const WEATHER_ALERT_SEVERITIES = [
  'ADVISORY', 'WATCH', 'WARNING', 'EMERGENCY'
];