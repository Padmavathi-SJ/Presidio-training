import re

with open('src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts', 'r') as f:
    content = f.read()

# Fix getCurrentWeather
content = content.replace("this.weatherService.getCurrentWeather(farmId, fieldId)", "this.weatherService.getCurrentWeather(fieldId)")

# Fix getActiveWeatherAlerts
content = content.replace("this.weatherService.getActiveWeatherAlerts(farmId)", "this.weatherService.getWeatherAlerts({ isActive: true })")

# Remove loadStatistics call
content = re.sub(r'this\.loadStatistics\(farmId\),?', '', content)

# Remove loadStatistics function
content = re.sub(r'private loadStatistics\(farmId: number\): Promise<void> \{.*?\}\s*(?=\/\/ =============================================\s*\/\/ CHARTS INITIALIZATION)', '', content, flags=re.DOTALL)

with open('src/app/features/worker/weather/weather-dashboard/weather-dashboard.component.ts', 'w') as f:
    f.write(content)
