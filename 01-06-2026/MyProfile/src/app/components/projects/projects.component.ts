import { Component, OnInit, HostListener } from '@angular/core';
import { CommonModule } from '@angular/common';

interface GalleryImage {
  url: string;
}

interface Project {
  name: string;
  description: string;
  image: string;
  gallery: string[];
  repoLink: string;
  role: string;
}

@Component({
  selector: 'app-projects',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './projects.component.html',
  styleUrls: ['./projects.component.css']
})
export class ProjectsComponent {
  projects: Project[] = [
    {
      name: "Interns Tracking Portal",
      description: "Interns Tracking Portal streamlines intern management for industries and institutions with task allocation, team collaboration, leave tracking, and feedback management. Users get a personalized dashboard to monitor work, update tasks, and manage personal details efficiently.",
      image: "assets/ems.jpg",
      gallery: [
        "assets/EMS/ems1.png",
        "assets/EMS/ems2.png",
        "assets/EMS/ems3.png",
        "assets/EMS/ems4.png",
        "assets/EMS/ems5.png",
        "assets/EMS/ems6.png",
        "assets/EMS/ems7.png",
        "assets/EMS/ems8.png"
      ],
      repoLink: "https://github.com/Padmavathi-SJ/Interns-Tracking-System",
      role: "Worked as FullStack Developer (React JS, Node JS, Express JS, MySQL)"
    },
    {
      name: "Online Coding Space",
      description: "Online Coding Space is a platform for academic institutions, enabling students to practice faculty-assigned problems regularly. It features an integrated compiler for popular languages and allows admins to track and evaluate student submissions efficiently.",
      image: "assets/compiler.jpg",
      gallery: [
        "assets/compiler/compiler1.png",
        "assets/compiler/compiler2.png",
        "assets/compiler/compiler3.png",
        "assets/compiler/compiler4.png",
        "assets/compiler/compiler5.png",
        "assets/compiler/compiler6.png",
        "assets/compiler/compiler7.png"
      ],
      repoLink: "https://github.com/Padmavathi-SJ/Online_Code_Space",
      role: "Worked as Frontend Developer (React JS)"
    },
    {
      name: "Campus Chronicles",
      description: "College Events & Clubs Gallery is a digital space for students to upload and cherish their event memories. Users can share images, videos, and quotes, organizing them into personal folders to preserve unforgettable moments for the future.",
      image: "assets/ncc.jpg",
      gallery: [
        "assets/ncc/ncc1.png",
        "assets/ncc/ncc2.png",
        "assets/ncc/ncc3.png",
        "assets/ncc/ncc4.png",
        "assets/ncc/ncc5.png",
        "assets/ncc/ncc6.png"
      ],
      repoLink: "https://github.com/Padmavathi-SJ/NCC_Memoria",
      role: "Worked as FullStack Developer (React JS, Node JS, Express JS, MySQL)"
    },
    {
      name: "Smart E-Cart Application - Ongoing",
      description: "Smart E-Cart Application empowers small sellers to effortlessly manage and sell their products online. With features like product listing, inventory control, customer orders, and wishlists, it streamlines the entire selling process. Sellers can track orders, update stock, and manage their store from a centralized dashboard, while customers enjoy a smooth and personalized shopping experience.",
      image: "assets/e-cart.jpg",
      gallery: [
        "assets/e-cart/ecart-1.png",
        "assets/e-cart/ecart-2.png",
        "assets/e-cart/ecart-3.png",
        "assets/e-cart/ecart-4.png"
      ],
      repoLink: "https://github.com/Padmavathi-SJ/E-Cart-Application-",
      role: "Working as FullStack Developer (React JS, Node JS, Express JS, MySQL)"
    }
  ];

  selectedProject: Project | null = null;
  currentImageIndex: number = 0;
  zoomedImage: string | null = null;

  handleNext(): void {
    if (this.selectedProject) {
      this.currentImageIndex = 
        this.currentImageIndex === this.selectedProject.gallery.length - 1 
          ? 0 
          : this.currentImageIndex + 1;
    }
  }

  handlePrev(): void {
    if (this.selectedProject) {
      this.currentImageIndex = 
        this.currentImageIndex === 0 
          ? this.selectedProject.gallery.length - 1 
          : this.currentImageIndex - 1;
    }
  }

  openProjectModal(project: Project): void {
    this.selectedProject = project;
    this.currentImageIndex = 0;
    document.body.style.overflow = 'hidden';
  }

  closeProjectModal(): void {
    this.selectedProject = null;
    document.body.style.overflow = 'auto';
  }

  openZoomedImage(image: string): void {
    this.zoomedImage = image;
    document.body.style.overflow = 'hidden';
  }

  closeZoomedImage(): void {
    this.zoomedImage = null;
    document.body.style.overflow = 'auto';
  }

  @HostListener('document:keydown', ['$event'])
  handleKeyboardEvent(event: KeyboardEvent): void {
    if (this.selectedProject) {
      if (event.key === 'ArrowLeft') {
        this.handlePrev();
      } else if (event.key === 'ArrowRight') {
        this.handleNext();
      } else if (event.key === 'Escape') {
        this.closeProjectModal();
      }
    }
    
    if (this.zoomedImage && event.key === 'Escape') {
      this.closeZoomedImage();
    }
  }
}