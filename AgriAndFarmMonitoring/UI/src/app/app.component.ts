import { Component } from '@angular/core';
import { RouterOutlet } from '@angular/router';

import { ChatbotComponent } from './shared/components/chatbot/chatbot';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, ChatbotComponent],
  template: `
    <router-outlet></router-outlet>
    <app-chatbot></app-chatbot>
  `,
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'Farm Management Platform';
}