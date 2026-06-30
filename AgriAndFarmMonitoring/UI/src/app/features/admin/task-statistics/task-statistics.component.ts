// src/app/features/admin/task-statistics/task-statistics.component.ts
import { Component, Input, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { TaskStatisticsDto } from '../models/task.model';

@Component({
  selector: 'app-task-statistics',
  standalone: true,
  imports: [
    CommonModule,
    MatCardModule,
    MatIconModule,
    MatProgressBarModule
  ],
  templateUrl: './task-statistics.component.html'
})
export class TaskStatisticsComponent {
  @Input() statistics!: TaskStatisticsDto;

  completionRate = computed(() => {
    if (!this.statistics || this.statistics.totalTasks === 0) return 0;
    return Math.round((this.statistics.completedTasks / this.statistics.totalTasks) * 100);
  });

  getPriorityColor(priority: string): string {
    const colors: Record<string, string> = {
      'LOW': 'bg-gray-500',
      'MEDIUM': 'bg-blue-500',
      'HIGH': 'bg-orange-500',
      'URGENT': 'bg-red-500'
    };
    return colors[priority] || 'bg-gray-500';
  }

  getStatusCount(status: string): number {
    if (!this.statistics) return 0;
    const map: Record<string, number> = {
      'PENDING': this.statistics.pendingTasks,
      'IN_PROGRESS': this.statistics.inProgressTasks,
      'COMPLETED': this.statistics.completedTasks,
      'OVERDUE': this.statistics.overdueTasks,
      'CANCELLED': this.statistics.cancelledTasks
    };
    return map[status] || 0;
  }
}