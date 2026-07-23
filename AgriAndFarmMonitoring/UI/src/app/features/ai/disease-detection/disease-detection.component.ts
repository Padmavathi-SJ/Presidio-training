import { Component, ElementRef, ViewChild, OnInit, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatChipsModule } from '@angular/material/chips';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCardModule } from '@angular/material/card';
import { MatDividerModule } from '@angular/material/divider';
import { DiseaseDetectionService, DiseaseAnalysisResultDto } from './disease-detection.service';
import { AuthService } from '../../../core/services/auth.service';
import { marked } from 'marked';

@Component({
  selector: 'app-disease-detection',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule, 
    FormsModule,
    MatButtonModule, 
    MatIconModule, 
    MatProgressSpinnerModule,
    MatChipsModule,
    MatFormFieldModule,
    MatInputModule,
    MatCardModule,
    MatDividerModule
  ],
  templateUrl: './disease-detection.component.html',
  styleUrls: ['./disease-detection.component.scss']
})
export class DiseaseDetectionComponent implements OnInit, AfterViewChecked {
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  @ViewChild('chatContainer') private chatContainer!: ElementRef;
  
  isDragging = false;
  selectedFile: File | null = null;
  imagePreview: string | null = null;
  isAnalyzing = false;
  
  // Hardcoded for demo - usually you'd get this from context or a form dropdown
  farmId = 1;
  fieldId = 1;
  cropType = 'Wheat';

  result: DiseaseAnalysisResultDto | null = null;
  
  followUpQuestion = '';
  isAsking = false;
  imageError = false;
  chatHistory: { role: 'user' | 'assistant', text: string, html: string }[] = [];
  scanHistory: any[] = [];

  constructor(
    private diseaseService: DiseaseDetectionService,
    private authService: AuthService
  ) {
    marked.setOptions({
      breaks: true,
      gfm: true
    });
  }

  ngOnInit() {
    const user = this.authService.getCurrentUser();
    if (user?.farmId) {
      this.farmId = user.farmId;
    }
    this.loadHistory();
  }

  loadHistory() {
    this.diseaseService.getMyHistory().subscribe({
      next: (data) => {
        this.scanHistory = data;
      },
      error: (err) => {
        console.error('Failed to load history', err);
      }
    });
  }

  async loadAnalysis(id: number) {
    this.diseaseService.getAnalysisById(id).subscribe({
      next: async (res) => {
        this.result = res;
        this.selectedFile = null;
        this.imagePreview = null;
        
        // Load chat history
        this.diseaseService.getChatHistory(id).subscribe({
          next: async (chats: any[]) => {
            this.chatHistory = [];
            for (let chat of chats) {
              this.chatHistory.push({
                role: 'user',
                text: chat.query,
                html: await marked.parse(chat.query)
              });
              this.chatHistory.push({
                role: 'assistant',
                text: chat.response,
                html: await marked.parse(chat.response)
              });
            }
          }
        });
      }
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  private scrollToBottom(): void {
    try {
      if (this.chatContainer) {
        this.chatContainer.nativeElement.scrollTop = this.chatContainer.nativeElement.scrollHeight;
      }
    } catch(err) { }
  }

  onDragOver(event: DragEvent) {
    event.preventDefault();
    this.isDragging = true;
  }

  onDragLeave(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
  }

  onDrop(event: DragEvent) {
    event.preventDefault();
    this.isDragging = false;
    
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.handleFile(event.dataTransfer.files[0]);
    }
  }

  onFileSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      this.handleFile(input.files[0]);
    }
  }

  triggerFileInput() {
    this.fileInput.nativeElement.click();
  }

  private handleFile(file: File) {
    if (!file.type.startsWith('image/')) {
      alert('Please select an image file.');
      return;
    }

    this.selectedFile = file;
    this.result = null; // Clear previous results
    this.chatHistory = []; // Clear chat history
    
    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
      this.imagePreview = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  analyzeImage() {
    if (!this.selectedFile) return;

    this.isAnalyzing = true;
    
    this.diseaseService.detectDisease({
      image: this.selectedFile,
      farmId: this.farmId,
      fieldId: this.fieldId,
      cropType: this.cropType,
      additionalSymptoms: 'Please analyze accurately.'
    }).subscribe({
      next: async (res) => {
        this.result = res;
        this.isAnalyzing = false;
        
        // Add initial context to chat
        const initialText = `I've analyzed the image and detected **${res.diseaseName}**. How can I help you further with this?`;
        this.chatHistory.push({
          role: 'assistant',
          text: initialText,
          html: await marked.parse(initialText)
        });
      },
      error: (err) => {
        console.error('Analysis failed', err);
        alert('Analysis failed. Please try again.');
        this.isAnalyzing = false;
      }
    });
  }

  clearSelection() {
    this.selectedFile = null;
    this.imagePreview = null;
    this.result = null;
    this.chatHistory = [];
    if (this.fileInput) {
      this.fileInput.nativeElement.value = '';
    }
  }

  getSeverityColor(severity: string): string {
    switch(severity.toLowerCase()) {
      case 'high': return 'warn';
      case 'medium': return 'accent';
      case 'low': return 'primary';
      case 'none': return 'primary';
      default: return 'primary';
    }
  }
  
  getSeverityBadgeClass(severity: string): string {
    switch(severity.toLowerCase()) {
      case 'high': return 'bg-red-100 text-red-800';
      case 'medium': return 'bg-orange-100 text-orange-800';
      case 'low': return 'bg-yellow-100 text-yellow-800';
      case 'none': return 'bg-green-100 text-green-800';
      default: return 'bg-gray-100 text-gray-800';
    }
  }

  async sendFollowUpQuestion() {
    if (!this.followUpQuestion.trim() || !this.result) return;

    const question = this.followUpQuestion;
    this.chatHistory.push({ 
      role: 'user', 
      text: question,
      html: await marked.parse(question)
    });
    this.followUpQuestion = '';
    this.isAsking = true;

    this.diseaseService.askWithDiseaseContext(this.result.id, question)
      .subscribe({
        next: async (res) => {
          this.chatHistory.push({ 
            role: 'assistant', 
            text: res.answer,
            html: await marked.parse(res.answer)
          });
          this.isAsking = false;
        },
        error: async (err) => {
          console.error('Chat failed', err);
          const errorMsg = 'Sorry, I encountered an error answering your question.';
          this.chatHistory.push({ 
            role: 'assistant', 
            text: errorMsg,
            html: await marked.parse(errorMsg)
          });
          this.isAsking = false;
        }
      });
  }
}
