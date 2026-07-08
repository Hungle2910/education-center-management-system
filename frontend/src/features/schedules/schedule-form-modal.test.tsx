import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { ScheduleFormModal } from "./schedule-form-modal";

vi.mock("./schedule-api", () => ({
  useCreateSchedule: () => ({}),
  checkConflicts: () => Promise.resolve({
    hasConflict: true,
    message: "Không thể lưu lịch. Phòng học đã có lịch vào thời gian này."
  }),
}));

vi.mock("@/features/classes/class-api", () => ({
  useTeachers: () => ({ data: [] }),
  useClasses: () => ({
    data: [
      { id: "c1", name: "Lớp Toán 9" }
    ]
  }),
}));

vi.mock("@/lib/http/api-client", () => ({
  apiClient: {
    get: vi.fn().mockResolvedValue({ data: { data: [{ id: "r1", name: "Phòng 101", capacity: 15 }] } }),
  },
}));

describe("ScheduleFormModal", () => {
  it("hiển thị cảnh báo khi xếp lịch trùng phòng", async () => {
    const onClose = vi.fn();
    render(<ScheduleFormModal onClose={onClose} />);

    // Select class
    const classSelect = screen.getByLabelText("Chọn Lớp học *");
    await userEvent.selectOptions(classSelect, "c1");

    // Wait for room option to render and select it
    const roomSelect = await screen.findByLabelText("Phòng học *");
    const roomOption = await screen.findByRole("option", { name: "Phòng 101 (Sức chứa: 15 HS)" });
    await userEvent.selectOptions(roomSelect, "r1");

    // Submit form
    await userEvent.click(screen.getByRole("button", { name: "Xác nhận xếp lịch" }));

    // Expect conflict error warning
    expect(await screen.findByText("Không thể lưu lịch. Phòng học đã có lịch vào thời gian này.")).toBeInTheDocument();
  });
});
