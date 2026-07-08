"use client";

import { useEffect, useState } from "react";
import { apiClient } from "@/lib/http/api-client";
import { useQueryClient } from "@tanstack/react-query";

interface MakeupModalProps {
  occurrenceId: string;
  className: string;
  date: string;
  onClose: () => void;
}

interface AbsentStudent {
  studentId: string;
  studentName: string;
  absentOccurrenceId: string;
  notes: string | null;
}

interface CalendarOccurrence {
  id: string;
  className: string;
  date: string;
  startTime: string;
  endTime: string;
  status: string;
}

export function MakeupModal({ occurrenceId, className, date, onClose }: MakeupModalProps) {
  const queryClient = useQueryClient();
  const [activeTab, setActiveTab] = useState<"cancel" | "makeup">("cancel");
  
  // Cancel form state
  const [cancelAction, setCancelAction] = useState<"Tạo học bù" | "Trừ học phí">("Tạo học bù");
  const [isCancelling, setIsCancelling] = useState(false);

  // Makeup form state
  const [absentStudents, setAbsentStudents] = useState<AbsentStudent[]>([]);
  const [selectedStudentId, setSelectedStudentId] = useState<string>("");
  const [futureOccurrences, setFutureOccurrences] = useState<CalendarOccurrence[]>([]);
  const [selectedMakeupId, setSelectedMakeupId] = useState<string>("");
  const [isRegisteringMakeup, setIsRegisteringMakeup] = useState(false);

  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Load eligible absent students and future calendar occurrences
  useEffect(() => {
    // 1. Fetch eligible absent students
    apiClient.get(`/schedules/occurrence/${occurrenceId}/eligible-absent-students`)
      .then((res) => setAbsentStudents(res.data.data))
      .catch((err) => console.error(err));

    // 2. Fetch future occurrences for makeup choices (next 30 days)
    const today = new Date();
    const nextMonth = new Date();
    nextMonth.setDate(today.getDate() + 30);
    
    const startStr = today.toISOString().split("T")[0];
    const endStr = nextMonth.toISOString().split("T")[0];

    apiClient.get(`/schedules/calendar?startDate=${startStr}&endDate=${endStr}`)
      .then((res) => {
        // Only show future scheduled occurrences that are not cancelled or done
        const list = (res.data.data as CalendarOccurrence[]).filter(
          (o) => o.id !== occurrenceId && o.status === "Đã lên lịch"
        );
        setFutureOccurrences(list);
      })
      .catch((err) => console.error(err));
  }, [occurrenceId]);

  const handleCancelSession = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      setIsCancelling(true);
      setSuccessMessage(null);
      setErrorMessage(null);

      await apiClient.post(`/schedules/occurrence/${occurrenceId}/cancel`, {
        action: cancelAction,
      });

      setSuccessMessage("Hủy buổi học thành công!");
      queryClient.invalidateQueries({ queryKey: ["calendar"] });
      setTimeout(onClose, 1000);
    } catch (err: any) {
      setErrorMessage(err.response?.data?.message ?? "Đã xảy ra lỗi khi hủy buổi học.");
    } finally {
      setIsCancelling(false);
    }
  };

  const handleScheduleMakeup = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedStudentId || !selectedMakeupId) {
      setErrorMessage("Vui lòng chọn đầy đủ học sinh và buổi học bù.");
      return;
    }

    try {
      setIsRegisteringMakeup(true);
      setSuccessMessage(null);
      setErrorMessage(null);

      await apiClient.post("/schedules/individual-makeup", {
        studentId: selectedStudentId,
        absentOccurrenceId: occurrenceId,
        makeupOccurrenceId: selectedMakeupId,
      });

      setSuccessMessage("Gán lịch học bù cá nhân thành công!");
      queryClient.invalidateQueries({ queryKey: ["calendar"] });
      
      // Update local list
      setAbsentStudents((prev) => prev.filter((s) => s.studentId !== selectedStudentId));
      setSelectedStudentId("");
      setSelectedMakeupId("");
      
      setTimeout(onClose, 1000);
    } catch (err: any) {
      setErrorMessage(err.response?.data?.message ?? "Đã xảy ra lỗi khi đăng ký học bù.");
    } finally {
      setIsRegisteringMakeup(false);
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-lg rounded-lg border border-border bg-surface p-6 shadow-lg animate-in fade-in zoom-in duration-200">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-border pb-3 mb-4">
          <div>
            <h2 className="text-lg font-bold text-foreground">Học bù & Hủy buổi</h2>
            <p className="text-xs text-muted-foreground mt-0.5">
              Lớp: {className} | Ngày: {date}
            </p>
          </div>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground text-sm cursor-pointer">
            Đóng
          </button>
        </div>

        {/* Tab Headers */}
        <div className="flex border-b border-border mb-4">
          <button
            onClick={() => { setActiveTab("cancel"); setErrorMessage(null); setSuccessMessage(null); }}
            className={`flex-1 pb-2.5 text-sm font-semibold border-b-2 cursor-pointer transition ${
              activeTab === "cancel" ? "border-primary text-primary" : "border-transparent text-muted-foreground"
            }`}
          >
            Hủy buổi học
          </button>
          <button
            onClick={() => { setActiveTab("makeup"); setErrorMessage(null); setSuccessMessage(null); }}
            className={`flex-1 pb-2.5 text-sm font-semibold border-b-2 cursor-pointer transition ${
              activeTab === "makeup" ? "border-primary text-primary" : "border-transparent text-muted-foreground"
            }`}
          >
            Học bù cá nhân
          </button>
        </div>

        {successMessage && (
          <div className="mb-4 p-3 rounded-md bg-green-50 text-xs text-green-700 border border-green-200">
            {successMessage}
          </div>
        )}

        {errorMessage && (
          <div className="mb-4 p-3 rounded-md bg-red-50 text-xs text-red-700 border border-red-200">
            {errorMessage}
          </div>
        )}

        {/* Tab Contents */}
        {activeTab === "cancel" ? (
          <form onSubmit={handleCancelSession} className="space-y-4">
            <div>
              <label className="block text-xs font-semibold mb-2">Chọn phương án xử lý học phí</label>
              <div className="space-y-3">
                <label className="flex items-start gap-3 cursor-pointer">
                  <input
                    type="radio"
                    name="cancelAction"
                    value="Tạo học bù"
                    checked={cancelAction === "Tạo học bù"}
                    onChange={() => setCancelAction("Tạo học bù")}
                    className="mt-1 h-4 w-4 text-primary focus:ring-primary border-border"
                  />
                  <div>
                    <span className="text-sm font-medium">Tạo học bù cả lớp</span>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      Lên lịch một buổi dạy bù thay thế. Không ảnh hưởng đến học phí hàng tháng.
                    </p>
                  </div>
                </label>

                <label className="flex items-start gap-3 cursor-pointer">
                  <input
                    type="radio"
                    name="cancelAction"
                    value="Trừ học phí"
                    checked={cancelAction === "Trừ học phí"}
                    onChange={() => setCancelAction("Trừ học phí")}
                    className="mt-1 h-4 w-4 text-primary focus:ring-primary border-border"
                  />
                  <div>
                    <span className="text-sm font-medium">Trừ tiền học phí tháng sau</span>
                    <p className="text-xs text-muted-foreground mt-0.5">
                      Không tổ chức học bù. Tiền học của buổi này sẽ tự động được cấn trừ ở phiếu học phí tiếp theo.
                    </p>
                  </div>
                </label>
              </div>
            </div>

            <div className="flex justify-end gap-3 pt-4 border-t border-border mt-6">
              <button
                type="button"
                onClick={onClose}
                className="h-9 rounded-md border border-border bg-surface px-4 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
              >
                Hủy bỏ
              </button>
              <button
                type="submit"
                disabled={isCancelling}
                className="h-9 rounded-md bg-red-600 text-white px-4 text-xs font-semibold hover:bg-red-700 transition shadow disabled:opacity-50 cursor-pointer"
              >
                {isCancelling ? "Đang hủy..." : "Xác nhận hủy buổi"}
              </button>
            </div>
          </form>
        ) : (
          <form onSubmit={handleScheduleMakeup} className="space-y-4">
            {/* Absent student selection */}
            <div>
              <label className="block text-xs font-semibold mb-1" htmlFor="studentId">
                Học sinh nghỉ phép <span className="text-red-500">*</span>
              </label>
              <select
                id="studentId"
                value={selectedStudentId}
                onChange={(e) => setSelectedStudentId(e.target.value)}
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                required
              >
                <option value="">-- Chọn học sinh nghỉ phép --</option>
                {absentStudents.map((s) => (
                  <option key={s.studentId} value={s.studentId}>
                    {s.studentName} ({s.notes || "Vắng có phép"})
                  </option>
                ))}
              </select>
            </div>

            {/* Makeup occurrence selection */}
            <div>
              <label className="block text-xs font-semibold mb-1" htmlFor="makeupId">
                Chọn buổi ghép học bù <span className="text-red-500">*</span>
              </label>
              <select
                id="makeupId"
                value={selectedMakeupId}
                onChange={(e) => setSelectedMakeupId(e.target.value)}
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                required
              >
                <option value="">-- Chọn buổi ghép học bù --</option>
                {futureOccurrences.map((o) => (
                  <option key={o.id} value={o.id}>
                    {o.className} | {o.date} ({o.startTime.substring(0, 5)} - {o.endTime.substring(0, 5)})
                  </option>
                ))}
              </select>
            </div>

            <div className="flex justify-end gap-3 pt-4 border-t border-border mt-6">
              <button
                type="button"
                onClick={onClose}
                className="h-9 rounded-md border border-border bg-surface px-4 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
              >
                Hủy bỏ
              </button>
              <button
                type="submit"
                disabled={isRegisteringMakeup || absentStudents.length === 0}
                className="h-9 rounded-md bg-primary text-primary-foreground px-4 text-xs font-semibold hover:bg-primary/90 transition shadow disabled:opacity-50 cursor-pointer"
              >
                {isRegisteringMakeup ? "Đang xếp..." : "Xác nhận xếp học bù"}
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
