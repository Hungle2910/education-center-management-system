"use client";

import { useEffect, useState } from "react";
import { useOccurrenceAttendance, useSubmitAttendance } from "./attendance-api";
import { StudentAttendance } from "./attendance-types";

interface AttendanceSheetModalProps {
  occurrenceId: string;
  onClose: () => void;
}

export function AttendanceSheetModal({ occurrenceId, onClose }: AttendanceSheetModalProps) {
  const { data: attendance, isLoading, error } = useOccurrenceAttendance(occurrenceId);
  const submitAttendance = useSubmitAttendance();
  const [students, setStudents] = useState<StudentAttendance[]>([]);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Sync state when data is loaded
  useEffect(() => {
    if (attendance) {
      setStudents(attendance.students);
    }
  }, [attendance]);

  const handleStatusChange = (studentId: string, status: string) => {
    setStudents((prev) =>
      prev.map((s) => (s.studentId === studentId ? { ...s, status } : s))
    );
  };

  const handleNotesChange = (studentId: string, notes: string) => {
    setStudents((prev) =>
      prev.map((s) => (s.studentId === studentId ? { ...s, notes: notes || null } : s))
    );
  };

  const handleSave = async () => {
    try {
      setSuccessMessage(null);
      setErrorMessage(null);
      await submitAttendance.mutateAsync({
        occurrenceId,
        students,
      });
      setSuccessMessage("Điểm danh thành công!");
      setTimeout(() => {
        onClose();
      }, 1000);
    } catch (err: any) {
      setErrorMessage(err.response?.data?.message ?? "Đã xảy ra lỗi khi điểm danh.");
    }
  };

  if (isLoading) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
        <div className="bg-surface rounded-lg p-6 max-w-sm w-full text-center">
          <p className="text-sm text-muted-foreground">Đang tải danh sách điểm danh...</p>
        </div>
      </div>
    );
  }

  if (error || !attendance) {
    return (
      <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
        <div className="bg-surface rounded-lg p-6 max-w-sm w-full text-center">
          <p className="text-sm text-red-600 font-semibold">Không tìm thấy thông tin buổi học.</p>
          <button onClick={onClose} className="mt-4 px-4 py-2 bg-primary text-primary-foreground rounded-md text-xs font-semibold">
            Đóng
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-4xl rounded-lg border border-border bg-surface p-6 shadow-lg animate-in fade-in zoom-in duration-200 flex flex-col max-h-[90vh]">
        {/* Header */}
        <div className="flex items-center justify-between border-b border-border pb-3 mb-4">
          <div>
            <h2 className="text-lg font-bold text-foreground">Bảng Điểm Danh Lớp: {attendance.className}</h2>
            <p className="text-xs text-muted-foreground mt-0.5">
              📅 Ngày: {attendance.date} | ⏰ Giờ: {attendance.startTime} - {attendance.endTime}
            </p>
          </div>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground text-sm cursor-pointer">
            Đóng
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

        {/* Student Table */}
        <div className="flex-1 overflow-y-auto mb-4 border border-border rounded-lg bg-background">
          <table className="min-w-full divide-y divide-border text-sm">
            <thead className="bg-slate-50 text-left font-semibold text-muted-foreground text-xs uppercase">
              <tr>
                <th className="px-4 py-3">Học sinh</th>
                <th className="px-4 py-3">Trạng thái điểm danh</th>
                <th className="px-4 py-3">Nhận xét / Lý do vắng</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-border">
              {students.map((student) => (
                <tr key={student.studentId} className="hover:bg-slate-50/30 transition-colors">
                  <td className="px-4 py-3 font-semibold">{student.studentName}</td>
                  <td className="px-4 py-3">
                    <div className="flex flex-wrap items-center gap-1.5">
                      {["Có mặt", "Vắng có phép", "Vắng không phép", "Đi trễ"].map((status) => {
                        const isSelected = student.status === status;
                        let badgeStyle = "border-border text-muted-foreground hover:bg-slate-50";
                        if (isSelected) {
                          if (status === "Có mặt") badgeStyle = "bg-green-50 text-green-700 border-green-200";
                          else if (status === "Vắng có phép") badgeStyle = "bg-yellow-50 text-yellow-700 border-yellow-200";
                          else if (status === "Vắng không phép") badgeStyle = "bg-red-50 text-red-700 border-red-200";
                          else if (status === "Đi trễ") badgeStyle = "bg-blue-50 text-blue-700 border-blue-200";
                        }

                        return (
                          <button
                            key={status}
                            type="button"
                            onClick={() => handleStatusChange(student.studentId, status)}
                            className={`px-2 py-1 text-xs rounded border font-medium cursor-pointer transition ${badgeStyle}`}
                          >
                            {status}
                          </button>
                        );
                      })}
                    </div>
                  </td>
                  <td className="px-4 py-3">
                    <input
                      type="text"
                      placeholder="Ghi chú nhận xét..."
                      value={student.notes ?? ""}
                      onChange={(e) => handleNotesChange(student.studentId, e.target.value)}
                      className="w-full h-8 px-2.5 rounded-md border border-border bg-surface text-xs focus:outline-none focus:ring-1 focus:ring-primary"
                    />
                  </td>
                </tr>
              ))}
              {students.length === 0 && (
                <tr>
                  <td colSpan={3} className="text-center py-6 text-muted-foreground italic">
                    Không có học sinh hoạt động nào trong hệ thống.
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>

        {/* Footer actions */}
        <div className="flex items-center justify-end gap-3 pt-4 border-t border-border">
          <button
            type="button"
            onClick={onClose}
            className="h-9 rounded-md border border-border bg-surface px-4 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
          >
            Hủy bỏ
          </button>
          <button
            type="button"
            onClick={handleSave}
            disabled={submitAttendance.isPending}
            className="h-9 rounded-md bg-primary text-primary-foreground px-4 text-xs font-semibold hover:bg-primary/90 transition shadow disabled:opacity-50 cursor-pointer"
          >
            {submitAttendance.isPending ? "Đang lưu..." : "Xác nhận điểm danh"}
          </button>
        </div>
      </div>
    </div>
  );
}
