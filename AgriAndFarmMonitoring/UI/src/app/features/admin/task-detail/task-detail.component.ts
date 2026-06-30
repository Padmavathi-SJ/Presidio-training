// src/app/features/admin/task-detail/task-detail.component.ts
import { Component, inject } from '@angular/core';  // ✅ Remove Inject import
import { CommonModule } from '@angular/common';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { Task, STATUS_COLORS, PRIORITY_COLORS } from '../models/task.model';

@Component({
  selector: 'app-task-detail',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule
  ],
  templateUrl: './task-detail.component.html'
})
export class TaskDetailComponent {
  private dialogRef = inject(MatDialogRef<TaskDetailComponent>);
  
  // ✅ Use inject() instead of @Inject decorator
  data = inject<{ task: Task }>(MAT_DIALOG_DATA);

  getStatusColor(status: string): string {
    return STATUS_COLORS[status] || 'bg-gray-100 text-gray-700';
  }

  getPriorityColor(priority: string): string {
    return PRIORITY_COLORS[priority] || 'bg-gray-100 text-gray-700';
  }

  onClose(): void {
    this.dialogRef.close();
  }
}