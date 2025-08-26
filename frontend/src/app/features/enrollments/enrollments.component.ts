import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { DropdownModule } from 'primeng/dropdown';
import { FileUploadModule } from 'primeng/fileupload';
import { ButtonModule } from 'primeng/button';
import { CardModule } from 'primeng/card';
import { ProgressBarModule } from 'primeng/progressbar';
import { MessageService } from 'primeng/api';
import { ToastModule } from 'primeng/toast';
import { Subscription, interval } from 'rxjs';
import { switchMap, takeWhile } from 'rxjs/operators';
import { Course } from '../../core/models/course.model';
import { ImportJob, ImportJobStatus } from '../../core/models/import-job.model';
import { CourseService } from '../../core/services/course.service';
import { EnrollmentService } from '../../core/services/enrollment.service';

@Component({
  selector: 'app-enrollments',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    DropdownModule,
    FileUploadModule,
    ButtonModule,
    CardModule,
    ProgressBarModule,
    ToastModule
  ],
  providers: [MessageService],
  template: `
    <div class="page-header">
      <h1>Bulk Enrollment</h1>
      <p>Upload a CSV file to enroll multiple users in a course</p>
    </div>

    <div class="card">
      <h3>Upload Enrollment File</h3>
      
      <form [formGroup]="enrollmentForm" (ngSubmit)="uploadFile()">
        <div class="form-group">
          <label for="course">Select Course *</label>
          <p-dropdown
            formControlName="courseId"
            [options]="courseOptions"
            optionLabel="label"
            optionValue="value"
            placeholder="Choose a course"
            [style]="{ width: '100%' }"
            [ngClass]="{ 'ng-invalid ng-dirty': enrollmentForm.get('courseId')?.invalid && enrollmentForm.get('courseId')?.touched }">
          </p-dropdown>
          <small 
            class="p-error" 
            *ngIf="enrollmentForm.get('courseId')?.invalid && enrollmentForm.get('courseId')?.touched">
            Course selection is required
          </small>
        </div>

        <div class="form-group">
          <label>CSV File *</label>
          <div class="upload-area" [ngClass]="{ 'dragover': isDragOver }">
            <p-fileUpload
              #fileUpload
              mode="basic"
              chooseLabel="Choose CSV File"
              [auto]="false"
              accept=".csv"
              [maxFileSize]="10000000"
              (onSelect)="onFileSelect($event)"
              (onRemove)="onFileRemove()"
              [disabled]="uploading">
            </p-fileUpload>
            
            <div *ngIf="!selectedFile" class="upload-instructions">
              <i class="pi pi-upload" style="font-size: 3rem; color: #6b7280; margin-bottom: 1rem;"></i>
              <p>Select a CSV file or drag and drop here</p>
              <p class="text-sm text-gray-500">File format: email,name</p>
            </div>
            
            <div *ngIf="selectedFile" class="file-info">
              <i class="pi pi-file" style="color: #059669; margin-right: 0.5rem;"></i>
              <span>{{ selectedFile.name }} ({{ formatFileSize(selectedFile.size) }})</span>
            </div>
          </div>
        </div>

        <div class="btn-group">
          <p-button 
            label="Upload and Process" 
            icon="pi pi-upload" 
            type="submit"
            [disabled]="enrollmentForm.invalid || !selectedFile || uploading"
            [loading]="uploading">
          </p-button>
        </div>
      </form>
    </div>

    <!-- Job Status Card -->
    <div class="card" *ngIf="currentJob">
      <h3>Import Job Status</h3>
      
      <div class="job-header">
        <div>
          <h4>{{ currentJob.fileName }}</h4>
          <p>Course: {{ currentJob.courseTitle }}</p>
        </div>
        <span class="status-badge" [ngClass]="getStatusClass(currentJob.status)">
          {{ getStatusText(currentJob.status) }}
        </span>
      </div>

      <div *ngIf="currentJob.status === 1" class="progress-section">
        <p-progressBar mode="indeterminate"></p-progressBar>
        <p class="text-center">Processing your file...</p>
      </div>

      <!-- SEMPRE MOSTRA OS CONTADORES PARA TESTE -->
      <div class="stats-grid">
        <div class="stat-card">
          <div class="stat-value">{{ currentJob.totalRows || 0 }}</div>
          <div class="stat-label">Total Rows</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ currentJob.succeeded || 0 }}</div>
          <div class="stat-label">Succeeded</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ currentJob.failed || 0 }}</div>
          <div class="stat-label">Failed</div>
        </div>
        <div class="stat-card">
          <div class="stat-value">{{ currentJob.ignoredDuplicates || 0 }}</div>
          <div class="stat-label">Duplicates Ignored</div>
        </div>
      </div>

      <div *ngIf="currentJob.note" class="job-note">
        <p><strong>Note:</strong> {{ currentJob.note }}</p>
      </div>

      <div class="job-meta">
        <p><small>Started: {{ currentJob.createdAt | date: 'medium' }}</small></p>
        <p *ngIf="currentJob.finishedAt">
          <small>Finished: {{ currentJob.finishedAt | date: 'medium' }}</small>
        </p>
      </div>
    </div>

    <!-- CSV Format Help -->
    <div class="card">
      <h3>CSV Format Requirements</h3>
      <p>Your CSV file must have the following format:</p>
      
      <pre class="csv-example">
email,name
ana&#64;example.com,Ana Silva
joao&#64;example.com,João Souza
maria&#64;example.com,Maria Santos</pre>
      
      <ul>
        <li>First row must be the header: <code>email,name</code></li>
        <li>Email addresses must be valid</li>
        <li>Duplicate emails within the file will be automatically removed</li>
        <li>Users already enrolled in the course will be skipped</li>
        <li>Maximum file size: 10MB</li>
      </ul>
    </div>

    <p-toast></p-toast>
  `,
  styles: [`
    .page-header {
      margin-bottom: 2rem;
    }
    
    .page-header h1 {
      margin: 0 0 0.5rem 0;
      color: #1f2937;
    }
    
    .page-header p {
      margin: 0;
      color: #6b7280;
    }
    
    .upload-area {
      padding: 2rem;
      text-align: center;
      border: 2px dashed #d1d5db;
      border-radius: 8px;
      background: #f9fafb;
      transition: all 0.3s ease;
    }
    
    .upload-area.dragover {
      border-color: #6366f1;
      background: #f0f9ff;
    }
    
    .upload-instructions p {
      margin: 0.5rem 0;
      color: #6b7280;
    }
    
    .file-info {
      display: flex;
      align-items: center;
      justify-content: center;
      padding: 1rem;
      background: #f0fdf4;
      border-radius: 6px;
      color: #059669;
    }
    
    .job-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1.5rem;
    }
    
    .job-header h4 {
      margin: 0 0 0.5rem 0;
      color: #1f2937;
    }
    
    .job-header p {
      margin: 0;
      color: #6b7280;
    }
    
    .progress-section {
      margin: 1.5rem 0;
    }
    
    .progress-section p {
      margin-top: 1rem;
      color: #6b7280;
    }
    
    .job-note {
      background: #fef3c7;
      border: 1px solid #f59e0b;
      border-radius: 6px;
      padding: 1rem;
      margin: 1rem 0;
    }
    
    .job-meta {
      margin-top: 1.5rem;
      padding-top: 1rem;
      border-top: 1px solid #e5e7eb;
    }
    
    .job-meta p {
      margin: 0.25rem 0;
      color: #6b7280;
    }
    
    .csv-example {
      background: #f3f4f6;
      border: 1px solid #d1d5db;
      border-radius: 6px;
      padding: 1rem;
      font-family: 'Courier New', monospace;
      margin: 1rem 0;
    }
    
    ul {
      margin: 1rem 0;
      padding-left: 1.5rem;
    }
    
    li {
      margin: 0.5rem 0;
      color: #6b7280;
    }
    
    code {
      background: #f3f4f6;
      padding: 0.2rem 0.4rem;
      border-radius: 3px;
      font-family: 'Courier New', monospace;
    }
    
    .text-sm {
      font-size: 0.875rem;
    }
    
    .text-gray-500 {
      color: #6b7280;
    }
    
    :host ::ng-deep .p-fileupload-choose {
      background: #6366f1;
      border-color: #6366f1;
    }
    
    :host ::ng-deep .p-fileupload-choose:hover {
      background: #4f46e5;
      border-color: #4f46e5;
    }
  `]
})
export class EnrollmentsComponent implements OnInit, OnDestroy {
  enrollmentForm!: FormGroup;
  courses: Course[] = [];
  courseOptions: any[] = [];
  selectedFile: File | null = null;
  uploading = false;
  isDragOver = false;
  currentJob: ImportJob | null = null;
  pollingSubscription?: Subscription;

  constructor(
    private fb: FormBuilder,
    private courseService: CourseService,
    private enrollmentService: EnrollmentService,
    private messageService: MessageService
  ) {
    this.initForm();
  }

  ngOnInit() {
    this.loadCourses();
  }

  ngOnDestroy() {
    this.stopPolling();
  }

  initForm() {
    this.enrollmentForm = this.fb.group({
      courseId: ['', Validators.required]
    });
  }

  loadCourses() {
    this.courseService.getCourses().subscribe({
      next: (courses) => {
        this.courses = courses.filter(c => c.isActive);
        this.courseOptions = this.courses.map(course => ({
          label: course.title,
          value: course.id
        }));
      },
      error: () => {
        this.messageService.add({
          severity: 'error',
          summary: 'Error',
          detail: 'Failed to load courses'
        });
      }
    });
  }

  onFileSelect(event: any) {
    const file = event.files[0];
    if (file && file.type === 'text/csv') {
      this.selectedFile = file;
    } else {
      this.messageService.add({
        severity: 'error',
        summary: 'Invalid File',
        detail: 'Please select a valid CSV file'
      });
    }
  }

  onFileRemove() {
    this.selectedFile = null;
  }

  uploadFile() {
    if (this.enrollmentForm.invalid || !this.selectedFile) return;

    this.uploading = true;
    const courseId = this.enrollmentForm.get('courseId')?.value;

    this.enrollmentService.importEnrollments(courseId, this.selectedFile).subscribe({
      next: (job) => {
        this.currentJob = job;
        this.uploading = false;
        this.messageService.add({
          severity: 'success',
          summary: 'Upload Successful',
          detail: 'File uploaded successfully. Processing started.'
        });
        this.startPolling(job.id);
      },
      error: () => {
        this.uploading = false;
        this.messageService.add({
          severity: 'error',
          summary: 'Upload Failed',
          detail: 'Failed to upload file. Please try again.'
        });
      }
    });
  }

  startPolling(jobId: number) {
    this.stopPolling();
    
    console.log('Starting polling for job:', jobId);
    
    this.pollingSubscription = interval(2000)
      .pipe(
        switchMap(() => {
          console.log('Polling job status...');
          return this.enrollmentService.getImportJob(jobId);
        }),
        takeWhile((job: ImportJob) => job.status === 0 || job.status === 1, true)
      )
      .subscribe({
        next: (job: ImportJob) => {
          console.log('Job status received:', job);
          this.currentJob = job;
          
          if (job.status === ImportJobStatus.Completed) {
            this.messageService.add({
              severity: 'success',
              summary: 'Processing Complete',
              detail: `Successfully processed ${job.succeeded} enrollments`
            });
          } else if (job.status === ImportJobStatus.Failed) {
            this.messageService.add({
              severity: 'error',
              summary: 'Processing Failed',
              detail: job.note || 'An error occurred during processing'
            });
          }
        },
        error: (error: any) => {
          console.error('Error polling job status:', error);
          this.messageService.add({
            severity: 'error',
            summary: 'Status Update Failed',
            detail: 'Failed to get job status'
          });
        }
      });
  }

  stopPolling() {
    if (this.pollingSubscription) {
      this.pollingSubscription.unsubscribe();
      this.pollingSubscription = undefined;
    }
  }

  getStatusClass(status: ImportJobStatus): string {
    switch (status) {
      case ImportJobStatus.Queued:
      case 0:
        return 'status-queued';
      case ImportJobStatus.Processing:
      case 1:
        return 'status-processing';
      case ImportJobStatus.Completed:
      case 2:
        return 'status-completed';
      case ImportJobStatus.Failed:
      case 3:
        return 'status-failed';
      default:
        return '';
    }
  }

  getStatusText(status: ImportJobStatus): string {
    switch (status) {
      case ImportJobStatus.Queued:
      case 0:
        return 'Queued';
      case ImportJobStatus.Processing:
      case 1:
        return 'Processing';
      case ImportJobStatus.Completed:
      case 2:
        return 'Completed';
      case ImportJobStatus.Failed:
      case 3:
        return 'Failed';
      default:
        return String(status) || 'Unknown';
    }
  }

  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }
}
