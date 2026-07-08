"use client";

import { useState } from "react";
import { useParents } from "@/features/students/student-api";
import { ParentResponse } from "@/features/students/student-types";
import { ParentForm } from "./parent-form";
import { useCareLogs, useCreateCareLog } from "@/features/admissions/admissions-api";

export function ParentList() {
  const { data: parents, isLoading, error } = useParents();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedParent, setSelectedParent] = useState<ParentResponse | null | undefined>(undefined);
  const [careLogParent, setCareLogParent] = useState<ParentResponse | null>(null);
  
  const [contactType, setContactType] = useState("Đã gọi");
  const [careNotes, setCareNotes] = useState("");

  const { data: careLogs } = useCareLogs(careLogParent?.id || undefined);
  const createCareLogMutation = useCreateCareLog();

  const handleCreateCareLog = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!careLogParent || !careNotes.trim()) return;
    await createCareLogMutation.mutateAsync({
      parentId: careLogParent.id,
      contactType,
      notes: careNotes,
    });
    setCareNotes("");
  };

  const filteredParents = parents?.filter((parent) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      parent.fullName.toLowerCase().includes(searchLower) ||
      (parent.email?.toLowerCase() ?? "").includes(searchLower) ||
      parent.phoneNumber.includes(searchLower)
    );
  });

  if (isLoading) {
    return <div className="text-center py-10 text-muted-foreground">Đang tải danh sách phụ huynh...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-600">Đã xảy ra lỗi khi tải dữ liệu.</div>;
  }

  return (
    <div className="space-y-6">
      {selectedParent !== undefined ? (
        <ParentForm
          parent={selectedParent ?? undefined}
          onClose={() => setSelectedParent(undefined)}
        />
      ) : (
        <>
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Phụ huynh</h1>
              <p className="text-sm text-muted-foreground mt-1">
                Quản lý thông tin liên hệ phụ huynh và liên kết học sinh.
              </p>
            </div>
            <button
              onClick={() => setSelectedParent(null)}
              className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow transition-colors hover:bg-primary/90 cursor-pointer"
            >
              Thêm phụ huynh
            </button>
          </div>

          {/* Search */}
          <div className="flex items-center gap-2 max-w-md">
            <input
              type="text"
              placeholder="Tìm theo tên, email, số điện thoại..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
            />
          </div>

          {/* Table */}
          <div className="overflow-x-auto rounded-lg border border-border bg-surface shadow-sm">
            <table className="min-w-full divide-y divide-border text-left text-sm text-foreground">
              <thead className="bg-background text-muted-foreground font-medium text-xs uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-4">Họ và tên</th>
                  <th className="px-6 py-4">Số điện thoại</th>
                  <th className="px-6 py-4">Email</th>
                  <th className="px-6 py-4">Học sinh liên kết</th>
                  <th className="px-6 py-4">Zalo Quick Chat</th>
                  <th className="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {filteredParents && filteredParents.length > 0 ? (
                  filteredParents.map((parent) => (
                    <tr key={parent.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-6 py-4 font-semibold">{parent.fullName}</td>
                      <td className="px-6 py-4">{parent.phoneNumber}</td>
                      <td className="px-6 py-4 text-muted-foreground">{parent.email ?? "--"}</td>
                      <td className="px-6 py-4">
                        <div className="flex flex-wrap gap-1">
                          {parent.students && parent.students.length > 0 ? (
                            parent.students.map((student) => (
                              <span
                                key={student.id}
                                className="inline-flex items-center rounded bg-teal-50 px-2 py-0.5 text-xs font-medium text-teal-700 border border-teal-200"
                              >
                                {student.fullName} ({student.relationship ?? "Con"})
                              </span>
                            ))
                          ) : (
                            <span className="text-xs text-muted-foreground italic">Chưa liên kết</span>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4">
                        <a
                          href={parent.zaloLink}
                          target="_blank"
                          rel="noreferrer"
                          className="inline-flex items-center text-xs font-semibold text-primary hover:underline"
                        >
                          Chat Zalo
                        </a>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <div className="flex justify-end gap-3">
                          <button
                            onClick={() => setCareLogParent(parent)}
                            className="text-teal-600 hover:underline text-sm font-semibold cursor-pointer"
                          >
                            Chăm sóc
                          </button>
                          <button
                            onClick={() => setSelectedParent(parent)}
                            className="text-primary hover:underline text-sm font-semibold cursor-pointer"
                          >
                            Chỉnh sửa
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="px-6 py-10 text-center text-muted-foreground">
                      Không tìm thấy phụ huynh nào.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </>
      )}

      {careLogParent && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
          <div className="w-full max-w-lg bg-surface rounded-xl border border-border p-6 shadow-xl space-y-4">
            <div className="flex justify-between items-center">
              <h3 className="text-base font-bold text-foreground">Nhật ký chăm sóc: {careLogParent.fullName}</h3>
              <button
                onClick={() => setCareLogParent(null)}
                className="text-muted-foreground hover:text-foreground text-base cursor-pointer"
              >
                ✕
              </button>
            </div>

            <div className="max-h-48 overflow-y-auto space-y-2 border border-border rounded p-3 bg-slate-55/30">
              {careLogs && careLogs.length > 0 ? (
                careLogs.map((log) => (
                  <div key={log.id} className="text-xs border-b border-slate-100 pb-2 last:border-none">
                    <div className="flex justify-between text-muted-foreground">
                      <span className="font-semibold text-slate-700">{log.contactType}</span>
                      <span className="font-mono">{new Date(log.loggedAtUtc).toLocaleString("vi-VN")}</span>
                    </div>
                    <p className="mt-1 text-slate-600 leading-relaxed">{log.notes}</p>
                  </div>
                ))
              ) : (
                <p className="text-xs text-muted-foreground text-center py-4 italic">Chưa có nhật ký chăm sóc nào.</p>
              )}
            </div>

            <form onSubmit={handleCreateCareLog} className="space-y-3">
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1">Phương thức chăm sóc</label>
                <select
                  value={contactType}
                  onChange={(e) => setContactType(e.target.value)}
                  className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                >
                  <option value="Đã gọi">Đã gọi</option>
                  <option value="Đã nhắn Zalo">Đã nhắn Zalo</option>
                  <option value="Đã gửi học phí">Đã gửi học phí</option>
                  <option value="Phụ huynh hẹn đóng tiền">Phụ huynh hẹn đóng tiền</option>
                  <option value="Phụ huynh phản hồi lịch học">Phụ huynh phản hồi lịch học</option>
                  <option value="Cần chăm sóc lại">Cần chăm sóc lại</option>
                </select>
              </div>
              <div>
                <label className="block text-xs font-medium text-muted-foreground mb-1">Ghi chú chi tiết tương tác</label>
                <textarea
                  required
                  value={careNotes}
                  onChange={(e) => setCareNotes(e.target.value)}
                  rows={3}
                  placeholder="Nhập nội dung trao đổi, phản hồi của phụ huynh..."
                  className="w-full rounded border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary resize-none"
                />
              </div>
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setCareLogParent(null)}
                  className="rounded border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 cursor-pointer"
                >
                  Đóng
                </button>
                <button
                  type="submit"
                  className="rounded bg-primary text-primary-foreground px-4 py-2 text-xs font-semibold hover:bg-primary/95 cursor-pointer shadow-sm"
                >
                  Lưu ghi chú
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
