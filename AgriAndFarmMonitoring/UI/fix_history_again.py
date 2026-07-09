import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-history/weather-history.component.ts"
with open(ts_path, "r") as f:
    ts = f.read()

# Completely remove all these methods which were for admin
ts = re.sub(r'openManualEntry\(\): void \{.*?\}\s*', '', ts, flags=re.DOTALL)
ts = re.sub(r'bulkDelete\(\): void \{.*?\}\s*', '', ts, flags=re.DOTALL)
ts = re.sub(r'clearSelection\(\): void \{.*?\}\s*', '', ts, flags=re.DOTALL)
ts = re.sub(r'editRecord\(.*?\): void \{.*?\}\s*', '', ts, flags=re.DOTALL)
ts = re.sub(r'deleteRecord\(.*?\): void \{.*?\}\s*', '', ts, flags=re.DOTALL)

with open(ts_path, "w") as f:
    f.write(ts)

html_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-history/weather-history.component.html"
with open(html_path, "r") as f:
    html = f.read()

html = re.sub(r'<button[^>]*\(click\)="openManualEntry\(\)"[^>]*>.*?</button>', '', html, flags=re.DOTALL)
html = re.sub(r'<button[^>]*\(click\)="bulkDelete\(\)"[^>]*>.*?</button>', '', html, flags=re.DOTALL)
html = re.sub(r'<button[^>]*\(click\)="clearSelection\(\)"[^>]*>.*?</button>', '', html, flags=re.DOTALL)

with open(html_path, "w") as f:
    f.write(html)
