import "@testing-library/jest-dom/vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { ReportsView } from "./reports-view";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

// Mock Reports API Hooks
vi.mock("./reports-api", () => ({
  useTuitionReport: () => ({
    data: {
      totalCollected: 50000000,
      totalUnpaid: 10000000,
      totalOverdue: 2000000,
      revenueByClass: [
        { classId: "c1", className: "Toán 9A", revenue: 30000000 },
      ],
      revenueByMonth: [
        { month: "2026-07", revenue: 50000000 },
      ],
    },
    isLoading: false,
  }),
  useClassReport: () => ({
    data: [
      {
        classId: "c1",
        className: "Toán 9A",
        activeStudentCount: 4,
        targetStudentCount: 10,
        status: "Đang học",
        isAtRiskOfLoss: true, // 4 < target/min
      },
    ],
    isLoading: false,
  }),
  useTeacherReport: () => ({
    data: [
      {
        teacherId: "t1",
        teacherName: "Thầy Hoàng",
        completedLessonsCount: 10,
        cancelledLessonsCount: 1,
        makeupLessonsCount: 0,
        projectedSalary: 3000000,
        paidSalary: 3000000,
      },
    ],
    isLoading: false,
  }),
}));

describe("ReportsView", () => {
  it("hiển thị báo cáo doanh thu mặc định và có thể chuyển đổi các tab lớp học, giáo viên", async () => {
    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <ReportsView />
      </QueryClientProvider>
    );

    // Default Tab: Tuition & Revenue
    expect(screen.getAllByText("50.000.000 đ")[0]).toBeInTheDocument();
    expect(screen.getByText("10.000.000 đ")).toBeInTheDocument();
    expect(screen.getByText("Toán 9A")).toBeInTheDocument();

    // Switch to Classes Tab
    const classesTab = screen.getByText("Hiệu suất lớp học");
    fireEvent.click(classesTab);

    // Verify Classes Tab data
    expect(screen.getByText("4 học sinh")).toBeInTheDocument();
    expect(screen.getByText("10 học sinh")).toBeInTheDocument();
    expect(screen.getByText("⚠️ Nguy cơ lỗ")).toBeInTheDocument();

    // Switch to Teachers Tab
    const teachersTab = screen.getByText("Lương & Buổi dạy");
    fireEvent.click(teachersTab);

    // Verify Teachers Tab data
    expect(screen.getByText("Thầy Hoàng")).toBeInTheDocument();
    expect(screen.getByText("10 buổi")).toBeInTheDocument();
    expect(screen.getAllByText("3.000.000 đ")[0]).toBeInTheDocument();
  });
});
