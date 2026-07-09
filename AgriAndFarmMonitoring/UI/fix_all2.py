import re

# 1. weather-alerts.component.ts
path = 'src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts'
with open(path, 'r') as f:
    c = f.read()

c = c.replace("this.weatherService.getWeatherAlerts(farmId, filter)", "this.weatherService.getWeatherAlerts(filter)")

with open(path, 'w') as f:
    f.write(c)

# 2. weather-alerts.component.html
path = 'src/app/features/worker/weather/weather-alerts/weather-alerts.component.html'
with open(path, 'r') as f:
    c = f.read()

# Remove the whole <button> for New Alert
c = re.sub(r'<!-- Create Alert -->\s*<button[^>]*\(click\)="openCreateDialog\(\)"[^>]*>.*?New Alert\s*</button>', '<!-- Create Alert removed -->', c, flags=re.DOTALL)

with open(path, 'w') as f:
    f.write(c)

# 3. weather-alert-dialog.component.ts
path = 'src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.ts'
with open(path, 'r') as f:
    c = f.read()

c = c.replace("this.fields = response.data.items;", "this.fields = response.data;")

with open(path, 'w') as f:
    f.write(c)

# 4. weather-alert-dialog.component.html
path = 'src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.html'
with open(path, 'r') as f:
    c = f.read()

c = c.replace('(ngSubmit)="onSubmit()"', '')
c = re.sub(r'<button mat-raised-button color="accent" \(click\)="acknowledgeAlert\(\)".*?</button>', '', c, flags=re.DOTALL)

with open(path, 'w') as f:
    f.write(c)

# 5. weather-dashboard.component.ts
path = 'src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts'
with open(path, 'r') as f:
    c = f.read()

c = c.replace("private chartDataService = inject(ChartDataService);", "")

with open(path, 'w') as f:
    f.write(c)

