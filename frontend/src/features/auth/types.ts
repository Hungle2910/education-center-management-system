export type ApiResponse<T> = {
  success: boolean;
  message: string;
  data: T;
  errors?: string[] | null;
  traceId?: string | null;
};

export type AuthUser = {
  id: string;
  email: string;
  fullName: string;
  roles: string[];
  permissions: string[];
};

export type LoginResponse = {
  accessToken: string;
  expiresAtUtc: string;
  user: AuthUser;
};

export type LoginFormValues = {
  email: string;
  password: string;
};
