import re
import glob

# Remove actions from dashboard
dashboard_ts = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts"
with open(dashboard_ts, "r") as f:
    ts = f.read()

ts = re.sub(r"acknowledgeAlert\(alert: WeatherAlert, event: Event\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"refreshWeatherData\(farmId, fieldId\)", "getCurrentWeather(fieldId)", ts)
ts = re.sub(r"refreshAllFields\(\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"this\.weatherService\.refreshWeatherData", "this.weatherService.getCurrentWeather", ts)

with open(dashboard_ts, "w") as f:
    f.write(ts)

dashboard_html = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.html"
with open(dashboard_html, "r") as f:
    html = f.read()
html = re.sub(r'<button mat-stroked-button color="primary".*?Resolve.*?</button>', '', html, flags=re.DOTALL)
html = re.sub(r'<button mat-button.*?Refresh All.*?</button>', '', html, flags=re.DOTALL)
with open(dashboard_html, "w") as f:
    f.write(html)

# Remove actions from alerts
alerts_ts = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts"
with open(alerts_ts, "r") as f:
    ts = f.read()
ts = re.sub(r"resolveAlert\(alert: WeatherAlert\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
with open(alerts_ts, "w") as f:
    f.write(ts)

alerts_html = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alerts/weather-alerts.component.html"
with open(alerts_html, "r") as f:
    html = f.read()
html = re.sub(r'<button mat-icon-button color="primary".*?resolveAlert.*?check_circle.*?</button>', '', html, flags=re.DOTALL)
html = html.replace("displayedColumns = ['title', 'severity', 'condition', 'field', 'date', 'status', 'actions']", "displayedColumns = ['title', 'severity', 'condition', 'field', 'date', 'status']")
with open(alerts_html, "w") as f:
    f.write(html)

# Remove actions from history
history_ts = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-history/weather-history.component.ts"
with open(history_ts, "r") as f:
    ts = f.read()
ts = ts.replace("['date', 'field', 'temperature', 'humidity', 'rainfall', 'condition', 'actions']", "['date', 'field', 'temperature', 'humidity', 'rainfall', 'condition']")
with open(history_ts, "w") as f:
    f.write(ts)

history_html = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-history/weather-history.component.html"
with open(history_html, "r") as f:
    html = f.read()
html = re.sub(r'<!-- Actions Column -->.*?</ng-container>', '', html, flags=re.DOTALL)
with open(history_html, "w") as f:
    f.write(html)

# Remove resolution from dialog
dialog_ts = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.ts"
with open(dialog_ts, "r") as f:
    ts = f.read()
ts = re.sub(r"resolveAlert\(\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
with open(dialog_ts, "w") as f:
    f.write(ts)

dialog_html = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.html"
with open(dialog_html, "r") as f:
    html = f.read()
html = re.sub(r'<!-- Resolution Notes -->.*</div>\s*</mat-dialog-content>', '</mat-dialog-content>', html, flags=re.DOTALL)
html = re.sub(r'<button mat-flat-button color="primary".*?Resolve Alert.*?</button>', '', html, flags=re.DOTALL)
with open(dialog_html, "w") as f:
    f.write(html)

