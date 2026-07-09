import { Injectable } from '@angular/core';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import { DatePipe } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class ReportGeneratorService {
  private datePipe = new DatePipe('en-US');

  constructor() { }

  exportToCsv(data: any[], filename: string): void {
    if (!data || !data.length) {
      console.warn('No data to export');
      return;
    }

    const headers = Object.keys(data[0]);
    const csvContent = [
      headers.join(','),
      ...data.map(row => 
        headers.map(header => {
          let cell = row[header] === null || row[header] === undefined ? '' : row[header];
          // Escape quotes and wrap in quotes if there's a comma
          cell = cell.toString().replace(/"/g, '""');
          if (cell.search(/("|,|\n)/g) >= 0) {
            cell = `"${cell}"`;
          }
          return cell;
        }).join(',')
      )
    ].join('\n');

    const blob = new Blob([csvContent], { type: 'text/csv;charset=utf-8;' });
    const link = document.createElement('a');
    if (link.download !== undefined) {
      const url = URL.createObjectURL(blob);
      link.setAttribute('href', url);
      link.setAttribute('download', `${filename}_${this.getCurrentDateString()}.csv`);
      link.style.visibility = 'hidden';
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    }
  }

  exportToPdf(data: any[], columns: { header: string, dataKey: string }[], title: string, filename: string): void {
    if (!data || !data.length) {
      console.warn('No data to export');
      return;
    }

    const doc = new jsPDF('landscape');
    
    // Add Title
    doc.setFontSize(18);
    doc.setTextColor(40);
    doc.text(title, 14, 22);

    // Add Date
    doc.setFontSize(11);
    doc.setTextColor(100);
    doc.text(`Generated on: ${this.datePipe.transform(new Date(), 'medium')}`, 14, 30);

    // Filter and format data based on provided columns
    const bodyData = data.map(row => {
      const formattedRow: any = {};
      columns.forEach(col => {
        let val = row[col.dataKey];
        // Basic date formatting heuristic (if it looks like an ISO date string)
        if (typeof val === 'string' && val.match(/^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}/)) {
           val = this.datePipe.transform(val, 'short');
        }
        formattedRow[col.dataKey] = val !== null && val !== undefined ? val : '-';
      });
      return formattedRow;
    });

    autoTable(doc, {
      head: [columns.map(c => c.header)],
      body: bodyData.map(row => columns.map(c => row[c.dataKey])),
      startY: 35,
      theme: 'grid',
      styles: {
        fontSize: 9,
        cellPadding: 3,
        overflow: 'linebreak'
      },
      headStyles: {
        fillColor: [41, 128, 185], // Professional Blue header
        textColor: 255,
        fontStyle: 'bold'
      },
      alternateRowStyles: {
        fillColor: [245, 247, 250] // Light grey alternating rows
      }
    });

    doc.save(`${filename}_${this.getCurrentDateString()}.pdf`);
  }

  private getCurrentDateString(): string {
    return this.datePipe.transform(new Date(), 'yyyyMMdd_HHmmss') || 'report';
  }
}
