import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { TableModule } from 'primeng/table';
import { ButtonModule } from 'primeng/button';
import { DialogModule } from 'primeng/dialog';
import { InputTextModule } from 'primeng/inputtext';
import { CheckboxModule } from 'primeng/checkbox';
import { ToastModule } from 'primeng/toast';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { MessageService, ConfirmationService } from 'primeng/api';
import { Course, CreateCourseDto, UpdateCourseDto } from '../../core/models/course.model';
import { CourseService } from '../../core/services/course.service';

@Component({
  selector: 'app-courses',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    TableModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    CheckboxModule,
    ToastModule,
    ConfirmDialogModule
  ],
  providers: [MessageService, ConfirmationService],
  template: `
    <div class="page-header">
      <h1>Course Management</h1>
      <p-button 
        label="Add Course" 
        icon="pi pi-plus" 
        (onClick)="openNewCourseDialog()"
        styleClass="p-button-primary">
      </p-button>
    </div>

    <div class="card">
      <p-table 
        [value]="courses" 
        [loading]="loading"
        [paginator]="true" 
        [rows]="10"
        [showCurrentPageReport]="true"
        currentPageReportTemplate="Showing {first} to {last} of {totalRecords} courses"
        responsiveLayout="scroll">
        
        <ng-template pTemplate="header">
          <tr>
            <th pSortableColumn="title">
              Title <p-sortIcon field="title"></p-sortIcon>
            </th>
            <th pSortableColumn="category">
              Category <p-sortIcon field="category"></p-sortIcon>
            </th>
            <th pSortableColumn="isActive">
              Status <p-sortIcon field="isActive"></p-sortIcon>
            </th>
            <th pSortableColumn="createdAt">
              Created <p-sortIcon field="createdAt"></p-sortIcon>
            </th>
            <th>Actions</th>
          </tr>
        </ng-template>
        
        <ng-template pTemplate="body" let-course>
          <tr>
            <td>{{ course.title }}</td>
            <td>{{ course.category }}</td>
            <td>
              <span 
                class="status-badge" 
                [ngClass]="course.isActive ? 'status-completed' : 'status-failed'">
                {{ course.isActive ? 'Active' : 'Inactive' }}
              </span>
            </td>
            <td>{{ course.createdAt | date: 'short' }}</td>
            <td>
              <p-button 
                icon="pi pi-pencil" 
                styleClass="p-button-text p-button-plain"
                pTooltip="Edit"
                (onClick)="editCourse(course)">
              </p-button>
              <p-button 
                icon="pi pi-trash" 
                styleClass="p-button-text p-button-danger"
                pTooltip="Delete"
                (onClick)="deleteCourse(course)">
              </p-button>
            </td>
          </tr>
        </ng-template>
        
        <ng-template pTemplate="emptymessage">
          <tr>
            <td colspan="5" class="text-center py-4">No courses found</td>
          </tr>
        </ng-template>
      </p-table>
    </div>

    <!-- Course Dialog -->
    <p-dialog 
      [(visible)]="courseDialog" 
      [header]="isEditMode ? 'Edit Course' : 'Add Course'"
      [modal]="true" 
      [closable]="true"
      [style]="{ width: '500px' }">
      
      <form [formGroup]="courseForm" (ngSubmit)="saveCourse()">
        <div class="form-grid">
          <div class="form-group">
            <label for="title">Title *</label>
            <input 
              pInputText 
              id="title" 
              formControlName="title"
              [ngClass]="{ 'ng-invalid ng-dirty': courseForm.get('title')?.invalid && courseForm.get('title')?.touched }"
              placeholder="Enter course title">
            <small 
              class="p-error" 
              *ngIf="courseForm.get('title')?.invalid && courseForm.get('title')?.touched">
              Title is required
            </small>
          </div>
          
          <div class="form-group">
            <label for="category">Category *</label>
            <input 
              pInputText 
              id="category" 
              formControlName="category"
              [ngClass]="{ 'ng-invalid ng-dirty': courseForm.get('category')?.invalid && courseForm.get('category')?.touched }"
              placeholder="Enter course category">
            <small 
              class="p-error" 
              *ngIf="courseForm.get('category')?.invalid && courseForm.get('category')?.touched">
              Category is required
            </small>
          </div>
        </div>
        
        <div class="form-group">
          <p-checkbox 
            formControlName="isActive" 
            label="Active"
            [binary]="true">
          </p-checkbox>
        </div>
        
        <div class="btn-group">
          <p-button 
            label="Cancel" 
            icon="pi pi-times" 
            styleClass="p-button-text"
            type="button"
            (onClick)="hideDialog()">
          </p-button>
          <p-button 
            label="Save" 
            icon="pi pi-check" 
            type="submit"
            [disabled]="courseForm.invalid"
            [loading]="saving">
          </p-button>
        </div>
      </form>
    </p-dialog>

    <p-toast></p-toast>
    <p-confirmDialog></p-confirmDialog>
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    
    .page-header h1 {
      margin: 0;
      color: #1f2937;
    }
  `]
})
export class CoursesComponent implements OnInit {
  courses: Course[] = [];
  courseDialog = false;
  courseForm!: FormGroup;
  isEditMode = false;
  selectedCourse: Course | null = null;
  loading = false;
  saving = false;

  constructor(
    private courseService: CourseService,
    private fb: FormBuilder,
    private messageService: MessageService,
    private confirmationService: ConfirmationService
  ) {
    this.initForm();
  }

  ngOnInit() {
    this.loadCourses();
  }

  initForm() {
    this.courseForm = this.fb.group({
      title: ['', Validators.required],
      category: ['', Validators.required],
      isActive: [true]
    });
  }

  loadCourses() {
    this.loading = true;
    this.courseService.getCourses().subscribe({
      next: (courses) => {
        this.courses = courses;
        this.loading = false;
      },
      error: (error) => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load courses'
        });
        this.loading = false;
      }
    });
  }

  openNewCourseDialog() {
    this.isEditMode = false;
    this.selectedCourse = null;
    this.courseForm.reset({ isActive: true });
    this.courseDialog = true;
  }

  editCourse(course: Course) {
    this.isEditMode = true;
    this.selectedCourse = course;
    this.courseForm.patchValue({
      title: course.title,
      category: course.category,
      isActive: course.isActive
    });
    this.courseDialog = true;
  }

  deleteCourse(course: Course) {
    this.confirmationService.confirm({
      message: `Are you sure you want to delete "${course.title}"?`,
      header: 'Confirm Deletion',
      icon: 'pi pi-exclamation-triangle',
      accept: () => {
        this.courseService.deleteCourse(course.id).subscribe({
          next: () => {
            this.messageService.add({
              severity: 'success',
              summary: 'Success',
              detail: 'Course deleted successfully'
            });
            this.loadCourses();
          },
          error: () => {
            this.messageService.add({
              severity: 'error',
              summary: 'Error',
              detail: 'Failed to delete course'
            });
          }
        });
      }
    });
  }

  saveCourse() {
    if (this.courseForm.invalid) return;

    this.saving = true;
    const formValue = this.courseForm.value;

    const courseData = {
      title: formValue.title,
      category: formValue.category,
      isActive: formValue.isActive
    };

    const operation = this.isEditMode && this.selectedCourse
      ? this.courseService.updateCourse(this.selectedCourse.id, courseData as UpdateCourseDto)
      : this.courseService.createCourse(courseData as CreateCourseDto);

    operation.subscribe({
      next: () => {
        this.messageService.add({
          severity: 'success',
          summary: 'Success',
          detail: `Course ${this.isEditMode ? 'updated' : 'created'} successfully`
        });
        this.loadCourses();
        this.hideDialog();
        this.saving = false;
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: `Failed to ${this.isEditMode ? 'update' : 'create'} course`
        });
        this.saving = false;
      }
    });
  }

  hideDialog() {
    this.courseDialog = false;
    this.courseForm.reset();
    this.selectedCourse = null;
    this.saving = false;
  }
}
