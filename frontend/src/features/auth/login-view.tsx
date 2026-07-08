"use client";

import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useState } from "react";
import { useForm } from "react-hook-form";
import {
  getAuthErrorMessage,
  useAuth,
} from "./auth-context";
import type { LoginFormValues } from "./types";

export function LoginView() {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { isAuthenticated, isCheckingAuth, login } = useAuth();
  const [apiError, setApiError] = useState<string | null>(null);
  const returnUrl = searchParams.get("returnUrl") ?? "/dashboard";

  const {
    formState: { errors, isSubmitting },
    handleSubmit,
    register,
  } = useForm<LoginFormValues>({
    defaultValues: {
      email: "",
      password: "",
    },
  });

  useEffect(() => {
    if (!isCheckingAuth && isAuthenticated) {
      router.replace(returnUrl);
    }
  }, [isAuthenticated, isCheckingAuth, returnUrl, router]);

  async function onSubmit(values: LoginFormValues) {
    setApiError(null);

    try {
      await login(values);
      router.replace(returnUrl);
    } catch (error) {
      setApiError(getAuthErrorMessage(error));
    }
  }

  return (
    <main className="min-h-screen bg-background px-4 py-8 text-foreground sm:px-6 lg:px-8">
      <div className="mx-auto grid min-h-[calc(100vh-4rem)] w-full max-w-6xl items-center gap-10 lg:grid-cols-[1.05fr_0.95fr]">
        <section className="hidden lg:block">
          <p className="text-sm font-medium text-primary">Education Center CRM</p>
          <h1 className="mt-4 max-w-xl text-4xl font-semibold leading-tight tracking-normal">
            Quản lý trung tâm giáo dục rõ ràng, gọn và đúng vai trò.
          </h1>
          <p className="mt-5 max-w-lg text-base leading-7 text-muted-foreground">
            Admin, nhân viên, giáo viên, phụ huynh và học sinh đăng nhập vào
            đúng không gian làm việc của mình.
          </p>
          <div className="mt-8 grid max-w-xl grid-cols-2 gap-3">
            {["Tổng quan", "Lịch học", "Học phí", "Thông báo"].map((item) => (
              <div
                className="rounded-lg border border-border bg-surface px-4 py-3 text-sm font-medium"
                key={item}
              >
                {item}
              </div>
            ))}
          </div>
        </section>

        <section className="mx-auto w-full max-w-md rounded-lg border border-border bg-surface p-6 shadow-sm sm:p-8">
          <div>
            <p className="text-sm font-medium text-primary lg:hidden">
              Education Center CRM
            </p>
            <h2 className="mt-2 text-2xl font-semibold tracking-normal">
              Đăng nhập
            </h2>
            <p className="mt-2 text-sm leading-6 text-muted-foreground">
              Nhập email và mật khẩu để tiếp tục.
            </p>
          </div>

          <form className="mt-8 space-y-5" onSubmit={handleSubmit(onSubmit)}>
            <div>
              <label className="text-sm font-medium" htmlFor="email">
                Email
              </label>
              <input
                autoComplete="email"
                className="mt-2 h-11 w-full rounded-md border border-border bg-white px-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
                id="email"
                type="email"
                {...register("email", {
                  required: "Vui lòng nhập email.",
                  pattern: {
                    value: /^\S+@\S+\.\S+$/,
                    message: "Email không hợp lệ.",
                  },
                })}
              />
              {errors.email ? (
                <p className="mt-2 text-sm font-medium text-red-600">
                  {errors.email.message}
                </p>
              ) : null}
            </div>

            <div>
              <label className="text-sm font-medium" htmlFor="password">
                Mật khẩu
              </label>
              <input
                autoComplete="current-password"
                className="mt-2 h-11 w-full rounded-md border border-border bg-white px-3 text-sm outline-none transition focus:border-primary focus:ring-2 focus:ring-primary/15"
                id="password"
                type="password"
                {...register("password", {
                  required: "Vui lòng nhập mật khẩu.",
                })}
              />
              {errors.password ? (
                <p className="mt-2 text-sm font-medium text-red-600">
                  {errors.password.message}
                </p>
              ) : null}
            </div>

            {apiError ? (
              <div className="rounded-md border border-red-200 bg-red-50 px-3 py-2 text-sm font-medium text-red-700">
                {apiError}
              </div>
            ) : null}

            <button
              className="h-11 w-full rounded-md bg-primary px-4 text-sm font-semibold text-primary-foreground transition hover:bg-primary/90 disabled:cursor-not-allowed disabled:opacity-70"
              disabled={isSubmitting}
              type="submit"
            >
              {isSubmitting ? "Đang đăng nhập..." : "Đăng nhập"}
            </button>
          </form>
        </section>
      </div>
    </main>
  );
}
