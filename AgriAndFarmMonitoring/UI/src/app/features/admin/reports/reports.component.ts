import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ReportGeneratorService } from '../../../core/services/report-generator.service';
import { TokenService } from '../../../core/services/token.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-admin-reports',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule, MatCardModule, MatFormFieldModule,
    MatSelectModule, MatInputModule, MatDatepickerModule, MatNativeDateModule,
    MatButtonModule, MatIconModule, MatTableModule, MatProgressSpinnerModule,
    DatePipe
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
    { value: 'fields', label: 'Fields & Crop Cycles', url: 'fields', admin: false },
    { value: 'workers', label: 'Workers Data', url: 'workers', admin: true },
    { value: 'tasks', label: 'Tasks', url: 'tasks', admin: true },
    { value: 'observations', label: 'Observations', url: 'observations', admin: true },
    { value: 'harvests', label: 'Harvests', url: 'harvests', admin: true },
    { value: 'quality-checks', label: 'Quality Checks', url: 'quality-checks', admin: true },
    { value: 'sensors', label: 'Sensor Readings', url: 'sensors', admin: true },
    { value: 'weather', label: 'Weather Data', url: 'weather', admin: true }
  ];

  statusOptions = ['PENDING', 'APPROVED', 'REJECTED', 'REQUEST_CHANGES', 'IN_PROGRESS', 'COMPLETED'];
  
  data = signal<any[]>([]);
  columns = signal<{header: string, dataKey: string}[]>([]);
  displayedColumns = signal<string[]>([]);
  isLoading = signal<boolean>(false);
  hasSearched = signal<boolean>(false);
  
  farmId = 1;

  ngOnInit() {
    this.farmId = this.tokenService.getUser()?.farmId || 1;
    this.filterForm = this.fb.group({
      reportType: ['tasks'],
      startDate: [null],
      endDate: [null],
      status: [null]
    });
  }

  onPreview() {
    const filters = this.filterForm.value;
    const reportTypeObj = this.reportTypes.find(r => r.value === filters.reportType);
    if (!reportTypeObj) return;

    this.isLoading.set(true);
    this.hasSearched.set(true);
    
    // Construct URL
    const prefix = reportTypeObj.admin ? 'api/admin/farms' : 'api/farms';
    const baseUrl = `${environment.apiUrl.replace('/api', '')}/api/${reportTypeObj.admin ? 'admin/' : ''}farms/${this.farmId}/${reportTypeObj.url}`;

    // Construct Query Params
    const params: any = {};
    if (filters.status) params.status = filters.status;
    if (filters.startDate) params.startDate = filters.startDate.toISOString();
    if (filters.endDate) params.endDate = filters.endDate.toISOString();

    this.http.get<any>(baseUrl, { params }).subscribe({
      next: (res) => {
        // Some APIs return data array directly, some wrapped in { success: true, data: [] }
        const rawData = Array.isArray(res) ? res : (res.data || []);
        
        // Setup columns dynamically based on the first item
        if (rawData.length > 0) {
          const keys = Object.keys(rawData[0]).filter(k => 
             // filter out complex objects or IDs that are not useful
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
    this.reportService.exportToPdf(this.data(), this.columns(), `${typeLabel} Report`, this.filterForm.value.reportType);
  }

  exportCsv() {
    if (this.data().length === 0) return;
    this.reportService.exportToCsv(this.data(), this.filterForm.value.reportType);
  }
}
