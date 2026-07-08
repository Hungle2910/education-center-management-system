export interface TeacherResponse {
  id: string;
  fullName: string;
  email?: string;
  phoneNumber?: string;
  subject?: string;
  isActive: boolean;
}

export interface TeacherRequest {
  fullName: string;
  email?: string;
  phoneNumber?: string;
  subject?: string;
  isActive: boolean;
}

export interface ClassResponse {
  id: string;
  name: string;
  subject?: string;
  teacherId?: string | null;
  teacherName?: string;
  status: string;
  minStudents: number;
  maxStudents: number;
}

export interface ClassRequest {
  name: string;
  subject?: string;
  teacherId?: string | null;
  status?: string;
  minStudents: number;
  maxStudents: number;
}
