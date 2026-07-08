import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { AttendanceSheetModal } from "./attendance-sheet-modal";

vi.mock("./attendance-api", () => ({
  useOccurrenceAttendance: () => ({
    data: {
      occurrenceId: "occ1",
      className: "Toán nâng cao 9",
      date: "2026-07-07",
      startTime: "08:00",
      endTime: "09:30",
      students: [
        { studentId: "s1", studentName: "Nguyễn Văn A", status: "Có mặt", notes: null }
      ]
    },
    isLoading: false,
    error: null
  }),
  useSubmitAttendance: () => ({
    mutateAsync: vi.fn().mockResolvedValue({}),
    isPending: false
  })
}));

describe("AttendanceSheetModal", () => {
  it("hiển thị danh sách học sinh và cho phép chọn trạng thái", async () => {
    render(<AttendanceSheetModal occurrenceId="occ1" onClose={vi.fn()} />);

    // Verify student name is rendered
    expect(screen.getByText("Nguyễn Văn A")).toBeInTheDocument();

    // Verify status buttons are rendered
    const absentButton = screen.getByRole("button", { name: "Vắng có phép" });
    expect(absentButton).toBeInTheDocument();

    // Click on status button
    await userEvent.click(absentButton);
  });
});
