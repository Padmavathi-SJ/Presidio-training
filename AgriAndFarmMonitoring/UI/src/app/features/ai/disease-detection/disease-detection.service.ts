import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface DiseaseAnalysisResultDto {
  id: number;
  diseaseName: string;
  category: string;
  severity: string;
  confidenceScore: number;
  symptoms: string[];
  treatment: string[];
  prevention: string[];
  organicRemedies: string[];
  additionalInfo: string;
  isResolved: boolean;
  createdAt: string;
}

export interface DiseaseDetectionRequest {
  image: File;
  farmId: number;
  fieldId: number;
  cropCycleId?: number;
  cropType?: string;
  growthStage?: string;
  additionalSymptoms?: string;
}

@Injectable({
  providedIn: 'root'
})
export class DiseaseDetectionService {
  private apiUrl = `${environment.apiUrl}/disease`;

  constructor(private http: HttpClient) {}

  detectDisease(request: DiseaseDetectionRequest): Observable<DiseaseAnalysisResultDto> {
    const formData = new FormData();
    formData.append('image', request.image);
    formData.append('farmId', request.farmId.toString());
    formData.append('fieldId', request.fieldId.toString());
    
    if (request.cropCycleId) formData.append('cropCycleId', request.cropCycleId.toString());
    if (request.cropType) formData.append('cropType', request.cropType);
    if (request.growthStage) formData.append('growthStage', request.growthStage);
    if (request.additionalSymptoms) formData.append('additionalSymptoms', request.additionalSymptoms);

    return this.http.post<DiseaseAnalysisResultDto>(`${this.apiUrl}/detect`, formData);
  }

  getHistory(farmId: number, fieldId: number): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/history?farmId=${farmId}&fieldId=${fieldId}`);
  }

  getAnalysisById(id: number): Observable<DiseaseAnalysisResultDto> {
    return this.http.get<DiseaseAnalysisResultDto>(`${this.apiUrl}/${id}`);
  }

  askWithDiseaseContext(analysisId: number, question: string): Observable<{ answer: string }> {
    return this.http.post<{ answer: string }>(`${this.apiUrl}/chat-with-context`, {
      analysisId,
      question
    });
  }
}
