import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";
import { OccurrenceAttendance, SubmitAttendancePayload } from "./attendance-types";

export function useOccurrenceAttendance(occurrenceId: string | null) {
  return useQuery<OccurrenceAttendance>({
    queryKey: ["attendance", occurrenceId],
    queryFn: async () => {
      const response = await apiClient.get(`/attendance/occurrence/${occurrenceId}`);
      return response.data.data;
    },
    enabled: !!occurrenceId,
  });
}

export function useSubmitAttendance() {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: async (payload: SubmitAttendancePayload) => {
      const response = await apiClient.post("/attendance/submit", payload);
      return response.data;
    },
    onSuccess: (_, variables) => {
      queryClient.invalidateQueries({ queryKey: ["attendance", variables.occurrenceId] });
      queryClient.invalidateQueries({ queryKey: ["calendar"] }); // Refetch calendar to show updated "Đã học" status
    },
  });
}
