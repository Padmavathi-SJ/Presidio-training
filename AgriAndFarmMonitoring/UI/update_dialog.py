import re

ts_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.ts"
html_path = "/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.html"

# Fix HTML
with open(html_path, "r") as f:
    html = f.read()

# Replace Acknowledge button with Resolve button and add text area
html = html.replace('Acknowledge Alert', 'Resolve Alert')
html = html.replace('(click)="acknowledgeAlert()"', '(click)="resolveAlert()"')
html = html.replace('[disabled]="isSubmitting()"', '[disabled]="isSubmitting() || !resolutionNotes()"')

# Add a textarea for notes before actions
textarea_html = """
  <!-- Resolution Notes -->
  <div class="mt-6" *ngIf="data.mode === 'view' && !data.alert.isAcknowledged && data.alert.status !== 'RESOLVED'">
    <mat-form-field appearance="outline" class="w-full">
      <mat-label>Resolution Notes</mat-label>
      <textarea matInput [ngModel]="resolutionNotes()" (ngModelChange)="resolutionNotes.set($event)" rows="3" placeholder="How was this alert resolved?"></textarea>
    </mat-form-field>
  </div>
"""
html = html.replace('</mat-dialog-content>', textarea_html + '\n</mat-dialog-content>')

with open(html_path, "w") as f:
    f.write(html)

# Fix TS
with open(ts_path, "r") as f:
    ts = f.read()

ts = ts.replace('import { FormsModule } from \'@angular/forms\';', 'import { FormsModule } from \'@angular/forms\';\nimport { MatInputModule } from \'@angular/material/input\';')
ts = ts.replace('imports: [', 'imports: [\n    MatInputModule,')

ts = ts.replace('acknowledgeAlert(', 'resolveAlert(')
ts = ts.replace('this.weatherService.acknowledgeWeatherAlert(farmId, this.data.alert.id)', 'this.weatherService.resolveAlert(farmId, this.data.alert.id, this.resolutionNotes())')

# Add signal for notes
ts = ts.replace('isSubmitting = signal(false);', 'isSubmitting = signal(false);\n  resolutionNotes = signal("");')

with open(ts_path, "w") as f:
    f.write(ts)
