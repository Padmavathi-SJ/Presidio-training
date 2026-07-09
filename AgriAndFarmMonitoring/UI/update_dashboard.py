import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts"
html_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.html"

# Fix HTML
with open(html_path, "r") as f:
    html = f.read()

# Remove Add Data button
html = re.sub(r'<!-- Manual Entry -->.*?</button>', '', html, flags=re.DOTALL)
# Remove Acknowledge button if exists in dashboard alerts list
html = re.sub(r'<button\s+mat-stroked-button\s+color="primary"\s+\(click\)="acknowledgeAlert\(alert, \$event\)".*?</button>', '', html, flags=re.DOTALL)

# Fix field.id to field.fieldId in select
html = html.replace('field.id', 'field.fieldId')

with open(html_path, "w") as f:
    f.write(html)

# Fix TS
with open(ts_path, "r") as f:
    ts = f.read()

ts = re.sub(r"import \{ ManualWeatherEntryComponent \}.*?\n", "", ts)
ts = re.sub(r"ManualWeatherEntryComponent,?\n?", "", ts)
ts = re.sub(r"openManualEntry\(\): void \{.*?\n  \}\n", "", ts, flags=re.DOTALL)

# Fix getFields -> getMyAssignedFields
ts = ts.replace("getFields(farmId, { page: 1, pageSize: 100 })", "getMyAssignedFields()")
ts = ts.replace("this.fields = response.data.items;", "this.fields = response.data;")

# Fix acknowledge to just call resolve if needed
ts = ts.replace("acknowledgeWeatherAlert(", "resolveAlert(")
ts = ts.replace("this.weatherService.resolveAlert(farmId, alert.id)", "this.weatherService.resolveAlert(farmId, alert.id, 'Resolved via dashboard')")

with open(ts_path, "w") as f:
    f.write(ts)
