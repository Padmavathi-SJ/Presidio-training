import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts"
html_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alerts/weather-alerts.component.html"

# Fix HTML
with open(html_path, "r") as f:
    html = f.read()

# Remove bulk actions
html = re.sub(r'<!-- Bulk Actions -->.*?</div>', '', html, flags=re.DOTALL)

# Remove row selection column
html = re.sub(r'<!-- Checkbox Column -->.*?</ng-container>', '', html, flags=re.DOTALL)
html = html.replace("'select', ", "")

# Fix field.id to field.fieldId in filters
html = html.replace('field.id', 'field.fieldId')

# Fix acknowledge action to resolve action
html = html.replace('acknowledgeAlert(row)', 'resolveAlert(row)')
html = html.replace('Acknowledge', 'Resolve')
html = html.replace('done_all', 'check_circle')
html = html.replace('row.isAcknowledged', 'row.status === "RESOLVED"')

with open(html_path, "w") as f:
    f.write(html)

# Fix TS
with open(ts_path, "r") as f:
    ts = f.read()

ts = re.sub(r"import \{ ConfirmDialogComponent \}.*?\n", "", ts)

# Fix getFields -> getMyAssignedFields
ts = ts.replace("getFields(farmId, { page: 1, pageSize: 100 })", "getMyAssignedFields()")
ts = ts.replace("this.fields = response.data.items;", "this.fields = response.data;")

# Remove bulk methods
ts = re.sub(r"// =============================================\s*// BULK OPERATIONS\s*// =============================================.*?// FILTERS", "// FILTERS", ts, flags=re.DOTALL)

# Remove selectedAlerts and toggleSelection etc.
ts = re.sub(r"selectedAlerts = signal<number\[\]>\(\[\]\);\n", "", ts)
ts = re.sub(r"isAllSelected\(\): boolean \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"toggleAllRows\(\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"toggleRow\(id: number\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"isSelected\(id: number\): boolean \{.*?\}\n", "", ts, flags=re.DOTALL)

# Fix acknowledge method
ts = ts.replace("acknowledgeAlert(alert: WeatherAlert): void {", "resolveAlert(alert: WeatherAlert): void {")
ts = ts.replace("this.weatherService.acknowledgeWeatherAlert(farmId, alert.id)", "this.weatherService.resolveAlert(farmId, alert.id, 'Resolved via alerts list')")

with open(ts_path, "w") as f:
    f.write(ts)
