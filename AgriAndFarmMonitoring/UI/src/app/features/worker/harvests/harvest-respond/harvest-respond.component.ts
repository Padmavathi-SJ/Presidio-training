import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatSnackBar } from '@angular/material/snack-bar';
import { HarvestStateService } from '../../services/harvest-state.service';

@Component({
  selector: 'app-harvest-respond',
  standalone: true,
  imports: [
    CommonModule, FormsModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule,
    MatIconModule, MatButtonModule
  ],
  templateUrl: './harvest-respond.component.html'
})
export class HarvestRespondComponent implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<HarvestRespondComponent>);
  private data = inject(MAT_DIALOG_DATA);
  private harvestState = inject(HarvestStateService);
  private snackBar = inject(MatSnackBar);

  respondForm!: FormGroup;
  isSubmitting = this.harvestState.isSubmitting;
  harvestId: number = this.data.harvest.id;

  ngOnInit() {
    this.respondForm = this.fb.group({
      responseNotes: ['', [Validators.required, Validators.minLength(5)]]
    });
  }

  closeDialog() {
    this.dialogRef.close();
  }

  save() {
    if (this.respondForm.invalid) return;

    this.harvestState.respondToAdmin(this.harvestId, this.respondForm.value).subscribe({
      next: (res) => {
        if (res.success) {
          this.snackBar.open('Response submitted successfully', 'Close', { duration: 3000 });
          this.dialogRef.close({ saved: true });
        } else {
          this.snackBar.open(res.message || 'Failed to submit response', 'Close', { duration: 3000 });
        }
      },
      error: (err) => {
        this.snackBar.open(err.error?.message || 'Failed to submit response', 'Close', { duration: 3000 });
      }
    });
  }
}
