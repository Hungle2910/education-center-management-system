"use client";

import { useState } from "react";
import { DashboardLayout } from "@/features/dashboard/dashboard-layout";
import { useClasses } from "@/features/classes/class-api";
import { useTeachers } from "@/features/classes/class-api";
import {
  useLeads,
  useCreateLead,
  useUpdateLead,
  useConvertLead,
  useTrialSessions,
  useScheduleTrial,
  useEvaluateTrial,
  useCareLogs,
  useCreateCareLog,
  LeadResponse,
  TrialSessionResponse,
} from "./admissions-api";

export function AdmissionsView() {
  const [activeTab, setActiveTab] = useState<"leads" | "trials">("leads");

  // Lead queries
  const { data: leads, isLoading: isLeadsLoading } = useLeads();
  const createLeadMutation = useCreateLead();
  const updateLeadMutation = useUpdateLead();
  const convertLeadMutation = useConvertLead();

  // Trial queries
  const { data: trials, isLoading: isTrialsLoading } = useTrialSessions();
  const scheduleTrialMutation = useScheduleTrial();
  const evaluateTrialMutation = useEvaluateTrial();

  // Helper lookups
  const { data: classes } = useClasses();
  const { data: teachers } = useTeachers();

  // Modals state
  const [leadModal, setLeadModal] = useState<{ open: boolean; editLead?: LeadResponse } | null>(null);
  const [trialModal, setTrialModal] = useState<LeadResponse | null>(null);
  const [evaluateModal, setEvaluateModal] = useState<TrialSessionResponse | null>(null);
  const [careLogModal, setCareLogModal] = useState<LeadResponse | null>(null);

  // Care log queries & state
  const [contactType, setContactType] = useState("Đã gọi");
  const [careNotes, setCareNotes] = useState("");
  const { data: careLogs } = useCareLogs(undefined, careLogModal?.id);
  const createCareLogMutation = useCreateCareLog();

  const handleSaveLead = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    const data = {
      studentName: formData.get("studentName") as string,
      parentName: formData.get("parentName") as string,
      parentPhone: formData.get("parentPhone") as string,
      email: formData.get("email") as string,
      source: formData.get("source") as string,
      notes: formData.get("notes") as string,
    };

    if (leadModal?.editLead) {
      await updateLeadMutation.mutateAsync({
        id: leadModal.editLead.id,
        data: { ...data, status: leadModal.editLead.status },
      });
    } else {
      await createLeadMutation.mutateAsync(data);
    }
    setLeadModal(null);
  };

  const handleScheduleTrial = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!trialModal) return;
    const formData = new FormData(e.currentTarget);
    await scheduleTrialMutation.mutateAsync({
      leadId: trialModal.id,
      classId: formData.get("classId") as string,
      trialDate: formData.get("trialDate") as string,
      teacherId: formData.get("teacherId") as string || undefined,
      notes: formData.get("notes") as string,
    });
    setTrialModal(null);
  };

  const handleEvaluateTrial = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!evaluateModal) return;
    const formData = new FormData(e.currentTarget);
    await evaluateTrialMutation.mutateAsync({
      id: evaluateModal.id,
      feedback: formData.get("feedback") as string,
      result: formData.get("result") as string,
      notes: formData.get("notes") as string,
    });
    setEvaluateModal(null);
  };

  const handleCreateCareLog = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!careLogModal || !careNotes.trim()) return;
    await createCareLogMutation.mutateAsync({
      leadId: careLogModal.id,
      contactType,
      notes: careNotes,
    });
    setCareNotes("");
  };

  return (
    <DashboardLayout>
      <div className="space-y-6">
        <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Tuyển sinh & Học thử</h1>
            <p className="text-sm text-muted-foreground mt-1">
              Quản lý học sinh tiềm năng, xếp lịch học thử và ghi nhật ký chăm sóc.
            </p>
          </div>
          {activeTab === "leads" && (
            <button
              onClick={() => setLeadModal({ open: true })}
              className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow transition hover:bg-primary/95 cursor-pointer"
            >
              Thêm học sinh tiềm năng
            </button>
          )}
        </div>

        {/* Tab selector */}
        <div className="flex border-b border-border">
          <button
            onClick={() => setActiveTab("leads")}
            className={`pb-3 text-sm font-semibold border-b-2 px-6 transition cursor-pointer ${
              activeTab === "leads"
                ? "border-primary text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground"
            }`}
          >
            Học sinh tiềm năng
          </button>
          <button
            onClick={() => setActiveTab("trials")}
            className={`pb-3 text-sm font-semibold border-b-2 px-6 transition cursor-pointer ${
              activeTab === "trials"
                ? "border-primary text-primary"
                : "border-transparent text-muted-foreground hover:text-foreground"
            }`}
          >
            Lịch học thử
          </button>
        </div>

        {/* Tab Lead content */}
        {activeTab === "leads" && (
          <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
            {isLeadsLoading ? (
              <p className="text-sm text-muted-foreground text-center py-8">Đang tải...</p>
            ) : leads && leads.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse text-sm">
                  <thead>
                    <tr className="border-b border-border text-muted-foreground text-xs font-semibold">
                      <th className="pb-3">Học sinh</th>
                      <th className="pb-3">Phụ huynh & SĐT</th>
                      <th className="pb-3">Nguồn</th>
                      <th className="pb-3">Trạng thái</th>
                      <th className="pb-3 text-right">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {leads.map((l) => (
                      <tr key={l.id} className="border-b border-border/50 last:border-none hover:bg-slate-50/30">
                        <td className="py-3 font-semibold text-foreground">{l.studentName}</td>
                        <td className="py-3 text-muted-foreground">
                          {l.parentName || "Chưa rõ"} - <span className="font-mono">{l.parentPhone}</span>
                        </td>
                        <td className="py-3 text-muted-foreground">{l.source || "--"}</td>
                        <td className="py-3">
                          <span className={`inline-flex px-2 py-0.5 rounded-full text-[10px] font-semibold border ${
                            l.status === "Đã đăng ký"
                              ? "bg-green-50 text-green-700 border-green-200"
                              : l.status === "Đã hẹn học thử"
                              ? "bg-indigo-50 text-indigo-700 border-indigo-200"
                              : "bg-slate-50 text-slate-700 border-slate-200"
                          }`}>
                            {l.status}
                          </span>
                        </td>
                        <td className="py-3 text-right space-x-2">
                          <button
                            onClick={() => setCareLogModal(l)}
                            className="text-xs font-semibold text-teal-600 hover:underline cursor-pointer"
                          >
                            Chăm sóc
                          </button>
                          {l.status !== "Đã đăng ký" && (
                            <>
                              <button
                                onClick={() => setTrialModal(l)}
                                className="text-xs font-semibold text-primary hover:underline cursor-pointer"
                              >
                                Hẹn học thử
                              </button>
                              <button
                                onClick={() => convertLeadMutation.mutate(l.id)}
                                className="text-xs font-semibold text-emerald-600 hover:underline cursor-pointer"
                              >
                                Chuyển chính thức
                              </button>
                            </>
                          )}
                          <button
                            onClick={() => setLeadModal({ open: true, editLead: l })}
                            className="text-xs font-semibold text-slate-500 hover:underline cursor-pointer"
                          >
                            Sửa
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground text-center py-6">Chưa có thông tin học sinh tiềm năng.</p>
            )}
          </div>
        )}

        {/* Tab Trials content */}
        {activeTab === "trials" && (
          <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
            {isTrialsLoading ? (
              <p className="text-sm text-muted-foreground text-center py-8">Đang tải...</p>
            ) : trials && trials.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse text-sm">
                  <thead>
                    <tr className="border-b border-border text-muted-foreground text-xs font-semibold">
                      <th className="pb-3">Học sinh</th>
                      <th className="pb-3">Lớp học thử</th>
                      <th className="pb-3">Ngày học thử</th>
                      <th className="pb-3">Giáo viên</th>
                      <th className="pb-3">Nhận xét & Kết quả</th>
                      <th className="pb-3 text-right">Thao tác</th>
                    </tr>
                  </thead>
                  <tbody>
                    {trials.map((t) => (
                      <tr key={t.id} className="border-b border-border/50 last:border-none hover:bg-slate-50/30">
                        <td className="py-3 font-semibold text-foreground">{t.studentName}</td>
                        <td className="py-3 text-muted-foreground">{t.className}</td>
                        <td className="py-3 text-muted-foreground font-mono">{t.trialDate}</td>
                        <td className="py-3 text-muted-foreground">{t.teacherName || "Chưa phân công"}</td>
                        <td className="py-3">
                          {t.result ? (
                            <span className={`inline-flex px-2 py-0.5 rounded-full text-[10px] font-semibold border ${
                              t.result === "Đăng ký"
                                ? "bg-green-50 text-green-700 border-green-200"
                                : "bg-red-50 text-red-700 border-red-200"
                            }`}>
                              {t.result}
                            </span>
                          ) : (
                            <span className="text-xs text-muted-foreground italic">Chưa đánh giá</span>
                          )}
                          {t.feedback && <p className="text-xs text-muted-foreground mt-1 max-w-xs truncate">{t.feedback}</p>}
                        </td>
                        <td className="py-3 text-right">
                          <button
                            onClick={() => setEvaluateModal(t)}
                            className="text-xs font-semibold text-primary hover:underline cursor-pointer"
                          >
                            Đánh giá kết quả
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-sm text-muted-foreground text-center py-6">Chưa có lịch hẹn học thử nào.</p>
            )}
          </div>
        )}

        {/* Lead Modal */}
        {leadModal?.open && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
            <form onSubmit={handleSaveLead} className="w-full max-w-md bg-surface rounded-xl border border-border p-6 shadow-xl space-y-4">
              <h3 className="text-base font-bold text-foreground">
                {leadModal.editLead ? "Sửa thông tin tiềm năng" : "Thêm học sinh tiềm năng"}
              </h3>
              <div className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Tên học sinh</label>
                  <input
                    type="text"
                    name="studentName"
                    required
                    defaultValue={leadModal.editLead?.studentName}
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Tên phụ huynh</label>
                  <input
                    type="text"
                    name="parentName"
                    defaultValue={leadModal.editLead?.parentName}
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">SĐT phụ huynh</label>
                  <input
                    type="text"
                    name="parentPhone"
                    required
                    defaultValue={leadModal.editLead?.parentPhone}
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Email</label>
                  <input
                    type="email"
                    name="email"
                    defaultValue={leadModal.editLead?.email}
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Nguồn tuyển sinh</label>
                  <input
                    type="text"
                    name="source"
                    placeholder="Facebook, Giới thiệu, v.v."
                    defaultValue={leadModal.editLead?.source}
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Ghi chú</label>
                  <textarea
                    name="notes"
                    rows={3}
                    defaultValue={leadModal.editLead?.notes}
                    className="w-full rounded border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary resize-none"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setLeadModal(null)}
                  className="rounded border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 cursor-pointer"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="rounded bg-primary text-primary-foreground px-4 py-2 text-xs font-semibold hover:bg-primary/95 cursor-pointer shadow-sm"
                >
                  Lưu
                </button>
              </div>
            </form>
          </div>
        )}

        {/* Schedule Trial Modal */}
        {trialModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
            <form onSubmit={handleScheduleTrial} className="w-full max-w-md bg-surface rounded-xl border border-border p-6 shadow-xl space-y-4">
              <h3 className="text-base font-bold text-foreground">Đặt lịch học thử: {trialModal.studentName}</h3>
              <div className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Chọn lớp</label>
                  <select
                    name="classId"
                    required
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  >
                    <option value="">-- Chọn lớp học --</option>
                    {classes?.map((c) => (
                      <option key={c.id} value={c.id}>
                        {c.name} ({c.subject})
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Ngày học thử</label>
                  <input
                    type="date"
                    name="trialDate"
                    required
                    defaultValue={new Date().toISOString().split("T")[0]}
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Giáo viên phụ trách (Tùy chọn)</label>
                  <select
                    name="teacherId"
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  >
                    <option value="">-- Chọn giáo viên --</option>
                    {teachers?.map((t) => (
                      <option key={t.id} value={t.id}>
                        {t.fullName}
                      </option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Ghi chú</label>
                  <textarea
                    name="notes"
                    rows={2}
                    className="w-full rounded border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary resize-none"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setTrialModal(null)}
                  className="rounded border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 cursor-pointer"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="rounded bg-primary text-primary-foreground px-4 py-2 text-xs font-semibold hover:bg-primary/95 cursor-pointer shadow-sm"
                >
                  Đặt lịch
                </button>
              </div>
            </form>
          </div>
        )}

        {/* Evaluate Trial Modal */}
        {evaluateModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
            <form onSubmit={handleEvaluateTrial} className="w-full max-w-md bg-surface rounded-xl border border-border p-6 shadow-xl space-y-4">
              <h3 className="text-base font-bold text-foreground">Đánh giá kết quả học thử: {evaluateModal.studentName}</h3>
              <div className="space-y-3">
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Kết quả</label>
                  <select
                    name="result"
                    required
                    className="w-full h-9 rounded border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  >
                    <option value="Đăng ký">Đăng ký (Chuyển thành học viên chính thức)</option>
                    <option value="Không đăng ký">Không đăng ký</option>
                  </select>
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Nhận xét của giáo viên</label>
                  <textarea
                    name="feedback"
                    required
                    rows={3}
                    className="w-full rounded border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary resize-none"
                  />
                </div>
                <div>
                  <label className="block text-xs font-medium text-muted-foreground mb-1">Ghi chú thêm</label>
                  <textarea
                    name="notes"
                    rows={2}
                    className="w-full rounded border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary resize-none"
                  />
                </div>
              </div>
              <div className="flex justify-end gap-2">
                <button
                  type="button"
                  onClick={() => setEvaluateModal(null)}
                  className="rounded border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 cursor-pointer"
                >
                  Hủy
                </button>
                <button
                  type="submit"
                  className="rounded bg-primary text-primary-foreground px-4 py-2 text-xs font-semibold hover:bg-primary/95 cursor-pointer shadow-sm"
                >
                  Lưu đánh giá
                </button>
              </div>
            </form>
          </div>
        )}

        {/* Parent Care Log Modal */}
        {careLogModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
            <div className="w-full max-w-lg bg-surface rounded-xl border border-border p-6 shadow-xl space-y-4">
              <div className="flex justify-between items-center">
                <h3 className="text-base font-bold text-foreground">Nhật ký chăm sóc: {careLogModal.studentName}</h3>
                <button
                  onClick={() => setCareLogModal(null)}
                  className="text-muted-foreground hover:text-foreground text-base cursor-pointer"
                >
                  ✕
                </button>
              </div>

              {/* Log entry list */}
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

              {/* Add log form */}
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
                    onClick={() => setCareLogModal(null)}
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
    </DashboardLayout>
  );
}
