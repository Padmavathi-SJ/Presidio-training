import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather.component.ts"

with open(ts_path, "r") as f:
    ts = f.read()

# Remove the settings tab from the inline template
ts = re.sub(r'<a\s+mat-tab-link\s+routerLink="\./settings".*?Settings\s*</a>', '', ts, flags=re.DOTALL)

with open(ts_path, "w") as f:
    f.write(ts)
