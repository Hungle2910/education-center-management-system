export interface CreateScheduleRequest {
  classId: string;
  dayOfWeek: number; // 0: Sunday, 1: Monday, ...
  startTime: string; // "HH:mm:ss" or "HH:mm"
  endTime: string;
  roomId: string;
  teacherId?: string | null;
}

export interface ScheduleResponse {
  id: string;
  classId: string;
  className: string;
  dayOfWeek: number;
  startTime: string;
  endTime: string;
  roomId: string;
  roomName: string;
  teacherId?: string | null;
  teacherName?: string | null;
}

export interface ScheduleOccurrenceResponse {
  id: string;
  classId: string;
  className: string;
  date: string; // "YYYY-MM-DD"
  startTime: string;
  endTime: string;
  roomId: string;
  roomName: string;
  teacherId?: string | null;
  teacherName?: string | null;
  status: string;
  reason?: string | null;
}

export interface ConflictCheckRequest {
  excludeOccurrenceId?: string | null;
  date: string;
  startTime: string;
  endTime: string;
  roomId: string;
  teacherId?: string | null;
}

export interface ConflictCheckResponse {
  hasConflict: boolean;
  message?: string | null;
}
