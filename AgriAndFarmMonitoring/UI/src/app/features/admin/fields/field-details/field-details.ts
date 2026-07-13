import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatDividerModule } from '@angular/material/divider';
import { Field } from '../../models/field.model';

@Component({
  selector: 'app-field-details',
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
  field: Field;

  constructor(
    public dialogRef: MatDialogRef<FieldDetailsComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { field: Field }
  ) {
    this.field = data.field;
  }

  close(): void {
    this.dialogRef.close();
  }
}
