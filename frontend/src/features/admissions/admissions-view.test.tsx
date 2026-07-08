import "@testing-library/jest-dom/vitest";
import { render, screen, fireEvent } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { AdmissionsView } from "./admissions-view";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

// Mock Auth Context
vi.mock("../auth/auth-context", () => ({
  useAuth: () => ({
    user: { fullName: "Quản trị viên", roles: ["Admin"] },
  }),
}));

// Mock Admissions API Hooks
vi.mock("./admissions-api", () => ({
  useLeads: () => ({
    data: [
      {
        id: "l1",
        studentName: "Nguyễn Văn Tiềm Năng",
        parentName: "Phụ Huynh Tiềm Năng",
        parentPhone: "0909123456",
        source: "Facebook",
        status: "Mới quan tâm",
        createdAtUtc: "2026-07-07T12:00:00Z",
      },
    ],
    isLoading: false,
  }),
  useTrialSessions: () => ({
    data: [
      {
        id: "ts1",
        leadId: "l1",
        studentName: "Nguyễn Văn Tiềm Năng",
        classId: "c1",
        className: "Toán nâng cao 9",
        trialDate: "2026-07-10",
        teacherName: "Thầy Hoàng",
        feedback: "Học tốt",
        result: "Đăng ký",
        createdAtUtc: "2026-07-07T12:00:00Z",
      },
    ],
    isLoading: false,
  }),
  useCreateLead: () => ({}),
  useUpdateLead: () => ({}),
  useConvertLead: () => ({}),
  useScheduleTrial: () => ({}),
  useEvaluateTrial: () => ({}),
  useCareLogs: () => ({ data: [] }),
  useCreateCareLog: () => ({}),
}));

// Mock Class APIs
vi.mock("@/features/classes/class-api", () => ({
  useClasses: () => ({ data: [] }),
  useTeachers: () => ({ data: [] }),
}));

// Mock Dashboard Layout
vi.mock("@/features/dashboard/dashboard-layout", () => ({
  DashboardLayout: ({ children }: { children: React.ReactNode }) => <div data-testid="dashboard-layout">{children}</div>,
}));

describe("AdmissionsView", () => {
  it("hiển thị danh sách học sinh tiềm năng và chuyển sang tab lịch học thử", () => {
    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <AdmissionsView />
      </QueryClientProvider>
    );

    // Verify lead displays
    expect(screen.getByText("Nguyễn Văn Tiềm Năng")).toBeInTheDocument();
    expect(screen.getByText("Phụ Huynh Tiềm Năng", { exact: false })).toBeInTheDocument();
    expect(screen.getByText("0909123456")).toBeInTheDocument();
    expect(screen.getByText("Facebook")).toBeInTheDocument();

    // Click trial sessions tab
    const trialsTab = screen.getByText("Lịch học thử");
    fireEvent.click(trialsTab);

    // Verify trial session data displays
    expect(screen.getByText("Toán nâng cao 9")).toBeInTheDocument();
    expect(screen.getByText("2026-07-10")).toBeInTheDocument();
    expect(screen.getByText("Thầy Hoàng")).toBeInTheDocument();
  });
});
