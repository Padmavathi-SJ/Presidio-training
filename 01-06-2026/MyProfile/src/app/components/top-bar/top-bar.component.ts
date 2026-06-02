import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-top-bar',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './top-bar.component.html',
  styleUrls: ['./top-bar.component.css']
})
export class TopBarComponent {
  // Social media links
  linkedinUrl: string = 'https://www.linkedin.com/in/padmavathisj/';
  githubUrl: string = 'https://github.com/Padmavathi-SJ';
  telegramUrl: string = 'https://t.me/your-telegram';
  emailAddress: string = 'padmavathisj2005@gmail.com';
}