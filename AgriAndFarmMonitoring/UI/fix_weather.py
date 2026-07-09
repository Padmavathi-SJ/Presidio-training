import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather.component.ts"

with open(ts_path, "r") as f:
    ts = f.read()

# Fix imports
replacements = {
    "../services/weather.service": "../../services/worker-weather.service",
    "../services/weather-signalr.service": "../../../admin/services/weather-signalr.service",
    "../models/weather.model": "../../../admin/models/weather.model",
    "WeatherService": "WorkerWeatherService",
    "this.weatherService": "this.workerWeatherService",
    "WorkerWeatherService = inject(WorkerWeatherService)": "workerWeatherService = inject(WorkerWeatherService)"
}

for old, new in replacements.items():
    ts = ts.replace(old, new)
    
# Fix getActiveWeatherAlerts
ts = ts.replace("this.workerWeatherService.getActiveWeatherAlerts(farmId)", "this.workerWeatherService.getActiveWeatherAlerts(farmId, { page: 1, pageSize: 10 })")

with open(ts_path, "w") as f:
    f.write(ts)
