import "@testing-library/jest-dom/vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import { MakeupModal } from "./makeup-modal";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";

vi.mock("@/lib/http/api-client", () => ({
  apiClient: {
    get: vi.fn().mockImplementation((url: string) => {
      if (url.includes("eligible-absent-students")) {
        return Promise.resolve({
          data: {
            data: [
              { studentId: "s1", studentName: "Học sinh A", absentOccurrenceId: "occ1", notes: "Nghỉ có phép" }
            ]
          }
        });
      }
      return Promise.resolve({ data: { data: [] } });
    }),
    post: vi.fn().mockResolvedValue({ data: {} })
  }
}));

describe("MakeupModal", () => {
  it("hiển thị các tab hủy buổi và học bù cá nhân", async () => {
    const queryClient = new QueryClient();
    render(
      <QueryClientProvider client={queryClient}>
        <MakeupModal
          occurrenceId="occ1"
          className="Toán nâng cao 9"
          date="2026-07-07"
          onClose={vi.fn()}
        />
      </QueryClientProvider>
    );

    // Cancel session form is default tab
    expect(screen.getByText("Tạo học bù cả lớp")).toBeInTheDocument();

    // Switch to Individual Makeup tab
    const makeupTab = screen.getByRole("button", { name: "Học bù cá nhân" });
    await userEvent.click(makeupTab);

    // Verify option label in select dropdown
    expect(await screen.findByRole("option", { name: "Học sinh A (Nghỉ có phép)" })).toBeInTheDocument();
  });
});

