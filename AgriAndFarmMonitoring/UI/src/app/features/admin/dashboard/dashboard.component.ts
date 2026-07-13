import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatTabsModule } from '@angular/material/tabs';
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

import { AuthService } from '../../../core/services/auth.service';
import { TaskService } from '../services/task.service';
import { AdminHarvestService } from '../harvests/services/admin-harvest.service';
import { AdminQualityCheckService } from '../quality-checks/services/admin-quality-check.service';
import { SensorService } from '../services/sensor.service';
import { FieldService } from '../services/field.service';
import { CropCycleService } from '../services/crop-cycle.service';
import { WorkerService } from '../services/worker.service';
import { AdminObservationService } from '../services/admin-observation.service';
import { WeatherService } from '../services/weather.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatDividerModule,
    MatMenuModule,
    MatTooltipModule,
    MatTabsModule,
    BaseChartDirective
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private taskService = inject(TaskService);
  private harvestService = inject(AdminHarvestService);
  private qcService = inject(AdminQualityCheckService);
  private sensorService = inject(SensorService);
  private fieldService = inject(FieldService);
  private cropCycleService = inject(CropCycleService);
  private workerService = inject(WorkerService);
  private obsService = inject(AdminObservationService);
  private weatherService = inject(WeatherService);

  isLoading = true;
  userName = '';
  farmName = '';

  // --- STATS ---
  stats = {
    monthlyRevenue: 0,
    totalFields: 0,
    totalWorkers: 0,
    activeCrops: 0,
    pendingObservations: 0,
    pendingHarvests: 0,
    pendingQualityChecks: 0,
    unresolvedSensorAlerts: 0,
    unresolvedWeatherAlerts: 0
  };

  // --- ARRAYS FOR PANELS ---
  topCriticalSensors: any[] = [];
  topCriticalWeathers: any[] = [];
  recentPendingObs: any[] = [];
  recentPendingHarvests: any[] = [];
  recentPendingQcs: any[] = [];
  recentActivities: any[] = [];

  // --- CHARTS CONFIG ---
  revenueChartData: any = { labels: [], datasets: [] };
  revenueChartOptions: any = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { mode: 'index', intersect: false } },
    elements: { line: { tension: 0.4, borderWidth: 3 }, point: { radius: 0, hoverRadius: 6 } },
    scales: {
      x: { grid: { display: false } },
      y: { grid: { color: 'rgba(0,0,0,0.05)' }, beginAtZero: true }
    }
  };



  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) return;
    const user = this.authService.getCurrentUser();
    if (user) {
      this.userName = user.name || 'Admin';
      this.farmName = user.farmName || 'Your Farm';
    }
    this.loadAllData();
  }

  private loadAllData(): void {
    const farmId = this.authService.getFarmId();
    if (!farmId) {
      this.isLoading = false;
      return;
    }
    this.isLoading = true;

    const filter = { page: 1, pageSize: 100 };

    forkJoin({
      fields: this.fieldService.getFields(farmId, filter).pipe(catchError(() => of(null))),
      workers: this.workerService.getWorkers(farmId, filter).pipe(catchError(() => of(null))),
      cropCycles: this.cropCycleService.getCropCycles(farmId, filter).pipe(catchError(() => of(null))),
      tasks: this.taskService.getTasks(farmId, filter).pipe(catchError(() => of(null))),
      harvests: this.harvestService.getHarvests(farmId, filter).pipe(catchError(() => of(null))),
      obs: this.obsService.getObservations(farmId, filter).pipe(catchError(() => of(null))),
      sensorAlerts: this.sensorService.getAlerts(farmId, filter).pipe(catchError(() => of(null))),
      weatherAlerts: this.weatherService.getWeatherAlerts(farmId, filter).pipe(catchError(() => of(null))),
      qcs: this.qcService.getAll(filter).pipe(catchError(() => of(null)))
    }).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe((results: any) => {
      this.processData(results);
    });
  }

  private processData(results: any): void {
    const fields = results.fields?.data?.items || [];
    const workers = results.workers?.data?.items || [];
    const cropCycles = results.cropCycles?.data?.items || [];
    const harvests = results.harvests?.data?.items || [];
    const obs = results.obs?.data?.items || [];
    const qcs = results.qcs?.data?.items || [];
    const sensorAlerts = results.sensorAlerts?.data?.items || [];
    const weatherAlerts = results.weatherAlerts?.data?.items || [];
    const tasks = results.tasks?.data?.items || [];

    // Top Stats
    this.stats.totalFields = results.fields?.data?.totalCount || 0;
    this.stats.totalWorkers = results.workers?.data?.totalCount || 0;
    this.stats.activeCrops = cropCycles.filter((c: any) => c.status?.toUpperCase() === 'ACTIVE' || c.status?.toUpperCase() === 'IN_PROGRESS').length || 0;

    // Approvals Logic
    const pendingObsList = obs.filter((o: any) => o.validationStatus === 'pending' || o.validationStatus === 'PENDING').map((o: any) => ({ ...o, type: 'Observation' }));
    const pendingHarvestsList = harvests.filter((h: any) => h.approvalStatus === 'PENDING').map((h: any) => ({ ...h, type: 'Harvest' }));
    const pendingQcsList = qcs.filter((q: any) => q.approvalStatus === 'PENDING').map((q: any) => ({ ...q, type: 'Quality Check' }));
    
    this.stats.pendingObservations = pendingObsList.length;
    this.stats.pendingHarvests = pendingHarvestsList.length;
    this.stats.pendingQualityChecks = pendingQcsList.length;

    this.recentPendingObs = pendingObsList.sort((a: any, b: any) => new Date(b.observationDate).getTime() - new Date(a.observationDate).getTime()).slice(0, 5);
    this.recentPendingHarvests = pendingHarvestsList.sort((a: any, b: any) => new Date(b.harvestDate).getTime() - new Date(a.harvestDate).getTime()).slice(0, 5);
    this.recentPendingQcs = pendingQcsList.sort((a: any, b: any) => new Date(b.checkDate).getTime() - new Date(a.checkDate).getTime()).slice(0, 5);

    // Alerts Logic
    const activeSensorAlerts = sensorAlerts.filter((a: any) => !a.isResolved);
    const activeWeatherAlerts = weatherAlerts.filter((a: any) => !a.isAcknowledged);
    this.stats.unresolvedSensorAlerts = activeSensorAlerts.length;
    this.stats.unresolvedWeatherAlerts = activeWeatherAlerts.length;

    const criticalSensors = activeSensorAlerts.filter((a: any) => a.severity === 'Critical' || a.severity === 'High' || a.severity === 'CRITICAL' || a.severity === 'HIGH').map((a: any) => ({ ...a, source: 'Sensor', date: a.createdAt }));
    const criticalWeathers = activeWeatherAlerts.filter((a: any) => a.severity === 'Emergency' || a.severity === 'Warning' || a.severity === 'EMERGENCY' || a.severity === 'WARNING').map((a: any) => ({ ...a, source: 'Weather', date: a.alertTime }));
    
    this.topCriticalSensors = criticalSensors.sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime()).slice(0, 3);
    this.topCriticalWeathers = criticalWeathers.sort((a: any, b: any) => new Date(b.date).getTime() - new Date(a.date).getTime()).slice(0, 3);

    this.calculateRevenue(harvests);
    this.buildActivityFeed(tasks, harvests, obs);
  }



  private calculateRevenue(harvests: any[]): void {
    const now = new Date();
    const currentMonth = now.getMonth();
    const currentYear = now.getFullYear();

    let currentMonthRevenue = 0;
    const monthlyRevenueMap = new Map<string, number>();

    for (let i = 5; i >= 0; i--) {
      const d = new Date(currentYear, currentMonth - i, 1);
      const key = d.toLocaleString('default', { month: 'short' });
      monthlyRevenueMap.set(key, 0);
    }

    harvests.forEach(h => {
      if (h.approvalStatus === 'APPROVED' && h.quantityKg && h.pricePerKg) {
        const rev = h.quantityKg * h.pricePerKg;
        const hDate = new Date(h.harvestDate);
        if (hDate.getMonth() === currentMonth && hDate.getFullYear() === currentYear) {
          currentMonthRevenue += rev;
        }
        
        const monthDiff = (currentYear - hDate.getFullYear()) * 12 + (currentMonth - hDate.getMonth());
        if (monthDiff >= 0 && monthDiff <= 5) {
          const key = hDate.toLocaleString('default', { month: 'short' });
          if (monthlyRevenueMap.has(key)) {
            monthlyRevenueMap.set(key, monthlyRevenueMap.get(key)! + rev);
          }
        }
      }
    });

    this.stats.monthlyRevenue = currentMonthRevenue;
    this.revenueChartData = {
      labels: Array.from(monthlyRevenueMap.keys()),
      datasets: [{
        label: 'Revenue ($)',
        data: Array.from(monthlyRevenueMap.values()),
        borderColor: '#10b981',
        backgroundColor: 'rgba(16, 185, 129, 0.1)',
        fill: true,
      }]
    };
  }



  private buildActivityFeed(tasks: any[], harvests: any[], obs: any[]): void {
    const feed: any[] = [];
    
    tasks.slice(0, 10).forEach(t => feed.push({
      type: 'Task',
      icon: 'check_circle',
      color: 'text-blue-500',
      bg: 'bg-blue-50',
      title: 'Task: ' + t.taskName,
      desc: 'Status: ' + t.status,
      date: new Date(t.updatedAt || t.assignedDate)
    }));

    harvests.slice(0, 10).forEach(h => feed.push({
      type: 'Harvest',
      icon: 'agriculture',
      color: 'text-green-500',
      bg: 'bg-green-50',
      title: 'Harvest Logged',
      desc: h.quantityKg + ' kg from ' + h.fieldName,
      date: new Date(h.harvestDate)
    }));

    obs.slice(0, 10).forEach(o => feed.push({
      type: 'Observation',
      icon: 'visibility',
      color: 'text-purple-500',
      bg: 'bg-purple-50',
      title: 'Observation Made',
      desc: 'Health: ' + o.cropHealth + ' at ' + o.fieldName,
      date: new Date(o.observationDate)
    }));

    this.recentActivities = feed.sort((a, b) => b.date.getTime() - a.date.getTime()).slice(0, 6);
  }



  // --- ACTIONS ---
  quickAction(action: string): void {
    if (action === 'addField') this.router.navigate(['/admin/fields']);
    if (action === 'addWorker') this.router.navigate(['/admin/worker-fields']);
    if (action === 'assignTask') this.router.navigate(['/admin/tasks']);
    if (action === 'reports') this.router.navigate(['/admin/tasks']);
  }

  approveItem(item: any): void {
    if (item.type === 'Harvest') {
      this.router.navigate(['/admin/harvests']);
    } else {
      this.router.navigate(['/admin/quality-checks']);
    }
  }

  resolveAlert(alert: any): void {
    this.router.navigate(['/admin/sensors/alerts']);
  }

  getHealthColor(health: string): string {
    switch (health) {
      case 'EXCELLENT': return 'bg-green-100 text-green-800';
      case 'GOOD': return 'bg-emerald-100 text-emerald-800';
      case 'AVERAGE': return 'bg-yellow-100 text-yellow-800';
      case 'POOR': return 'bg-orange-100 text-orange-800';
      case 'CRITICAL': return 'bg-red-100 text-red-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }
}