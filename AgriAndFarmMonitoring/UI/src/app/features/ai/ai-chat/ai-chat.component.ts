import { Component, OnInit, ViewChild, ElementRef, AfterViewChecked, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatCardModule } from '@angular/material/card';
import { ChatService } from '../../../core/services/chat.service';
import { AuthService } from '../../../core/services/auth.service';
import { ChatMessage } from '../../../core/models/chat.model';
import { Subscription } from 'rxjs';
import { marked } from 'marked';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatButtonModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatCardModule
  ],
  templateUrl: './ai-chat.component.html',
  styleUrls: ['./ai-chat.component.scss']
})
export class AiChatComponent implements OnInit, AfterViewChecked, OnDestroy {
  @ViewChild('chatScroll') private chatScroll!: ElementRef;
  
  messages: { role: 'user' | 'assistant', text: string, html: string }[] = [];
  sessions: any[] = [];
  inputText = '';
  isTyping = false;
  farmId = 1;
  private msgSub!: Subscription;

  constructor(
    private chatService: ChatService,
    private authService: AuthService
  ) {
    marked.setOptions({ breaks: true, gfm: true });
  }

  ngOnInit(): void {
    const user = this.authService.getCurrentUser();
    if (user?.farmId) {
      this.farmId = user.farmId;
    }

    this.loadSessions();

    this.msgSub = this.chatService.messages$.subscribe(async msgs => {
      // the ChatService updates current messages
      if (msgs.length === 0) {
        this.messages = [];
        return;
      }
      this.messages = [];
      for (let m of msgs) {
        this.messages.push({
          role: m.sender === 'user' ? 'user' : 'assistant',
          text: m.text,
          html: await marked.parse(m.text)
        });
      }
      this.isTyping = false;
    });
  }

  ngAfterViewChecked() {
    this.scrollToBottom();
  }

  ngOnDestroy() {
    if (this.msgSub) this.msgSub.unsubscribe();
    this.chatService.clearChat();
  }

  loadSessions() {
    this.chatService.getMySessions().subscribe({
      next: (data) => {
        this.sessions = data;
      },
      error: (err) => console.error('Failed to load sessions', err)
    });
  }

  loadSession(sessionId: string) {
    this.chatService.clearChat(); // clear current state
    this.chatService.getSessionMessages(sessionId).subscribe({
      next: async (msgs) => {
        this.messages = [];
        for (let m of msgs) {
          this.messages.push({
            role: m.role,
            text: m.text,
            html: await marked.parse(m.text)
          });
        }
      },
      error: (err) => console.error('Failed to load messages', err)
    });
  }

  startNewChat() {
    this.chatService.clearChat();
    this.messages = [];
  }

  sendMessage() {
    if (!this.inputText.trim()) return;
    this.isTyping = true;
    this.chatService.sendMessage(this.inputText, this.farmId);
    this.inputText = '';
    // reload sessions shortly after so new session appears
    setTimeout(() => this.loadSessions(), 1000);
  }

  private scrollToBottom(): void {
    try {
      if (this.chatScroll) {
        this.chatScroll.nativeElement.scrollTop = this.chatScroll.nativeElement.scrollHeight;
      }
    } catch(err) { }
  }
}
