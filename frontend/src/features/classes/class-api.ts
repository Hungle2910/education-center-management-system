import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";
import { TeacherResponse, TeacherRequest, ClassResponse, ClassRequest } from "./class-types";

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export const useTeachers = () => {
  return useQuery<TeacherResponse[]>({
    queryKey: ["teachers"],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<TeacherResponse[]>>("/teachers");
      return response.data.data;
    },
  });
};

export const useCreateTeacher = () => {
  const queryClient = useQueryClient();
  return useMutation<TeacherResponse, Error, TeacherRequest>({
    mutationFn: async (teacher) => {
      const response = await apiClient.post<ApiResponse<TeacherResponse>>("/teachers", teacher);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });
};

export const useUpdateTeacher = ({ id }: { id: string }) => {
  const queryClient = useQueryClient();
  return useMutation<TeacherResponse, Error, TeacherRequest>({
    mutationFn: async (teacher) => {
      const response = await apiClient.put<ApiResponse<TeacherResponse>>(`/teachers/${id}`, teacher);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["teachers"] });
    },
  });
};

export const useClasses = () => {
  return useQuery<ClassResponse[]>({
    queryKey: ["classes"],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<ClassResponse[]>>("/classes");
      return response.data.data;
    },
  });
};

export const useCreateClass = () => {
  const queryClient = useQueryClient();
  return useMutation<ClassResponse, Error, ClassRequest>({
    mutationFn: async (cls) => {
      const response = await apiClient.post<ApiResponse<ClassResponse>>("/classes", cls);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
};

export const useUpdateClass = ({ id }: { id: string }) => {
  const queryClient = useQueryClient();
  return useMutation<ClassResponse, Error, ClassRequest>({
    mutationFn: async (cls) => {
      const response = await apiClient.put<ApiResponse<ClassResponse>>(`/classes/${id}`, cls);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["classes"] });
    },
  });
};
