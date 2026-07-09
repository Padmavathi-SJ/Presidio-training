import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather.component.ts"
admin_ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/admin/weather/weather.component.ts"

with open(admin_ts_path, "r") as f:
    ts = f.read()

# Replace Admin classes with Worker
ts = ts.replace("WeatherComponent", "WorkerWeatherComponent")
ts = ts.replace("selector: 'app-weather',", "selector: 'app-worker-weather',")

with open(ts_path, "w") as f:
    f.write(ts)
