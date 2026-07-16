import { Component, OnInit, ElementRef, ViewChild, AfterViewChecked } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ChatService } from '../../../core/services/chat.service';
import { ChatMessage } from '../../../core/models/chat.model';
import { marked } from 'marked';

@Component({
  selector: 'app-chatbot',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chatbot.html',
  styleUrls: ['./chatbot.scss']
})
export class ChatbotComponent implements OnInit, AfterViewChecked {
  isOpen = false;
  messages: ChatMessage[] = [];
  newMessage = '';
  // Hardcoded for demo/testing, should be from auth service
  farmId = 1; 
  
  @ViewChild('scrollMe') private myScrollContainer!: ElementRef;

  constructor(private chatService: ChatService) {
    marked.setOptions({
      breaks: true,
      gfm: true
    });
  }

  ngOnInit(): void {
    this.chatService.messages$.subscribe(async messages => {
      const parsedMessages = [];
      for (const msg of messages) {
        parsedMessages.push({
          ...msg,
          html: await marked.parse(msg.text)
        });
      }
      this.messages = parsedMessages;
    });
  }

  ngAfterViewChecked() {        
    this.scrollToBottom();        
  } 

  scrollToBottom(): void {
    try {
      if (this.myScrollContainer) {
        this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
      }
    } catch(err) { }                 
  }

  toggleChat(): void {
    this.isOpen = !this.isOpen;
  }

  sendMessage(): void {
    if (!this.newMessage.trim()) return;
    
    this.chatService.sendMessage(this.newMessage, this.farmId);
    this.newMessage = '';
  }
}
