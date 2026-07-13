import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { WorkerFieldDetail } from '../../models/worker-field.model';

@Component({
  selector: 'app-worker-field-details',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatButtonModule,
    MatIconModule,
    MatChipsModule,
    MatDividerModule
  ],
  templateUrl: './field-details.html'
})
export class FieldDetailsComponent {
  field: WorkerFieldDetail;

  constructor(
    public dialogRef: MatDialogRef<FieldDetailsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { field: WorkerFieldDetail }
  ) {
    this.field = data.field;
  }

  close(): void {
    this.dialogRef.close();
  }
}
