import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";
import { LoginView } from "./login-view";

const replace = vi.fn();
const login = vi.fn();

vi.mock("next/navigation", () => ({
  useRouter: () => ({
    replace,
  }),
  useSearchParams: () => ({
    get: () => null,
  }),
}));

vi.mock("./auth-context", () => ({
  getAuthErrorMessage: () => "Tài khoản hoặc mật khẩu không chính xác.",
  useAuth: () => ({
    isAuthenticated: false,
    isCheckingAuth: false,
    login,
  }),
}));

describe("LoginView", () => {
  beforeEach(() => {
    replace.mockClear();
    login.mockClear();
  });

  it("hiển thị cảnh báo đỏ khi bỏ trống email và mật khẩu", async () => {
    render(<LoginView />);

    await userEvent.click(screen.getByRole("button", { name: "Đăng nhập" }));

    expect(await screen.findByText("Vui lòng nhập email.")).toBeVisible();
    expect(screen.getByText("Vui lòng nhập mật khẩu.")).toBeVisible();
    expect(login).not.toHaveBeenCalled();
  });
});
