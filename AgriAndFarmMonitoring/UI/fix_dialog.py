import re

with open('src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.ts', 'r') as f:
    ts = f.read()

# Fix FieldService import
ts = ts.replace("import { FieldService } from '../../services/field.service';", "import { WorkerFieldService } from '../../services/worker-field.service';")
ts = ts.replace("private fieldService = inject(FieldService);", "private fieldService = inject(WorkerFieldService);")

# Fix fieldService.getFields -> getMyAssignedFields
ts = ts.replace("this.fieldService.getFields(farmId, filter)", "this.fieldService.getMyAssignedFields()")
ts = ts.replace("const field = this.fields.find(f => f.id === fieldId);", "const field = this.fields.find(f => f.fieldId === fieldId);")

# Remove onSubmit and acknowledgeAlert completely
ts = re.sub(r'  onSubmit\(\): void \{.*?\n  \}', '', ts, flags=re.DOTALL)
ts = re.sub(r'  acknowledgeAlert\(\): void \{.*?\n  \}', '', ts, flags=re.DOTALL)

with open('src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.ts', 'w') as f:
    f.write(ts)

with open('src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.html', 'r') as f:
    html = f.read()

# Remove the save/acknowledge buttons and just keep Close
html = re.sub(r'<!-- Save/Acknowledge Actions -->.*?</div>', '<!-- Save/Acknowledge Actions -->\n</div>', html, flags=re.DOTALL)

with open('src/app/features/worker/weather/weather-alert-dialog/weather-alert-dialog.component.html', 'w') as f:
    f.write(html)

