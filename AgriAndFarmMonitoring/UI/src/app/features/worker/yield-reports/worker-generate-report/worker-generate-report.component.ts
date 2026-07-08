import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatIconModule } from '@angular/material/icon';
import { WorkerFieldService } from '../../services/worker-field.service';
import { WorkerYieldReportService } from '../../services/worker-yield-report.service';
import { provideNativeDateAdapter } from '@angular/material/core';

@Component({
  selector: 'app-worker-generate-report',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatIconModule,
    MatProgressSpinnerModule
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './worker-generate-report.component.html'
})
export class WorkerGenerateReportComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<WorkerGenerateReportComponent>);
  private reportService = inject(WorkerYieldReportService);
  private fieldService = inject(WorkerFieldService);

  reportForm: FormGroup;
  fields: any[] = [];
  isSubmitting = false;

  constructor() {
    this.reportForm = this.fb.group({
      reportName: ['', Validators.required],
      startDate: [null, Validators.required],
      endDate: [null, Validators.required],
      fieldId: [null, Validators.required],
      exportFormat: ['csv', Validators.required]
    });
  }

  ngOnInit(): void {
    this.loadFields();
  }

  loadFields(): void {
    this.fieldService.getMyAssignedFields().subscribe({
      next: (res: any) => this.fields = res.data || []
    });
  }

  onSubmit(): void {
    if (this.reportForm.valid) {
      this.isSubmitting = true;
      const formValue = this.reportForm.value;
      const payload = {
        ...formValue,
        startDate: formValue.startDate ? new Date(formValue.startDate).toISOString() : null,
        endDate: formValue.endDate ? new Date(formValue.endDate).toISOString() : null
      };

      this.reportService.generateReport(payload).subscribe({
        next: (res: any) => {
          this.isSubmitting = false;
          this.dialogRef.close(true);
        },
        error: (err: any) => {
          console.error('Failed to generate report', err);
          this.isSubmitting = false;
        }
      });
    }
  }

  onCancel(): void {
    this.dialogRef.close(false);
  }
}
