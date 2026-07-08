import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

export interface LeadResponse {
  id: string;
  studentName: string;
  parentName?: string;
  parentPhone: string;
  email?: string;
  source?: string;
  status: string;
  notes?: string;
  createdAtUtc: string;
}

export interface TrialSessionResponse {
  id: string;
  leadId: string;
  studentName: string;
  classId: string;
  className: string;
  trialDate: string;
  teacherId?: string;
  teacherName?: string;
  feedback?: string;
  result?: string;
  notes?: string;
  createdAtUtc: string;
}

export interface ParentCareLogResponse {
  id: string;
  parentId?: string;
  parentName?: string;
  leadId?: string;
  leadStudentName?: string;
  staffId: string;
  contactType: string;
  notes: string;
  loggedAtUtc: string;
}

export const useLeads = () => {
  return useQuery<LeadResponse[]>({
    queryKey: ["leads"],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<LeadResponse[]>>("/admissions/leads");
      return response.data.data;
    },
  });
};

export const useCreateLead = () => {
  const queryClient = useQueryClient();
  return useMutation<
    ApiResponse<LeadResponse>,
    Error,
    Omit<LeadResponse, "id" | "status" | "createdAtUtc">
  >({
    mutationFn: async (data) => {
      const response = await apiClient.post<ApiResponse<LeadResponse>>("/admissions/leads", data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads"] });
    },
  });
};

export const useUpdateLead = () => {
  const queryClient = useQueryClient();
  return useMutation<
    ApiResponse<LeadResponse>,
    Error,
    { id: string; data: Omit<LeadResponse, "id" | "createdAtUtc"> }
  >({
    mutationFn: async ({ id, data }) => {
      const response = await apiClient.put<ApiResponse<LeadResponse>>(`/admissions/leads/${id}`, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads"] });
    },
  });
};

export const useConvertLead = () => {
  const queryClient = useQueryClient();
  return useMutation<ApiResponse<void>, Error, string>({
    mutationFn: async (id) => {
      const response = await apiClient.post<ApiResponse<void>>(`/admissions/leads/${id}/convert`);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["leads"] });
      queryClient.invalidateQueries({ queryKey: ["students"] });
      queryClient.invalidateQueries({ queryKey: ["parents"] });
    },
  });
};

export const useTrialSessions = () => {
  return useQuery<TrialSessionResponse[]>({
    queryKey: ["trial-sessions"],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<TrialSessionResponse[]>>("/admissions/trials");
      return response.data.data;
    },
  });
};

export const useScheduleTrial = () => {
  const queryClient = useQueryClient();
  return useMutation<
    ApiResponse<TrialSessionResponse>,
    Error,
    { leadId: string; classId: string; trialDate: string; teacherId?: string; notes?: string }
  >({
    mutationFn: async (data) => {
      const response = await apiClient.post<ApiResponse<TrialSessionResponse>>("/admissions/trials", data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["trial-sessions"] });
      queryClient.invalidateQueries({ queryKey: ["leads"] });
    },
  });
};

export const useEvaluateTrial = () => {
  const queryClient = useQueryClient();
  return useMutation<
    ApiResponse<void>,
    Error,
    { id: string; feedback?: string; result: string; notes?: string }
  >({
    mutationFn: async ({ id, ...data }) => {
      const response = await apiClient.post<ApiResponse<void>>(`/admissions/trials/${id}/evaluate`, data);
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["trial-sessions"] });
      queryClient.invalidateQueries({ queryKey: ["leads"] });
    },
  });
};

export const useCareLogs = (parentId?: string, leadId?: string) => {
  return useQuery<ParentCareLogResponse[]>({
    queryKey: ["care-logs", parentId, leadId],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (parentId) params.append("parentId", parentId);
      if (leadId) params.append("leadId", leadId);
      const response = await apiClient.get<ApiResponse<ParentCareLogResponse[]>>(
        `/admissions/care-logs?${params.toString()}`
      );
      return response.data.data;
    },
  });
};

export const useCreateCareLog = () => {
  const queryClient = useQueryClient();
  return useMutation<
    ApiResponse<ParentCareLogResponse>,
    Error,
    { parentId?: string; leadId?: string; contactType: string; notes: string }
  >({
    mutationFn: async (data) => {
      const response = await apiClient.post<ApiResponse<ParentCareLogResponse>>("/admissions/care-logs", data);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["care-logs", variables.parentId, variables.leadId] });
    },
  });
};
