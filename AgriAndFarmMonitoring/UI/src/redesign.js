const fs = require('fs');
const path = require('path');

const filesToRedesign = [
  'app/features/admin/harvests/harvests.component.html',
  'app/features/admin/observations/observations.component.html',
  'app/features/admin/quality-checks/quality-checks.component.html',
  'app/features/admin/sensors/sensors.component.html',
  'app/features/admin/sensors/sensor-readings/sensor-readings.component.html',
  'app/features/admin/sensors/alerts/alerts.component.html',
  'app/features/admin/weather/weather-data-history/weather-data-history.component.html',
  'app/features/admin/weather/weather-alerts/weather-alerts.component.html',
];

const basePath = '/Users/padmavathisj/Documents/Presidio-Training/AgriAndFarmMonitoring/UI/src';

const headers = {
  'harvests.component.html': { icon: 'agriculture', title: 'Harvests', sub: 'Review and approve harvest submissions' },
  'observations.component.html': { icon: 'visibility', title: 'Observations', sub: 'Review field observations and crop health reports' },
  'quality-checks.component.html': { icon: 'fact_check', title: 'Quality Checks', sub: 'Review quality inspection reports' },
  'sensors.component.html': { icon: 'sensors', title: 'Sensors', sub: 'Monitor real-time field sensor data' },
  'sensor-readings.component.html': { icon: 'sensors', title: 'Sensor Readings', sub: 'Detailed sensor measurement history' },
  'alerts.component.html': { icon: 'notifications_active', title: 'Sensor Alerts', sub: 'Critical threshold breaches and notifications' },
  'weather-data-history.component.html': { icon: 'cloud', title: 'Weather History', sub: 'Historical weather data for all fields' },
  'weather-alerts.component.html': { icon: 'storm', title: 'Weather Alerts', sub: 'Weather warnings and emergency notifications' }
};

for (const relPath of filesToRedesign) {
  const fullPath = path.join(basePath, relPath);
  if (!fs.existsSync(fullPath)) {
    console.log(`Skipping ${relPath} (not found)`);
    continue;
  }
  
  let content = fs.readFileSync(fullPath, 'utf8');
  const filename = path.basename(fullPath);
  const headerData = headers[filename];
  
  // 1. Outer wrappers
  content = content.replace(/<div class="min-h-screen[^>]*>\s*<div class="[^"]*max-w-7xl[^>]*>/i, '<div class="admin-page-shell">');
  // Just in case it's only one wrapper
  content = content.replace(/<div class="min-h-screen[^>]*>/, '<div class="admin-page-shell">');
  
  // 2. Large gradient header -> Compact Header
  // The header usually starts with <div class="relative overflow-hidden rounded-2xl bg-gradient-to-r...
  // Let's replace the whole block from that to the end of its inner wrappers
  const headerRegex = /<div class="relative overflow-hidden rounded-2xl bg-gradient-to-r[\s\S]*?<\/div>\s*<\/div>\s*<\/div>\s*<\/div>/;
  
  // We need to keep any buttons in the header actions. 
  // Let's extract buttons manually if possible, or just replace with standard if there are none.
  // We can do a simpler replace by preserving the right side actions if they exist.
  const match = content.match(headerRegex);
  if (match) {
    const headerHtml = match[0];
    let actionsHtml = '';
    
    // find buttons
    const btnRegex = /<button[\s\S]*?<\/button>/g;
    let buttons = [];
    let bMatch;
    while ((bMatch = btnRegex.exec(headerHtml)) !== null) {
      buttons.push(bMatch[0]);
    }
    
    if (buttons.length > 0) {
      actionsHtml = buttons.join('\n');
    }
    
    // Create compact header
    const compactHeader = `
    <!-- Page Header -->
    <div class="admin-compact-header">
      <div class="admin-header-left">
        <div class="admin-header-icon"><mat-icon>${headerData.icon}</mat-icon></div>
        <div>
          <h1 class="admin-header-title">${headerData.title}</h1>
          <p class="admin-header-subtitle">${headerData.sub}</p>
        </div>
      </div>
      <div class="admin-header-actions">
        ${actionsHtml}
      </div>
    </div>`;
    
    content = content.replace(headerRegex, compactHeader);
  }
  
  // 3. Stat Cards -> KPI Grid
  // Find grid container
  content = content.replace(/<div class="grid grid-cols-[^>]*gap-[^>]*mb-[^>]*>/g, '<div class="admin-kpi-grid mb-2">');
  content = content.replace(/<div class="grid grid-cols-[^>]*gap-[^>]*>/g, '<div class="admin-kpi-grid mb-2">');
  
  // Replace card classes
  content = content.replace(/<div class="group bg-white rounded-xl shadow-sm hover:shadow-md[^"]*border-l-4[^"]*border-[a-z]+-500[^"]*">/g, '<div class="admin-kpi-card kpi-teal">');
  // Or match any bg-white rounded-xl shadow-sm...
  content = content.replace(/class="group bg-white rounded-xl shadow-sm hover:shadow-md[^"]*"/g, 'class="admin-kpi-card kpi-teal"');
  
  // Replace inner kpi content if possible
  content = content.replace(/<div class="w-12 h-12 rounded-xl bg-[a-z]+-50 flex items-center justify-center text-[a-z]+-600[^"]*">/g, '<div class="kpi-icon-wrap">');
  content = content.replace(/<div class="p-4 sm:p-5">/g, ''); // strip padding wrapper if any
  content = content.replace(/<h3 class="text-xs sm:text-sm font-semibold text-gray-500 uppercase tracking-wider mb-1">/g, '<span class="kpi-label">');
  content = content.replace(/<\/h3>/g, '</span>');
  content = content.replace(/<div class="text-2xl sm:text-3xl font-bold text-gray-800">/g, '<span class="kpi-value">');
  
  // 4. Filter Mat Card -> admin-filter-card
  content = content.replace(/<mat-card class="mb-4 md:mb-6 p-2 sm:p-3 md:p-4 shadow-sm border border-gray-100 rounded-xl">/g, '<div class="admin-filter-card">');
  content = content.replace(/<mat-card class="mb-4 md:mb-6[^>]*>/g, '<div class="admin-filter-card">');
  content = content.replace(/<form \[formGroup\]="filterForm">/g, '<form [formGroup]="filterForm" class="admin-filter-row">');
  content = content.replace(/<div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-[^>]*gap-[^>]*>/g, '');
  
  // 5. Table Mat Card -> admin-table-card
  content = content.replace(/<mat-card class="shadow-sm border border-gray-100 rounded-xl overflow-hidden">/g, '<div class="admin-table-card"><div class="admin-table-wrap">');
  content = content.replace(/<table mat-table/g, '<table mat-table class="admin-data-table"');
  
  // Paginator
  content = content.replace(/<mat-paginator/g, '</div><mat-paginator class="admin-paginator"');
  
  // End of table card
  content = content.replace(/<\/mat-card>/g, '</div>');
  
  // Remove one extra trailing </div> from the removed inner wrapper
  content = content.replace(/<\/div>\s*<\/div>\s*$/i, '</div>\n');
  
  fs.writeFileSync(fullPath, content);
  console.log(`Updated ${relPath}`);
}
