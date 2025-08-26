import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    redirectTo: '/courses',
    pathMatch: 'full'
  },
  {
    path: 'courses',
    loadComponent: () => import('./features/courses/courses.component').then(m => m.CoursesComponent)
  },
  {
    path: 'enrollments',
    loadComponent: () => import('./features/enrollments/enrollments.component').then(m => m.EnrollmentsComponent)
  },
  {
    path: '**',
    redirectTo: '/courses'
  }
];
