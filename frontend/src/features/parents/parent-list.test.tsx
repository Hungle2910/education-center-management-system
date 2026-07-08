import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ParentList } from "./parent-list";

vi.mock("../students/student-api", () => ({
  useParents: () => ({
    data: [
      {
        id: "p1",
        fullName: "Nguyễn Văn A",
        phoneNumber: "84909123456",
        email: "vana@example.com",
        zaloLink: "https://zalo.me/84909123456",
        students: [
          { id: "s1", fullName: "Nguyễn Con A", relationship: "Con" }
        ]
      }
    ],
    isLoading: false,
    error: null
  }),
  useCreateParent: () => ({}),
  useUpdateParent: () => ({}),
}));

vi.mock("@/features/admissions/admissions-api", () => ({
  useCareLogs: () => ({ data: [] }),
  useCreateCareLog: () => ({}),
}));

describe("ParentList", () => {
  it("hiển thị danh sách phụ huynh với Zalo Link chuẩn", () => {
    render(<ParentList />);

    expect(screen.getByText("Nguyễn Văn A")).toBeInTheDocument();
    expect(screen.getByText("84909123456")).toBeInTheDocument();
    expect(screen.getByText("Nguyễn Con A (Con)")).toBeInTheDocument();
    
    const zaloLink = screen.getByRole("link", { name: "Chat Zalo" });
    expect(zaloLink).toHaveAttribute("href", "https://zalo.me/84909123456");
  });
});
