import { Routes } from '@angular/router';
import { HomeComponent } from './home/home';
import { ProfileComponent } from './profile/profile';
import { SkillsComponent } from './skills/skills';
import { ProjectsComponent } from './projects/projects';
import { ContactComponent } from './contact/contact';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'profile', component: ProfileComponent },
  { path: 'skills', component: SkillsComponent },
  { path: 'projects', component: ProjectsComponent },
  { path: 'contact', component: ContactComponent },
  { path: '**', redirectTo: '' }
];