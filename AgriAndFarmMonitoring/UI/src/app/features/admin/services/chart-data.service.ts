// src/app/core/services/chart-data.service.ts
import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class ChartDataService {
  constructor() {}

  // Generate color palette for charts
  getColorPalette(count: number): string[] {
    const colors = [
      '#40916c', '#d4a373', '#2d6a4f', '#c9935e', 
      '#52b788', '#f5b980', '#1b4332', '#b0784d',
      '#74c69d', '#f9d1ab', '#0f5238', '#8a5d3b'
    ];
    return colors.slice(0, count);
  }

  // Get gradient for chart
  getGradient(ctx: CanvasRenderingContext2D, color1: string, color2: string): CanvasGradient {
    const gradient = ctx.createLinearGradient(0, 0, 0, 300);
    gradient.addColorStop(0, color1);
    gradient.addColorStop(1, color2);
    return gradient;
  }

  // Generate random chart data (for demo purposes)
  generateRandomData(count: number, min: number, max: number): number[] {
    return Array.from({ length: count }, () => 
      Math.round((Math.random() * (max - min) + min) * 10) / 10
    );
  }

  // Generate labels for time-based charts
  generateTimeLabels(startHour: number = 0, count: number = 12): string[] {
    return Array.from({ length: count }, (_, i) => {
      const hour = (startHour + i * 2) % 24;
      return `${hour.toString().padStart(2, '0')}:00`;
    });
  }

  // Generate date labels
  generateDateLabels(startDate: Date, count: number): string[] {
    const labels: string[] = [];
    for (let i = 0; i < count; i++) {
      const date = new Date(startDate);
      date.setDate(date.getDate() + i);
      labels.push(date.toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
    }
    return labels;
  }
}