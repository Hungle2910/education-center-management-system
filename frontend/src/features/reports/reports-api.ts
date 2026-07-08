import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";

export interface ClassRevenueItem {
  classId: string;
  className: string;
  revenue: number;
}

export interface MonthlyRevenueItem {
  month: string;
  revenue: number;
}

export interface TuitionReportResponse {
  totalCollected: number;
  totalUnpaid: number;
  totalOverdue: number;
  revenueByClass: ClassRevenueItem[];
  revenueByMonth: MonthlyRevenueItem[];
}

export interface ClassReportItem {
  classId: string;
  className: string;
  activeStudentCount: number;
  targetStudentCount: number;
  status: string;
  isAtRiskOfLoss: boolean;
}

export interface TeacherReportItem {
  teacherId: string;
  teacherName: string;
  completedLessonsCount: number;
  cancelledLessonsCount: number;
  makeupLessonsCount: number;
  projectedSalary: number;
  paidSalary: number;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export function useTuitionReport() {
  return useQuery<TuitionReportResponse>({
    queryKey: ["reports", "tuition"],
    queryFn: async () => {
      const res = await apiClient.get<ApiResponse<TuitionReportResponse>>("/api/reports/tuition");
      return res.data.data;
    },
  });
}

export function useClassReport() {
  return useQuery<ClassReportItem[]>({
    queryKey: ["reports", "classes"],
    queryFn: async () => {
      const res = await apiClient.get<ApiResponse<ClassReportItem[]>>("/api/reports/classes");
      return res.data.data;
    },
  });
}

export function useTeacherReport() {
  return useQuery<TeacherReportItem[]>({
    queryKey: ["reports", "teachers"],
    queryFn: async () => {
      const res = await apiClient.get<ApiResponse<TeacherReportItem[]>>("/api/reports/teachers");
      return res.data.data;
    },
  });
}
