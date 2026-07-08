import "@testing-library/jest-dom/vitest";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render, screen } from "@testing-library/react";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { setAccessToken, clearAccessToken } from "@/lib/http/auth-token";
import { AuthGuard } from "./auth-guard";
import { AuthProvider } from "./auth-context";

const replace = vi.fn();

vi.mock("next/navigation", () => ({
  usePathname: () => "/dashboard",
  useRouter: () => ({
    replace,
  }),
}));

vi.mock("./auth-api", () => ({
  getCurrentUser: async () => ({
    id: "user-1",
    email: "admin@test.local",
    fullName: "Quản trị viên Demo",
    roles: ["Admin"],
    permissions: [],
  }),
  login: vi.fn(),
}));

describe("AuthGuard", () => {
  beforeEach(() => {
    clearAccessToken();
    replace.mockClear();
  });

  it("giữ trang dashboard khi đã có token và tải lại thông tin người dùng", async () => {
    setAccessToken("jwt-demo-token");
    const queryClient = new QueryClient({
      defaultOptions: {
        queries: {
          retry: false,
        },
      },
    });

    render(
      <QueryClientProvider client={queryClient}>
        <AuthProvider>
          <AuthGuard>
            <div>Dashboard được bảo vệ</div>
          </AuthGuard>
        </AuthProvider>
      </QueryClientProvider>,
    );

    expect(await screen.findByText("Dashboard được bảo vệ")).toBeVisible();
    expect(replace).not.toHaveBeenCalledWith("/login?returnUrl=%2Fdashboard");
  });
});
