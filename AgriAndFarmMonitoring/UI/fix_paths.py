import os

def replace_in_file(filepath, replacements):
    if not os.path.exists(filepath): return
    with open(filepath, "r") as f:
        content = f.read()
    
    for old, new in replacements.items():
        content = content.replace(old, new)
        
    with open(filepath, "w") as f:
        f.write(content)

replace_in_file("src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts", {
    "../../../../core/services/auth.service": "../../../core/services/auth.service",
    "../../../services/worker-weather.service": "../../services/worker-weather.service",
    "../../../services/worker-field.service": "../../services/worker-field.service",
    "../../../../admin/services/weather-signalr.service": "../../../admin/services/weather-signalr.service",
    "../../../../admin/models/weather.model": "../../../admin/models/weather.model",
    "/*\n    const farmId": "const farmId",
    "    */\n": "",
    "return; /*": "",
})

replace_in_file("src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts", {
    "../../../../core/services/auth.service": "../../../core/services/auth.service",
    "../../../services/worker-weather.service": "../../services/worker-weather.service",
    "../../../services/worker-field.service": "../../services/worker-field.service",
    "../../../../admin/models/weather.model": "../../../admin/models/weather.model",
})

replace_in_file("src/app/features/worker/weather/weather-history/weather-history.component.ts", {
    "../../../../../core/services/auth.service": "../../../core/services/auth.service",
    "../../../../core/services/auth.service": "../../../core/services/auth.service",
    "../../../services/worker-weather.service": "../../services/worker-weather.service",
    "../../../services/worker-field.service": "../../services/worker-field.service",
    "../../../../admin/models/weather.model": "../../../admin/models/weather.model",
})

replace_in_file("src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.ts", {
    "../../../../admin/models/weather.model": "../../../admin/models/weather.model",
    "../../../services/worker-weather.service": "../../services/worker-weather.service",
    "../../../../core/services/auth.service": "../../../core/services/auth.service",
})

replace_in_file("src/app/features/worker/worker.routes.ts", {
    "WorkerWorkerWeatherComponent": "WorkerWeatherComponent",
    "WorkerWeatherDashboardComponent": "WeatherDashboardComponent",
    "WorkerWeatherAlertsComponent": "WeatherAlertsComponent",
    "WorkerWeatherHistoryComponent": "WeatherDataHistoryComponent"
})

replace_in_file("src/app/features/worker/weather/weather.component.ts", {
    "import { WorkerWeatherComponent } from '../weather/weather.component';": "",
    "export class WorkerWorkerWeatherComponent": "export class WorkerWeatherComponent"
})
