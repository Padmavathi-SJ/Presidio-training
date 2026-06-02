import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HttpClient, HttpClientModule } from '@angular/common/http';
import { animate, style, transition, trigger } from '@angular/animations';

interface ContactForm {
  name: string;
  email: string;
  message: string;
}

@Component({
  selector: 'app-contact',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.css'],
  animations: [
    trigger('fadeIn', [
      transition(':enter', [
        style({ opacity: 0 }),
        animate('0.8s ease-out', style({ opacity: 1 }))
      ])
    ]),
    trigger('slideIn', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateY(30px)' }),
        animate('0.6s ease-out', style({ opacity: 1, transform: 'translateY(0)' }))
      ])
    ])
  ]
})
export class ContactComponent {
  formData: ContactForm = {
    name: '',
    email: '',
    message: ''
  };
  
  isSubmitting: boolean = false;
  submitStatus: 'success' | 'error' | null = null;
  submitMessage: string = '';

  constructor(private http: HttpClient) {}

  onSubmit(): void {
    if (!this.isFormValid()) {
      return;
    }

    this.isSubmitting = true;
    this.submitStatus = null;
    
    // Updated API endpoint - change this to your actual backend URL
    const apiUrl = 'http://localhost:5000/api/send-email';
    
    this.http.post(apiUrl, this.formData).subscribe({
      next: (response: any) => {
        if (response.success) {
          this.submitStatus = 'success';
          this.submitMessage = 'Message sent successfully! We\'ll get back to you soon.';
          this.resetForm();
        } else {
          this.submitStatus = 'error';
          this.submitMessage = 'Failed to send message. Please try again later.';
        }
        this.isSubmitting = false;
        
        // Auto-hide message after 5 seconds
        setTimeout(() => {
          this.submitStatus = null;
        }, 5000);
      },
      error: (error) => {
        console.error('Error sending message:', error);
        this.submitStatus = 'error';
        this.submitMessage = 'Failed to send message. Please check your connection and try again.';
        this.isSubmitting = false;
        
        // Auto-hide message after 5 seconds
        setTimeout(() => {
          this.submitStatus = null;
        }, 5000);
      }
    });
  }

  private isFormValid(): boolean {
    return this.formData.name.trim() !== '' && 
           this.formData.email.trim() !== '' && 
           this.formData.message.trim() !== '' &&
           this.isValidEmail(this.formData.email);
  }

  private isValidEmail(email: string): boolean {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
  }

  private resetForm(): void {
    this.formData = {
      name: '',
      email: '',
      message: ''
    };
  }
}