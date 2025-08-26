import { Component } from '@angular/core';
import { RouterOutlet, RouterLink } from '@angular/router';
import { MenubarModule } from 'primeng/menubar';
import { MenuItem } from 'primeng/api';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, MenubarModule],
  template: `
    <div class="app-container">
      <p-menubar [model]="menuItems" styleClass="border-none shadow-1">
        <ng-template pTemplate="start">
          <img src="assets/logo.svg" height="40" class="mr-2" alt="Mini-LMS">
          <span class="text-2xl font-bold text-primary">Mini-LMS Lite</span>
        </ng-template>
      </p-menubar>
      
      <main class="container content">
        <router-outlet></router-outlet>
      </main>
    </div>
  `,
  styles: [`
    .app-container {
      min-height: 100vh;
      background-color: #f8fafc;
    }
    
    :host ::ng-deep .p-menubar {
      background: white;
      border: none;
      border-bottom: 1px solid #e2e8f0;
    }
    
    :host ::ng-deep .p-menubar .p-menubar-root-list > .p-menuitem > .p-menuitem-link {
      padding: 1rem 1.5rem;
    }
  `]
})
export class AppComponent {
  menuItems: MenuItem[] = [
    {
      label: 'Courses',
      icon: 'pi pi-book',
      routerLink: '/courses'
    },
    {
      label: 'Bulk Enrollment',
      icon: 'pi pi-upload',
      routerLink: '/enrollments'
    }
  ];
}
