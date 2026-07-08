import { useQuery } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";

export interface DashboardFilter {
  month?: number;
  year?: number;
  classId?: string;
}

export interface AdminOverviewResponse {
  totalTuitionRevenue: number;
  activeStudentCount: number;
  activeClassCount: number;
  recruitingClassCount: number;
  scheduleConflictCount: number;
}

export interface TodayScheduleItem {
  occurrenceId: string;
  classId: string;
  className: string;
  date: string;
  startTime: string;
  endTime: string;
  roomId: string;
  roomName: string;
  teacherId?: string;
  teacherName?: string;
  status: string;
}

export interface PendingPaymentInvoiceItem {
  invoiceId: string;
  studentId: string;
  studentName: string;
  classId: string;
  className: string;
  month: string;
  totalAmount: number;
  paidAmount?: number;
  status: string;
  paymentProofUrl?: string;
  createdAtUtc: string;
}

export interface OperationsDashboardResponse {
  todaySchedules: TodayScheduleItem[];
  trialSessionsTodayCount: number;
  pendingPaymentInvoices: PendingPaymentInvoiceItem[];
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export function useAdminOverview(filter: DashboardFilter = {}) {
  const queryParams = new URLSearchParams();
  if (filter.month !== undefined) queryParams.append("month", filter.month.toString());
  if (filter.year !== undefined) queryParams.append("year", filter.year.toString());
  if (filter.classId) queryParams.append("classId", filter.classId);

  const queryString = queryParams.toString();
  const url = `/api/dashboard/admin/overview${queryString ? `?${queryString}` : ""}`;

  return useQuery<AdminOverviewResponse>({
    queryKey: ["dashboard", "overview", filter],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<AdminOverviewResponse>>(url);
      return response.data.data;
    },
  });
}

export function useAdminOperations(filter: DashboardFilter = {}) {
  const queryParams = new URLSearchParams();
  if (filter.month !== undefined) queryParams.append("month", filter.month.toString());
  if (filter.year !== undefined) queryParams.append("year", filter.year.toString());
  if (filter.classId) queryParams.append("classId", filter.classId);

  const queryString = queryParams.toString();
  const url = `/api/dashboard/admin/operations${queryString ? `?${queryString}` : ""}`;

  return useQuery<OperationsDashboardResponse>({
    queryKey: ["dashboard", "operations", filter],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<OperationsDashboardResponse>>(url);
      return response.data.data;
    },
  });
}
