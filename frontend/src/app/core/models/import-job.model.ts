export enum ImportJobStatus {
  Queued = 0,
  Processing = 1, 
  Completed = 2,
  Failed = 3
}

export interface ImportJob {
  id: number;
  status: ImportJobStatus;
  createdAt: string;
  finishedAt?: string;
  totalRows: number;
  succeeded: number;
  failed: number;
  ignoredDuplicates: number;
  note?: string;
  fileName: string;
  filePath: string;
  courseId: number;
  courseTitle: string;
}
