import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { DashboardView } from "./dashboard-view";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

// Mock Auth Context
vi.mock("./auth-context", () => ({
  useAuth: () => ({
    user: { fullName: "Nguyễn Văn Admin", roles: ["Admin"], email: "admin@test.local" },
    logout: vi.fn(),
  }),
}));

// Mock Dashboard API Hooks
vi.mock("../dashboard/dashboard-api", () => ({
  useAdminOverview: () => ({
    data: {
      totalTuitionRevenue: 55000000,
      activeStudentCount: 120,
      activeClassCount: 8,
      recruitingClassCount: 2,
      scheduleConflictCount: 1,
    },
    isLoading: false,
  }),
  useAdminOperations: () => ({
    data: {
      todaySchedules: [
        {
          occurrenceId: "o1",
          classId: "c1",
          className: "Toán nâng cao 9",
          date: "2026-07-07",
          startTime: "08:00:00",
          endTime: "09:30:00",
          roomId: "r1",
          roomName: "Phòng 101",
          teacherName: "Thầy Hoàng",
          status: "Đã học",
        },
      ],
      trialSessionsTodayCount: 3,
      pendingPaymentInvoices: [
        {
          invoiceId: "i1",
          studentId: "s1",
          studentName: "Học sinh A",
          classId: "c1",
          className: "Toán nâng cao 9",
          month: "2026-07",
          totalAmount: 1500000,
          status: "Chờ xác nhận",
        },
      ],
    },
    isLoading: false,
  }),
}));

// Mock Class API
vi.mock("@/features/classes/class-api", () => ({
  useClasses: () => ({ data: [] }),
}));

// Mock Tuition API
vi.mock("@/features/tuition/tuition-api", () => ({
  useConfirmPayment: () => ({
    mutateAsync: vi.fn(),
    isPending: false,
  }),
}));

// Mock Dashboard Layout
vi.mock("@/features/dashboard/dashboard-layout", () => ({
  DashboardLayout: ({ children }: { children: React.ReactNode }) => <div data-testid="dashboard-layout">{children}</div>,
}));

// Mock Auth Guard
vi.mock("./auth-guard", () => ({
  AuthGuard: ({ children }: { children: React.ReactNode }) => <>{children}</>,
}));

describe("DashboardView", () => {
  it("hiển thị dữ liệu tổng quan, các thẻ chỉ số và cảnh báo trùng lịch", () => {
    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <DashboardView />
      </QueryClientProvider>
    );

    // Verify stats
    expect(screen.getByText("55.000.000 đ")).toBeInTheDocument();
    expect(screen.getByText("120")).toBeInTheDocument();
    expect(screen.getByText("8")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();

    // Verify schedule conflict warning
    expect(screen.getByText(/Cảnh báo: Có 1 lớp đang bị trùng lịch/)).toBeInTheDocument();

    // Verify today schedules table
    expect(screen.getAllByText("Toán nâng cao 9")[0]).toBeInTheDocument();
    expect(screen.getByText("Phòng 101")).toBeInTheDocument();

    // Verify pending invoice
    expect(screen.getByText("Học sinh A")).toBeInTheDocument();
    expect(screen.getByText("1.500.000 đ")).toBeInTheDocument();
  });
});
