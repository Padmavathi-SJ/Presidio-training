import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SidebarComponent } from '../components/sidebar/sidebar.component';
import { TopBarComponent } from '../components/topbar/topbar';
import { HomeBannerComponent } from '../components/home-banner/home-banner';
import { ProfileComponent } from '../profile/profile';
import { SkillsComponent } from '../skills/skills';
import { ProjectsComponent } from '../projects/projects';
import { ContactComponent } from '../contact/contact';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    CommonModule,
    SidebarComponent,
    TopBarComponent,
    HomeBannerComponent,
    ProfileComponent,
    SkillsComponent,
    ProjectsComponent,
    ContactComponent
  ],
  templateUrl: './home.html',
  styleUrls: ['./home.css']
})
export class HomeComponent {
   constructor() {
    console.log('HomeComponent loaded and rendered!');
  }
}