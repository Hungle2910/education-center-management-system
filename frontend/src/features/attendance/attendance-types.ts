export interface StudentAttendance {
  studentId: string;
  studentName: string;
  status: string; // Có mặt, Vắng có phép, Vắng không phép, Đi trễ, Đã học bù
  notes: string | null;
}

export interface OccurrenceAttendance {
  occurrenceId: string;
  className: string;
  date: string;
  startTime: string;
  endTime: string;
  status: string;
  students: StudentAttendance[];
}

export interface SubmitAttendancePayload {
  occurrenceId: string;
  students: StudentAttendance[];
}
