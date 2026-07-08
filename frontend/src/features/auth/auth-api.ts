import { apiClient } from "@/lib/http/api-client";
import type {
  ApiResponse,
  AuthUser,
  LoginFormValues,
  LoginResponse,
} from "./types";

export async function login(values: LoginFormValues): Promise<LoginResponse> {
  const response = await apiClient.post<ApiResponse<LoginResponse>>(
    "/auth/login",
    values,
  );

  return response.data.data;
}

export async function getCurrentUser(): Promise<AuthUser> {
  const response =
    await apiClient.get<ApiResponse<AuthUser>>("/auth/me");

  return response.data.data;
}
