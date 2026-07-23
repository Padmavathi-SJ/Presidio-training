import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChatRequestDto, ChatResponseDto, ChatMessage } from '../models/chat.model';

@Injectable({
  providedIn: 'root'
})
export class ChatService {
  private apiUrl = `${environment.apiUrl}/chat`;
  private currentSessionId?: string;
  private messagesSubject = new BehaviorSubject<ChatMessage[]>([]);
  public messages$ = this.messagesSubject.asObservable();

  constructor(private http: HttpClient) {}

  public sendMessage(messageText: string, farmId: number): void {
    const currentMessages = this.messagesSubject.value;
    
    // Add user message to UI immediately
    const userMsg: ChatMessage = {
      id: Date.now().toString(),
      text: messageText,
      sender: 'user',
      timestamp: new Date()
    };
    this.messagesSubject.next([...currentMessages, userMsg]);

    const request: ChatRequestDto = {
      sessionId: this.currentSessionId,
      message: messageText,
      farmId: farmId
    };

    this.http.post<ChatResponseDto>(this.apiUrl, request).subscribe({
      next: (response) => {
        if (!this.currentSessionId) {
          this.currentSessionId = response.sessionId;
        }
        
        const botMsg: ChatMessage = {
          id: Date.now().toString() + '-bot',
          text: response.message,
          sender: 'bot',
          timestamp: new Date(response.timestamp)
        };
        
        this.messagesSubject.next([...this.messagesSubject.value, botMsg]);
      },
      error: (err) => {
        console.error('Chat error', err);
        const errorMsg: ChatMessage = {
          id: Date.now().toString() + '-error',
          text: 'Sorry, I encountered an error processing your request.',
          sender: 'bot',
          timestamp: new Date()
        };
        this.messagesSubject.next([...this.messagesSubject.value, errorMsg]);
      }
    });
  }

  public clearChat(): void {
    this.messagesSubject.next([]);
    this.currentSessionId = undefined;
  }

  public getMySessions(): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/sessions`);
  }

  public getSessionMessages(sessionId: string): Observable<any[]> {
    return this.http.get<any[]>(`${this.apiUrl}/${sessionId}/messages`);
  }
}
