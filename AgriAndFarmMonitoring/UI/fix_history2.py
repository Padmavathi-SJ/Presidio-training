import re

def fix_ts():
    with open('src/app/features/worker/weather/weather-history/weather-history.component.ts', 'r') as f:
        content = f.read()
    
    # Remove 'select' and 'actions' from displayedColumns
    content = re.sub(r"'select',\s*", "", content)
    content = re.sub(r"'actions'\s*", "", content)
    
    # Clean up trailing comma if any in displayedColumns
    content = re.sub(r",\s*]", "]", content)
    
    with open('src/app/features/worker/weather/weather-history/weather-history.component.ts', 'w') as f:
        f.write(content)

fix_ts()
