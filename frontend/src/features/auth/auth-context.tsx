"use client";

import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import { AxiosError } from "axios";
import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useMemo,
  useState,
} from "react";
import { clearAccessToken, getAccessToken, setAccessToken } from "@/lib/http/auth-token";
import { getCurrentUser, login } from "./auth-api";
import type { ApiResponse, AuthUser, LoginFormValues } from "./types";

type AuthContextValue = {
  user: AuthUser | null;
  isAuthenticated: boolean;
  isCheckingAuth: boolean;
  login: (values: LoginFormValues) => Promise<void>;
  logout: () => void;
};

const AuthContext = createContext<AuthContextValue | null>(null);

type AuthProviderProps = {
  children: ReactNode;
};

export function AuthProvider({ children }: AuthProviderProps) {
  const queryClient = useQueryClient();
  const [hasToken, setHasToken] = useState(() => Boolean(getAccessToken()));

  const currentUserQuery = useQuery({
    queryKey: ["auth", "me"],
    queryFn: getCurrentUser,
    enabled: hasToken,
    retry: false,
  });

  const loginMutation = useMutation({
    mutationFn: login,
    onSuccess: (response) => {
      setAccessToken(response.accessToken);
      setHasToken(true);
      queryClient.setQueryData(["auth", "me"], response.user);
    },
  });

  const logout = useCallback(() => {
    clearAccessToken();
    setHasToken(false);
    queryClient.removeQueries({ queryKey: ["auth"] });
  }, [queryClient]);

  const loginAction = useCallback(
    async (values: LoginFormValues) => {
      await loginMutation.mutateAsync(values);
    },
    [loginMutation],
  );

  const value = useMemo<AuthContextValue>(
    () => ({
      user: currentUserQuery.data ?? null,
      isAuthenticated: Boolean(currentUserQuery.data),
      isCheckingAuth: hasToken && currentUserQuery.isLoading,
      login: loginAction,
      logout,
    }),
    [
      currentUserQuery.data,
      currentUserQuery.isLoading,
      hasToken,
      loginAction,
      logout,
    ],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth(): AuthContextValue {
  const value = useContext(AuthContext);

  if (!value) {
    throw new Error("useAuth must be used inside AuthProvider.");
  }

  return value;
}

export function getAuthErrorMessage(error: unknown): string {
  if (error instanceof AxiosError) {
    const response = error.response?.data as ApiResponse<unknown> | undefined;
    return response?.message ?? "Không thể đăng nhập.";
  }

  return "Không thể đăng nhập.";
}
