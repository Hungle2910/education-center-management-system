import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";
import {
  CreateScheduleRequest,
  ScheduleResponse,
  ScheduleOccurrenceResponse,
  ConflictCheckResponse,
} from "./schedule-types";

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export const useCreateSchedule = () => {
  const queryClient = useQueryClient();
  return useMutation<ScheduleResponse, Error, CreateScheduleRequest>({
    mutationFn: async (req) => {
      const response = await apiClient.post<ApiResponse<ScheduleResponse>>("/schedules", req);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["calendar"] });
    },
  });
};

export const useCalendar = (startDate: string, endDate: string) => {
  return useQuery<ScheduleOccurrenceResponse[]>({
    queryKey: ["calendar", startDate, endDate],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<ScheduleOccurrenceResponse[]>>(
        `/schedules/calendar?startDate=${startDate}&endDate=${endDate}`
      );
      return response.data.data;
    },
    enabled: !!startDate && !!endDate,
  });
};

export const checkConflicts = async (params: {
  date: string;
  startTime: string;
  endTime: string;
  roomId: string;
  teacherId?: string | null;
  excludeOccurrenceId?: string | null;
}): Promise<ConflictCheckResponse> => {
  const url = `/schedules/conflicts/check?date=${params.date}&startTime=${params.startTime}&endTime=${params.endTime}&roomId=${params.roomId}` +
    (params.teacherId ? `&teacherId=${params.teacherId}` : "") +
    (params.excludeOccurrenceId ? `&excludeOccurrenceId=${params.excludeOccurrenceId}` : "");
  
  const response = await apiClient.get<ApiResponse<ConflictCheckResponse>>(url);
  return response.data.data;
};
