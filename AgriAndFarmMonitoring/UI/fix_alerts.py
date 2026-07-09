import re

with open('src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts', 'r') as f:
    content = f.read()

# I want to KEEP `viewAlert(alert: WeatherAlert)` but delete everything else in CRUD and BULK.
# Let's extract viewAlert.
view_alert_match = re.search(r'(  viewAlert\(alert: WeatherAlert\): void \{.*?\n  \})', content, flags=re.DOTALL)
view_alert = view_alert_match.group(1) if view_alert_match else ''

# Replace the whole CRUD and BULK section
content = re.sub(r'// CRUD OPERATIONS.*?// FILTERS', '// FILTERS', content, flags=re.DOTALL)

# Insert viewAlert back right before FILTERS
if view_alert:
    content = content.replace('// FILTERS', '// CRUD OPERATIONS\n\n' + view_alert + '\n\n  // FILTERS')

# Also fix the displayedColumns
content = re.sub(r"'select',\s*", "", content)
content = re.sub(r"'actions'\s*", "", content)
content = re.sub(r",\s*]", "]", content)

with open('src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts', 'w') as f:
    f.write(content)

with open('src/app/features/worker/weather/weather-alerts/weather-alerts.component.html', 'r') as f:
    html = f.read()

# Remove bulk actions
html = re.sub(r'<!-- Bulk Actions -->.*?<!-- Loading State -->', '<!-- Loading State -->', html, flags=re.DOTALL)

# Remove select column
html = re.sub(r'<!-- Select Column -->.*?<!-- Severity Column -->', '<!-- Severity Column -->', html, flags=re.DOTALL)

# Remove actions column (which was probably commented out but missing a close tag, or similar)
html = re.sub(r'<!-- Actions Column -->.*?<tr mat-header-row', '<tr mat-header-row', html, flags=re.DOTALL)
html = re.sub(r'<ng-container matColumnDef="actions">.*?</ng-container>', '', html, flags=re.DOTALL)

# Remove Add Alert button
html = re.sub(r'<!-- Add Alert -->.*?<!-- Refresh -->', '<!-- Refresh -->', html, flags=re.DOTALL)

with open('src/app/features/worker/weather/weather-alerts/weather-alerts.component.html', 'w') as f:
    f.write(html)
