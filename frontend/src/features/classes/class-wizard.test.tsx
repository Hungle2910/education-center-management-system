import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ClassWizard } from "./class-wizard";

vi.mock("./class-api", () => ({
  useTeachers: () => ({
    data: [
      { id: "t1", fullName: "Giáo viên Test", subject: "Toán", isActive: true }
    ],
    isLoading: false,
    error: null
  }),
  useCreateClass: () => ({}),
  useUpdateClass: () => ({}),
}));

describe("ClassWizard", () => {
  it("hiển thị các bước của Wizard tuần tự", async () => {
    const onClose = vi.fn();
    render(<ClassWizard onClose={onClose} />);

    // Step 1: Basic Info
    expect(screen.getByText("1. Thông tin lớp")).toBeInTheDocument();
    expect(screen.getByPlaceholderText("Nhập tên lớp học")).toBeInTheDocument();

    // Try next without filling name -> error shown
    await userEvent.click(screen.getByRole("button", { name: "Tiếp tục" }));
    expect(screen.getByText("Vui lòng nhập tên lớp học.")).toBeInTheDocument();

    // Fill name and continue
    await userEvent.type(screen.getByPlaceholderText("Nhập tên lớp học"), "Lớp 9A");
    await userEvent.click(screen.getByRole("button", { name: "Tiếp tục" }));

    // Step 2: Select Teacher
    expect(screen.getByText("2. Giáo viên")).toBeInTheDocument();
    expect(screen.getByText("Giáo viên Test")).toBeInTheDocument();
    
    // Choose teacher and continue
    await userEvent.click(screen.getByText("Giáo viên Test"));
    await userEvent.click(screen.getByRole("button", { name: "Tiếp tục" }));

    // Step 3: Confirmation
    expect(screen.getByText("3. Xác nhận")).toBeInTheDocument();
    expect(screen.getByText("Giáo viên Test")).toBeInTheDocument();
    expect(screen.getByText("Lớp 9A")).toBeInTheDocument();
  });
});
