import os

def replace_in_file(filepath, replacements):
    if not os.path.exists(filepath): return
    with open(filepath, "r") as f:
        content = f.read()
    
    for old, new in replacements.items():
        content = content.replace(old, new)
        
    with open(filepath, "w") as f:
        f.write(content)

# DASHBOARD TS
replace_in_file("src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts", {
    "WeatherDashboardComponent": "WorkerWeatherDashboardComponent",
    "app-weather-dashboard": "app-worker-weather-dashboard",
    "WeatherService": "WorkerWeatherService",
    "WeatherAlertDialogComponent": "WorkerWeatherAlertDialogComponent",
    "FieldService": "WorkerFieldService",
    "../../../core/services/auth.service": "../../../../core/services/auth.service",
    "../../services/weather.service": "../../../services/worker-weather.service",
    "../../services/field.service": "../../../services/worker-field.service",
    "../../services/weather-signalr.service": "../../../../admin/services/weather-signalr.service",
    "../../models/weather.model": "../../../../admin/models/weather.model",
    "../services/weather.service": "../../services/worker-weather.service",
    "../services/field.service": "../../services/worker-field.service",
    "../services/weather-signalr.service": "../../../admin/services/weather-signalr.service",
    "../models/weather.model": "../../../admin/models/weather.model",
    "this.fieldService.getFields(farmId, { page: 1, pageSize: 100 })": "this.fieldService.getMyAssignedFields()",
    "this.fields = response.data.items;": "this.fields = response.data;",
    "this.weatherService.refreshWeatherData(farmId, fieldId)": "this.weatherService.getCurrentWeather(fieldId)",
    "this.weatherService.refreshWeatherData(farmId, 0)": "this.weatherService.getCurrentWeather(0)",
    "import { ManualWeatherEntryComponent } from '../manual-weather-entry/manual-weather-entry.component';\n": "",
    "const dialogRef = this.dialog.open(ManualWeatherEntryComponent, {": "return; /*\n",
    "data: { fieldId: this.selectedFieldId }\n    });": "*/\n",
    "dialogRef.afterClosed().subscribe(result => {": "/*\n",
    "this.loadFieldWeather(this.selectedFieldId);\n      }\n    });": "*/\n",
    "this.weatherService.acknowledgeWeatherAlert(farmId, alert.id)": "/*"
})

# DASHBOARD HTML
replace_in_file("src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.html", {
    "field.id": "field.fieldId",
    '<button mat-stroked-button color="primary" (click)="openManualEntry()">': '<!--',
    "Manual Entry\n    </button>": "-->",
    '<button mat-button (click)="refreshAllFields()">': '<!--',
    "Refresh All\n    </button>": "-->",
    '<button mat-stroked-button color="primary" (click)="acknowledgeAlert(alert, $event)">': '<!--',
    "Resolve\n                  </button>": "-->"
})

# ALERTS TS
replace_in_file("src/app/features/worker/weather/weather-alerts/weather-alerts.component.ts", {
    "WeatherAlertsComponent": "WorkerWeatherAlertsComponent",
    "app-weather-alerts": "app-worker-weather-alerts",
    "WeatherService": "WorkerWeatherService",
    "WeatherAlertDialogComponent": "WorkerWeatherAlertDialogComponent",
    "FieldService": "WorkerFieldService",
    "../../../core/services/auth.service": "../../../../core/services/auth.service",
    "../../services/weather.service": "../../../services/worker-weather.service",
    "../../services/field.service": "../../../services/worker-field.service",
    "../../models/weather.model": "../../../../admin/models/weather.model",
    "../services/weather.service": "../../services/worker-weather.service",
    "../services/field.service": "../../services/worker-field.service",
    "../models/weather.model": "../../../admin/models/weather.model",
    "this.fieldService.getFields(farmId, { page: 1, pageSize: 100 })": "this.fieldService.getMyAssignedFields()",
    "this.fields = response.data.items;": "this.fields = response.data;",
    "import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';\n": "",
    "import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';\n": "",
    "displayedColumns = ['select', 'title', 'severity', 'condition', 'field', 'date', 'status', 'actions'];": "displayedColumns = ['title', 'severity', 'condition', 'field', 'date', 'status'];",
    "deleteAlert(alert: WeatherAlert): void {": "deleteAlert(alert: WeatherAlert): void { return; ",
    "acknowledgeAlert(alert: WeatherAlert): void {": "acknowledgeAlert(alert: WeatherAlert): void { return; ",
    "bulkAcknowledge(): void {": "bulkAcknowledge(): void { return; "
})

# ALERTS HTML
replace_in_file("src/app/features/worker/weather/weather-alerts/weather-alerts.component.html", {
    "field.id": "field.fieldId",
    "displayedColumns = ['select', 'title', 'severity', 'condition', 'field', 'date', 'status', 'actions']": "displayedColumns = ['title', 'severity', 'condition', 'field', 'date', 'status']",
    '<div class="flex items-center gap-2 mb-4 p-3 bg-blue-50 text-blue-800 rounded-lg" *ngIf="selectedAlerts().length > 0">': '<!--',
    "Resolve Selected\n      </button>\n    </div>": "-->",
    "<!-- Checkbox Column -->": "<!--",
    '</mat-checkbox>\n          </td>\n        </ng-container>': "-->",
    "<!-- Actions Column -->": "<!--",
    '</mat-icon>\n            </button>\n          </td>\n        </ng-container>': "-->"
})

# HISTORY TS
replace_in_file("src/app/features/worker/weather/weather-history/weather-history.component.ts", {
    "WeatherDataHistoryComponent": "WorkerWeatherHistoryComponent",
    "app-weather-data-history": "app-worker-weather-history",
    "weather-data-history.component.html": "weather-history.component.html",
    "weather-data-history.component.scss": "weather-history.component.scss",
    "WeatherService": "WorkerWeatherService",
    "FieldService": "WorkerFieldService",
    "../../../core/services/auth.service": "../../../../core/services/auth.service",
    "../../services/weather.service": "../../../services/worker-weather.service",
    "../../services/field.service": "../../../services/worker-field.service",
    "../../models/weather.model": "../../../../admin/models/weather.model",
    "../services/weather.service": "../../services/worker-weather.service",
    "../services/field.service": "../../services/worker-field.service",
    "../models/weather.model": "../../../admin/models/weather.model",
    "this.fieldService.getFields(farmId, { page: 1, pageSize: 100 })": "this.fieldService.getMyAssignedFields()",
    "this.fields = response.data.items;": "this.fields = response.data;",
    "this.weatherService.getWeatherHistory(farmId, filter)": "this.weatherService.getWeatherHistory(filter)",
    "import { ManualWeatherEntryComponent } from '../manual-weather-entry/manual-weather-entry.component';\n": "",
    "import { ConfirmDialogComponent } from '../../../../shared/components/confirm-dialog/confirm-dialog.component';\n": "",
    "import { ConfirmDialogComponent } from '../../../../../shared/components/confirm-dialog/confirm-dialog.component';\n": "",
    "displayedColumns = ['select', 'date', 'field', 'temperature', 'humidity', 'rainfall', 'condition', 'actions'];": "displayedColumns = ['date', 'field', 'temperature', 'humidity', 'rainfall', 'condition'];",
    "openManualEntry(): void {": "openManualEntry(): void { return; ",
    "editRecord(data: WeatherData): void {": "editRecord(data: WeatherData): void { return; ",
    "deleteRecord(data: WeatherData): void {": "deleteRecord(data: WeatherData): void { return; ",
    "bulkDelete(): void {": "bulkDelete(): void { return; "
})

# HISTORY HTML
replace_in_file("src/app/features/worker/weather/weather-history/weather-history.component.html", {
    "field.id": "field.fieldId",
    "['select', 'date', 'field', 'temperature', 'humidity', 'rainfall', 'condition', 'actions']": "['date', 'field', 'temperature', 'humidity', 'rainfall', 'condition']",
    '<button mat-stroked-button color="primary" (click)="openManualEntry()">': '<!--',
    "Manual Entry\n      </button>": "-->",
    '<div class="flex items-center gap-2 mb-4 p-3 bg-blue-50 text-blue-800 rounded-lg" *ngIf="selectedRecords().length > 0">': '<!--',
    "Delete Selected\n      </button>\n    </div>": "-->",
    "<!-- Checkbox Column -->": "<!--",
    '</mat-checkbox>\n          </td>\n        </ng-container>': "-->",
    "<!-- Actions Column -->": "<!--",
    '</mat-icon>\n            </button>\n          </td>\n        </ng-container>': "-->",
    '<button mat-flat-button color="primary" (click)="openManualEntry()" class="mt-4">': '<!--',
    "Record Weather Data\n        </button>": "-->"
})

# DIALOG TS
replace_in_file("src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.ts", {
    "WeatherAlertDialogComponent": "WorkerWeatherAlertDialogComponent",
    "app-weather-alert-dialog": "app-worker-weather-alert-dialog",
    "WeatherService": "WorkerWeatherService",
    "../../../core/services/auth.service": "../../../../core/services/auth.service",
    "../../services/weather.service": "../../../services/worker-weather.service",
    "../../models/weather.model": "../../../../admin/models/weather.model",
    "../services/weather.service": "../../services/worker-weather.service",
    "../models/weather.model": "../../../admin/models/weather.model",
    "acknowledgeAlert(): void {": "acknowledgeAlert(): void { return; "
})

# DIALOG HTML
replace_in_file("src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.html", {
    '<button mat-flat-button color="primary"': '<!--',
    "Acknowledge Alert }}\n  </button>": "-->"
})

# ORCHESTRATOR TS
replace_in_file("src/app/features/worker/weather/weather.component.ts", {
    "import { WeatherComponent } from '../../admin/weather/weather.component';\n": "",
    "import { WeatherService } from '../../admin/services/weather.service';": "import { WorkerWeatherService } from '../services/worker-weather.service';",
    "private weatherService = inject(WeatherService);": "private weatherService = inject(WorkerWeatherService);",
    "this.weatherService.getActiveWeatherAlerts(farmId)": "this.weatherService.getActiveWeatherAlerts(farmId, { page: 1, pageSize: 10 })"
})

