import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";
import {
  StudentResponse,
  StudentRequest,
  ParentResponse,
  ParentRequest,
} from "./student-types";

// Core API response contract matching backend ApiResponse<T>
interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export const useStudents = () => {
  return useQuery<StudentResponse[]>({
    queryKey: ["students"],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<StudentResponse[]>>("/students");
      return response.data.data;
    },
  });
};

export const useCreateStudent = () => {
  const queryClient = useQueryClient();
  return useMutation<StudentResponse, Error, StudentRequest>({
    mutationFn: async (student) => {
      const response = await apiClient.post<ApiResponse<StudentResponse>>("/students", student);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
    },
  });
};

export const useUpdateStudent = ({ id }: { id: string }) => {
  const queryClient = useQueryClient();
  return useMutation<StudentResponse, Error, StudentRequest>({
    mutationFn: async (student) => {
      const response = await apiClient.put<ApiResponse<StudentResponse>>(`/students/${id}`, student);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
    },
  });
};

export const useParents = () => {
  return useQuery<ParentResponse[]>({
    queryKey: ["parents"],
    queryFn: async () => {
      const response = await apiClient.get<ApiResponse<ParentResponse[]>>("/parents");
      return response.data.data;
    },
  });
};

export const useCreateParent = () => {
  const queryClient = useQueryClient();
  return useMutation<ParentResponse, Error, ParentRequest>({
    mutationFn: async (parent) => {
      const response = await apiClient.post<ApiResponse<ParentResponse>>("/parents", parent);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["parents"] });
    },
  });
};

export const useUpdateParent = ({ id }: { id: string }) => {
  const queryClient = useQueryClient();
  return useMutation<ParentResponse, Error, ParentRequest>({
    mutationFn: async (parent) => {
      const response = await apiClient.put<ApiResponse<ParentResponse>>(`/parents/${id}`, parent);
      return response.data.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["parents"] });
    },
  });
};
