import re

with open('src/app/features/worker/weather/weather-alerts/weather-alerts.component.html', 'r') as f:
    html = f.read()

# Fix the open comment
html = re.sub(r'<!--\s+<tr mat-header-row', '<tr mat-header-row', html)

# Also remove openCreateDialog button in empty state
html = re.sub(r'<button mat-raised-button color="primary" \(click\)="openCreateDialog\(\)" class="mt-4">.*?Create Alert\s*</button>', '', html, flags=re.DOTALL)

with open('src/app/features/worker/weather/weather-alerts/weather-alerts.component.html', 'w') as f:
    f.write(html)
