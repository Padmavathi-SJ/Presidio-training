import re

# 1. weather-alerts.component.ts
path1 = 'src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts'
with open(path1, 'r') as f:
    c1 = f.read()

c1 = c1.replace("../../services/weather-signalr.service", "../../../admin/services/weather-signalr.service")
c1 = c1.replace("this.fieldService.getFields(farmId, filter)", "this.fieldService.getMyAssignedFields()")
c1 = c1.replace("const field = this.fields.find(f => f.id === fieldId);", "const field = this.fields.find(f => f.fieldId === fieldId);")

with open(path1, 'w') as f:
    f.write(c1)

# 2. weather-dashboard.component.ts
path2_ts = 'src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts'
with open(path2_ts, 'r') as f:
    c2 = f.read()

c2 = re.sub(r"import \{ ChartDataService \} from '../../services/chart-data.service';\n?", "", c2)
c2 = c2.replace("this.fieldService.getFields(farmId, filter)", "this.fieldService.getMyAssignedFields()")
c2 = c2.replace("const field = this.fields.find(f => f.id === fieldId);", "const field = this.fields.find(f => f.fieldId === fieldId);")
c2 = c2.replace("this.activeAlerts.set(response.data || []);", "this.activeAlerts.set(response.data?.items || []);")
c2 = c2.replace("this.viewAlertDetails(alert);", "// this.viewAlertDetails(alert);")
c2 = c2.replace("private chartService = inject(ChartDataService);", "")

with open(path2_ts, 'w') as f:
    f.write(c2)

path2_html = 'src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.html'
with open(path2_html, 'r') as f:
    c2h = f.read()

c2h = c2h.replace('(click)="openManualEntry()"', "")
c2h = c2h.replace('(click)="viewAlertDetails(alert)"', "")
c2h = c2h.replace('(click)="acknowledgeAlert(alert, $event)"', "")

with open(path2_html, 'w') as f:
    f.write(c2h)

# 3. weather-history.component.ts
path3_ts = 'src/app/features/worker/weather/weather-history/weather-history.component.ts'
with open(path3_ts, 'r') as f:
    c3 = f.read()

c3 = c3.replace("this.fieldService.getFields(farmId, filter)", "this.fieldService.getMyAssignedFields()")
c3 = c3.replace("const field = this.fields.find(f => f.id === fieldId);", "const field = this.fields.find(f => f.fieldId === fieldId);")

with open(path3_ts, 'w') as f:
    f.write(c3)

