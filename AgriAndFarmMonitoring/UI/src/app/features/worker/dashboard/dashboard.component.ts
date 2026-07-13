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
import { BaseChartDirective } from 'ng2-charts';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { forkJoin, of } from 'rxjs';
import { catchError, finalize } from 'rxjs/operators';

import { AuthService } from '../../../core/services/auth.service';
import { WorkerTaskService } from '../services/worker-task.service';
import { WorkerHarvestService } from '../services/worker-harvest.service';
import { WorkerObservationService } from '../services/worker-observation.service';
import { WorkerFieldService } from '../services/worker-field.service';
import { WorkerQualityCheckService } from '../quality-checks/services/worker-quality-check.service';

@Component({
  selector: 'app-worker-dashboard',
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
    BaseChartDirective
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  private authService = inject(AuthService);
  private router = inject(Router);
  private taskService = inject(WorkerTaskService);
  private harvestService = inject(WorkerHarvestService);
  private obsService = inject(WorkerObservationService);
  private fieldService = inject(WorkerFieldService);
  private qcService = inject(WorkerQualityCheckService);
  
  isLoading = true;
  currentDate = new Date();
  userName = '';
  farmName = '';

  // --- STATS ---
  stats = {
    activeTasks: 0,
    fieldsManaged: 0,
    observationsMade: 0,
    harvestsSubmitted: 0,
    activeAlerts: 0
  };

  // --- DETAILED TRACKING ---
  tracking = {
    observations: { pending: 0, changesRequired: 0 },
    harvests: { pending: 0, changesRequired: 0 },
    qualityChecks: { pending: 0, changesRequired: 0 }
  };

  // --- ARRAYS FOR PANELS ---
  todayTasks: any[] = [];
  recentActivity: any[] = [];
  fields: any[] = [];

  // --- CHARTS CONFIG ---
  performanceChartData: ChartConfiguration<'line'>['data'] = { labels: [], datasets: [] };
  performanceChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    plugins: { legend: { display: false }, tooltip: { mode: 'index', intersect: false } },
    elements: {
      line: { tension: 0.4, borderWidth: 3 },
      point: { radius: 0, hoverRadius: 6 }
    },
    scales: {
      x: { grid: { display: false } },
      y: { grid: { color: 'rgba(0,0,0,0.05)' }, beginAtZero: true, ticks: { stepSize: 1 } }
    }
  };

  ngOnInit(): void {
    if (!this.authService.isLoggedIn()) return;
    const user = this.authService.getCurrentUser();
    if (user) {
      this.userName = user.name || 'Worker';
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
      fields: this.fieldService.getMyAssignedFields().pipe(catchError(() => of(null))),
      tasks: this.taskService.getMyTasks(filter).pipe(catchError(() => of(null))),
      harvests: this.harvestService.getMyHarvests(filter).pipe(catchError(() => of(null))),
      obs: this.obsService.getMyObservations(filter).pipe(catchError(() => of(null))),
      qcs: this.qcService.getMyQualityChecks(filter).pipe(catchError(() => of(null)))
    }).pipe(
      finalize(() => this.isLoading = false)
    ).subscribe((results: any) => {
      this.processData(results);
    });
  }

  private processData(results: any): void {
    const fieldsData = results.fields?.data || [];
    const tasks = results.tasks?.data?.items || [];
    const harvests = results.harvests?.data?.items || [];
    const obs = results.obs?.data?.items || [];
    const qcs = results.qcs?.data?.items || [];

    // Basic Counts
    this.stats.fieldsManaged = fieldsData.length;
    this.stats.activeTasks = tasks.filter((t: any) => t.status === 'PENDING' || t.status === 'IN_PROGRESS' || t.status === 'ACTIVE').length;
    this.stats.observationsMade = obs.length;
    this.stats.harvestsSubmitted = harvests.length;

    // Detailed Tracking Counts
    this.tracking.observations.pending = obs.filter((o: any) => o.validationStatus === 'pending' || o.validationStatus === 'PENDING').length;
    this.tracking.observations.changesRequired = obs.filter((o: any) => o.validationStatus === 'questioned' || o.validationStatus === 'REQUEST_CHANGES').length;
    
    this.tracking.harvests.pending = harvests.filter((h: any) => h.approvalStatus === 'PENDING').length;
    this.tracking.harvests.changesRequired = harvests.filter((h: any) => h.approvalStatus === 'REQUEST_CHANGES').length;
    
    this.tracking.qualityChecks.pending = qcs.filter((q: any) => q.approvalStatus === 'PENDING').length;
    this.tracking.qualityChecks.changesRequired = qcs.filter((q: any) => q.approvalStatus === 'REQUEST_CHANGES').length;

    // Fields Overview
    this.fields = fieldsData.slice(0, 4).map((f: any) => ({
      ...f,
      statusColor: this.getFieldStatusColor(f.status)
    }));

    // Today's Priority Tasks
    const todayStr = new Date().toDateString();
    this.todayTasks = tasks.filter((t: any) => {
      if (t.status === 'COMPLETED' || t.status === 'CANCELLED') return false;
      const due = new Date(t.dueDate);
      return due.toDateString() === todayStr || due.getTime() < new Date().getTime(); // Overdue or today
    }).sort((a: any, b: any) => new Date(a.dueDate).getTime() - new Date(b.dueDate).getTime()).slice(0, 3);

    this.buildActivityFeed(tasks, harvests, obs);
    this.buildPerformanceChart(tasks, obs);
  }

  private buildActivityFeed(tasks: any[], harvests: any[], obs: any[]): void {
    const feed: any[] = [];
    
    tasks.slice(0, 10).forEach(t => feed.push({
      type: 'Task',
      icon: 'task_alt',
      color: 'text-blue-500',
      bg: 'bg-blue-50',
      title: t.status + ' Task: ' + t.taskName,
      date: new Date(t.updatedAt || t.assignedDate)
    }));

    harvests.slice(0, 10).forEach(h => feed.push({
      type: 'Harvest',
      icon: 'agriculture',
      color: 'text-green-500',
      bg: 'bg-green-50',
      title: 'Harvest Submitted',
      desc: h.quantityKg + ' kg, Status: ' + h.approvalStatus,
      date: new Date(h.harvestDate)
    }));

    obs.slice(0, 10).forEach(o => feed.push({
      type: 'Observation',
      icon: 'visibility',
      color: 'text-purple-500',
      bg: 'bg-purple-50',
      title: 'Observation Made',
      desc: 'Health: ' + o.cropHealth,
      date: new Date(o.observationDate)
    }));

    this.recentActivity = feed.sort((a, b) => b.date.getTime() - a.date.getTime()).slice(0, 6);
  }

  private buildPerformanceChart(tasks: any[], obs: any[]): void {
    const now = new Date();
    const mapTasks = new Map<string, number>();
    const mapObs = new Map<string, number>();

    for (let i = 6; i >= 0; i--) {
      const d = new Date(now);
      d.setDate(d.getDate() - i);
      const key = d.toLocaleDateString('en-US', { weekday: 'short' });
      mapTasks.set(key, 0);
      mapObs.set(key, 0);
    }

    tasks.forEach(t => {
      if (t.status === 'COMPLETED' && t.completedDate) {
        const d = new Date(t.completedDate);
        if ((now.getTime() - d.getTime()) <= 7 * 24 * 60 * 60 * 1000) {
          const key = d.toLocaleDateString('en-US', { weekday: 'short' });
          if (mapTasks.has(key)) mapTasks.set(key, mapTasks.get(key)! + 1);
        }
      }
    });

    obs.forEach(o => {
      const d = new Date(o.observationDate);
      if ((now.getTime() - d.getTime()) <= 7 * 24 * 60 * 60 * 1000) {
        const key = d.toLocaleDateString('en-US', { weekday: 'short' });
        if (mapObs.has(key)) mapObs.set(key, mapObs.get(key)! + 1);
      }
    });

    this.performanceChartData = {
      labels: Array.from(mapTasks.keys()),
      datasets: [
        {
          label: 'Tasks Completed',
          data: Array.from(mapTasks.values()),
          borderColor: '#3b82f6',
          backgroundColor: 'rgba(59, 130, 246, 0.1)',
          fill: true,
        },
        {
          label: 'Observations',
          data: Array.from(mapObs.values()),
          borderColor: '#8b5cf6',
          backgroundColor: 'rgba(139, 92, 246, 0.1)',
          fill: true,
        }
      ]
    };
  }



  private getFieldStatusColor(status: string): string {
    switch (status) {
      case 'ACTIVE': return 'bg-emerald-100 text-emerald-800';
      case 'FALLOW': return 'bg-amber-100 text-amber-800';
      case 'PREPARATION': return 'bg-blue-100 text-blue-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  quickAction(action: string): void {
    if (action === 'addObservation') this.router.navigate(['/worker/observations']);
    if (action === 'logHarvest') this.router.navigate(['/worker/harvests']);
    if (action === 'viewTasks') this.router.navigate(['/worker/tasks']);
  }
}