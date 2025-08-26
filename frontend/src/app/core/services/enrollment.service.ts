import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ImportJob } from '../models/import-job.model';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class EnrollmentService {
  private apiUrl = `${environment.apiUrl}/api/enrollments`;

  constructor(private http: HttpClient) {}

  importEnrollments(courseId: number, file: File): Observable<ImportJob> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<ImportJob>(`${this.apiUrl}/import?courseId=${courseId}`, formData);
  }

  getImportJob(jobId: number): Observable<ImportJob> {
    return this.http.get<ImportJob>(`${this.apiUrl}/import/${jobId}`);
  }
}
