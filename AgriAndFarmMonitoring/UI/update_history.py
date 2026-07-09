import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-history/weather-history.component.ts"
html_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-history/weather-history.component.html"

# Fix HTML
with open(html_path, "r") as f:
    html = f.read()

# Remove edit and delete buttons
html = re.sub(r'<button\s+mat-icon-button\s+color="primary"\s+\(click\)="editRecord\(row\)"[^>]*>.*?</button>', '', html, flags=re.DOTALL)
html = re.sub(r'<button\s+mat-icon-button\s+color="warn"\s+\(click\)="deleteRecord\(row\)"[^>]*>.*?</button>', '', html, flags=re.DOTALL)

# Remove select checkbox column
html = re.sub(r'<!-- Checkbox Column -->.*?</ng-container>', '', html, flags=re.DOTALL)
html = html.replace("'select', ", "")

# Fix field.id to field.fieldId in filters
html = html.replace('field.id', 'field.fieldId')

with open(html_path, "w") as f:
    f.write(html)

# Fix TS
with open(ts_path, "r") as f:
    ts = f.read()

ts = re.sub(r"import \{ ConfirmDialogComponent \}.*?\n", "", ts)
ts = re.sub(r"import \{ ManualWeatherEntryComponent \}.*?\n", "", ts)

# Fix getFields -> getMyAssignedFields
ts = ts.replace("getFields(farmId, { page: 1, pageSize: 100 })", "getMyAssignedFields()")
ts = ts.replace("this.fields = response.data.items;", "this.fields = response.data;")

# Remove edit/delete methods
ts = re.sub(r"editRecord\(record: WeatherData\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"deleteRecord\(record: WeatherData\): void \{.*?\}\n", "", ts, flags=re.DOTALL)

# Remove select logic
ts = re.sub(r"// =============================================\s*// BULK OPERATIONS\s*// =============================================.*?// FILTERS", "// FILTERS", ts, flags=re.DOTALL)
ts = re.sub(r"selectedRecords = signal<number\[\]>\(\[\]\);\n", "", ts)
ts = re.sub(r"isAllSelected\(\): boolean \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"toggleAllRows\(\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"toggleRow\(id: number\): void \{.*?\}\n", "", ts, flags=re.DOTALL)
ts = re.sub(r"isSelected\(id: number\): boolean \{.*?\}\n", "", ts, flags=re.DOTALL)

with open(ts_path, "w") as f:
    f.write(ts)
