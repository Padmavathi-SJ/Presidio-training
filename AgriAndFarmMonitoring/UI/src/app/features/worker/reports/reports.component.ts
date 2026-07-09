import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReportGeneratorService } from '../../../core/services/report-generator.service';
import { TokenService } from '../../../core/services/token.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-worker-reports',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule,
    MatSelectModule, MatInputModule, MatButtonModule, MatIconModule, 
    MatTableModule, MatProgressSpinnerModule, DatePipe
  ],
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  private fb = inject(FormBuilder);
  private http = inject(HttpClient);
  private reportService = inject(ReportGeneratorService);
  private tokenService = inject(TokenService);

  filterForm!: FormGroup;
  reportTypes = [
    { value: 'sensors', label: 'Sensor Data', url: 'sensors' },
    { value: 'weather', label: 'Weather Data', url: 'weather' }
  ];

  data = signal<any[]>([]);
  columns = signal<{header: string, dataKey: string}[]>([]);
  displayedColumns = signal<string[]>([]);
  isLoading = signal<boolean>(false);
  hasSearched = signal<boolean>(false);
  
  farmId = 1;
  startDate!: Date;
  endDate!: Date;

  ngOnInit() {
    this.farmId = this.tokenService.getUser()?.farmId || 1;
    
    // Lock dates to last 7 days
    this.endDate = new Date();
    this.startDate = new Date();
    this.startDate.setDate(this.endDate.getDate() - 7);

    this.filterForm = this.fb.group({
      reportType: ['sensors']
    });
  }

  onPreview() {
    const filters = this.filterForm.value;
    const reportTypeObj = this.reportTypes.find(r => r.value === filters.reportType);
    if (!reportTypeObj) return;

    this.isLoading.set(true);
    this.hasSearched.set(true);
    
    // Worker routes
    // Worker APIs for sensors are usually GET /api/farms/{farmId}/worker-fields/sensors 
    // or GET /api/worker/farms/{farmId}/sensors
    // Wait, the API routes for worker sensor data might just be standard sensor fetch with auth filtering, 
    // or specific worker endpoints. 
    // Let's use the standard `api/worker/farms/{farmId}/{url}` since that's typical for this platform.
    const baseUrl = `${environment.apiUrl.replace('/api', '')}/api/worker/farms/${this.farmId}/${reportTypeObj.url}`;

    // Apply strict 7-day filters
    const params: any = {
      startDate: this.startDate.toISOString(),
      endDate: this.endDate.toISOString()
    };

    this.http.get<any>(baseUrl, { params }).subscribe({
      next: (res) => {
        const rawData = Array.isArray(res) ? res : (res.data || []);
        
        if (rawData.length > 0) {
          const keys = Object.keys(rawData[0]).filter(k => 
             typeof rawData[0][k] !== 'object' && !k.toLowerCase().endsWith('id')
          );
          
          const colDefs = keys.map(k => ({
            header: k.charAt(0).toUpperCase() + k.slice(1).replace(/([A-Z])/g, ' $1').trim(),
            dataKey: k
          }));
          
          this.columns.set(colDefs);
          this.displayedColumns.set(colDefs.map(c => c.dataKey));
        } else {
          this.columns.set([]);
          this.displayedColumns.set([]);
        }

        this.data.set(rawData);
        this.isLoading.set(false);
      },
      error: (err) => {
        console.error('Error fetching report data', err);
        this.data.set([]);
        this.isLoading.set(false);
      }
    });
  }

  exportPdf() {
    if (this.data().length === 0) return;
    const typeLabel = this.reportTypes.find(r => r.value === this.filterForm.value.reportType)?.label || 'Report';
    this.reportService.exportToPdf(this.data(), this.columns(), `Worker ${typeLabel} Report (Last 7 Days)`, this.filterForm.value.reportType);
  }

  exportCsv() {
    if (this.data().length === 0) return;
    this.reportService.exportToCsv(this.data(), `Worker_${this.filterForm.value.reportType}`);
  }
}
