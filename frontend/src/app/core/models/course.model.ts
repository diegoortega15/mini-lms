export interface Course {
  id: number;
  title: string;
  category: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateCourseDto {
  title: string;
  category: string;
  isActive: boolean;
}

export interface UpdateCourseDto {
  title: string;
  category: string;
  isActive: boolean;
}
