import re

def fix_ts():
    with open('src/app/features/worker/weather/weather-history/weather-history.component.ts', 'r') as f:
        content = f.read()
    
    # Let's aggressively cut from "      });" to "clearSelection(): void {"
    # Actually, from "  // CRUD OPERATIONS" to "  resetFilters(): void {"
    content = re.sub(r'// CRUD OPERATIONS.*?// FILTERS', '// FILTERS', content, flags=re.DOTALL)
    
    with open('src/app/features/worker/weather/weather-history/weather-history.component.ts', 'w') as f:
        f.write(content)

def fix_html():
    with open('src/app/features/worker/weather/weather-history/weather-history.component.html', 'r') as f:
        content = f.read()

    # Remove the broken comment
    content = re.sub(r'<!--\s*<ng-container matColumnDef="actions">.*?</ng-container>', '', content, flags=re.DOTALL)
    
    # Also remove bulk actions
    content = re.sub(r'<!-- Bulk Actions -->.*?<!-- Loading State -->', '<!-- Loading State -->', content, flags=re.DOTALL)

    # Also remove "Add Data" button entirely
    content = re.sub(r'<!-- Add Data -->.*?<!-- Export -->', '<!-- Export -->', content, flags=re.DOTALL)
    
    # Also remove the Add Weather Data button from empty state
    content = re.sub(r'<button mat-raised-button color="primary" \(click\)="openManualEntry\(\)".*?</button>', '', content, flags=re.DOTALL)
    
    with open('src/app/features/worker/weather/weather-history/weather-history.component.html', 'w') as f:
        f.write(content)

fix_ts()
fix_html()
