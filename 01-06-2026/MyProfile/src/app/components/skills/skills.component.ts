import { Component, OnInit, AfterViewInit, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';

interface Skill {
  name: string;
  icon: string;
  level: number;
}

@Component({
  selector: 'app-skills',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './skills.component.html',
  styleUrls: ['./skills.component.css']
})
export class SkillsComponent implements OnInit, AfterViewInit {
  skillsData: Skill[] = [
    { name: "C", icon: "assets/letter-c.png", level: 85 },
    { name: "Java", icon: "assets/java.png", level: 90 },
    { name: "HTML", icon: "assets/html-5.png", level: 95 },
    { name: "CSS", icon: "assets/css-3.png", level: 95 },
    { name: "JavaScript", icon: "assets/java-script.png", level: 80 },
    { name: "React JS", icon: "assets/react.png", level: 85 },
    { name: "Node JS", icon: "assets/nodejs.png", level: 80 },
    { name: "Express JS", icon: "assets/express.png", level: 85 },
    { name: "SQL", icon: "assets/sql-server.png", level: 90 },
    { name: "MySQL", icon: "assets/mysql.png", level: 85 },
    { name: "Git", icon: "assets/git.png", level: 95 },
    { name: "GitHub", icon: "assets/github.png", level: 90 },
    { name: "Docker", icon: "assets/docker.png", level: 80 },
    { name: "AWS", icon: "assets/aws-cloud.png", level: 80 }
  ];

  isVisible: boolean = false;

  constructor(private el: ElementRef) {}

  ngOnInit() {
    // Add scroll animation observer
    this.addScrollAnimation();
  }

  ngAfterViewInit() {
    // Trigger initial animation if component is already in view
    this.checkVisibility();
  }

  private addScrollAnimation() {
    const observer = new IntersectionObserver((entries) => {
      entries.forEach(entry => {
        if (entry.isIntersecting && !this.isVisible) {
          this.isVisible = true;
          entry.target.classList.add('animate-fade-in-up');
        }
      });
    }, { threshold: 0.1 });

    const container = this.el.nativeElement.querySelector('.skills-container');
    if (container) {
      observer.observe(container);
    }
  }

  private checkVisibility() {
    const rect = this.el.nativeElement.querySelector('.skills-container')?.getBoundingClientRect();
    if (rect && rect.top < window.innerHeight) {
      this.isVisible = true;
      this.el.nativeElement.querySelector('.skills-container')?.classList.add('animate-fade-in-up');
    }
  }
}