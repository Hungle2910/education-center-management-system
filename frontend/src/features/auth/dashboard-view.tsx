"use client";

import { useState } from "react";
import { DashboardLayout } from "@/features/dashboard/dashboard-layout";
import { useAuth } from "./auth-context";
import { useAdminOverview, useAdminOperations } from "../dashboard/dashboard-api";
import { useClasses } from "@/features/classes/class-api";
import { useConfirmPayment } from "@/features/tuition/tuition-api";

export function DashboardView() {
  const { user } = useAuth();
  const isAdminOrStaff = user?.roles.includes("Admin") || user?.roles.includes("Staff");

  // Filter states
  const [selectedMonth, setSelectedMonth] = useState<number | undefined>(undefined);
  const [selectedYear, setSelectedYear] = useState<number | undefined>(undefined);
  const [selectedClassId, setSelectedClassId] = useState<string>("");

  const filter = {
    month: selectedMonth,
    year: selectedYear,
    classId: selectedClassId || undefined,
  };

  const { data: overview, isLoading: isOverviewLoading, refetch: refetchOverview } = useAdminOverview(filter);
  const { data: operations, isLoading: isOperationsLoading, refetch: refetchOperations } = useAdminOperations(filter);
  const { data: classes } = useClasses();

  // Handle confirm payment quick action
  const [confirmingInvoiceId, setConfirmingInvoiceId] = useState<string | null>(null);
  const [confirmNotes, setConfirmNotes] = useState("");
  const confirmPaymentMutation = useConfirmPayment(confirmingInvoiceId || "");

  const handleConfirmPayment = async (invoiceId: string, paidAmount: number) => {
    try {
      await confirmPaymentMutation.mutateAsync({
        paidAmount: paidAmount,
        note: confirmNotes || "Xác nhận thanh toán từ Dashboard",
      });
      setConfirmingInvoiceId(null);
      setConfirmNotes("");
      refetchOverview();
      refetchOperations();
    } catch (err) {
      console.error("Lỗi khi xác nhận thanh toán:", err);
    }
  };

  const formatCurrency = (amount: number) => {
    return amount.toLocaleString("vi-VN") + " đ";
  };

  const months = Array.from({ length: 12 }, (_, i) => i + 1);
  const years = [2024, 2025, 2026, 2027];

  return (
    <DashboardLayout>
      <div className="space-y-6">
        {/* Header */}
        <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
          <div>
            <h1 className="text-2xl font-bold tracking-tight">Tổng quan</h1>
            <p className="text-sm text-muted-foreground mt-1">
              Chào mừng trở lại, {user?.fullName}! Vai trò: {user?.roles.includes("Admin") ? "Quản trị viên" : "Nhân viên"}
            </p>
          </div>

          {/* Filters */}
          <div className="flex flex-wrap items-center gap-3">
            <select
              value={selectedMonth ?? ""}
              onChange={(e) => setSelectedMonth(e.target.value ? parseInt(e.target.value) : undefined)}
              className="h-9 rounded-md border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
            >
              <option value="">Chọn tháng</option>
              {months.map((m) => (
                <option key={m} value={m}>Tháng {m}</option>
              ))}
            </select>

            <select
              value={selectedYear ?? ""}
              onChange={(e) => setSelectedYear(e.target.value ? parseInt(e.target.value) : undefined)}
              className="h-9 rounded-md border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
            >
              <option value="">Chọn năm</option>
              {years.map((y) => (
                <option key={y} value={y}>{y}</option>
              ))}
            </select>

            <select
              value={selectedClassId}
              onChange={(e) => setSelectedClassId(e.target.value)}
              className="h-9 rounded-md border border-border bg-surface px-3 py-1 text-sm focus:outline-none focus:ring-1 focus:ring-primary max-w-[200px]"
            >
              <option value="">Chọn lớp học</option>
              {classes?.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>
        </div>

        {/* Schedule Conflict Alert */}
        {overview && overview.scheduleConflictCount > 0 && (
          <div className="p-4 rounded-lg bg-red-50 border border-red-200 text-red-800 flex items-center justify-between shadow-sm animate-in fade-in slide-in-from-top duration-200">
            <div className="flex items-center gap-3">
              <span className="text-lg">⚠️</span>
              <p className="text-sm font-semibold">
                Cảnh báo: Có {overview.scheduleConflictCount} lớp đang bị trùng lịch phòng học hoặc giáo viên!
              </p>
            </div>
            <a href="/dashboard/schedules" className="text-xs font-bold underline hover:text-red-900 shrink-0">
              Kiểm tra ngay
            </a>
          </div>
        )}

        {/* Stats Grid */}
        <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
          <div className="p-6 rounded-xl border border-border bg-surface shadow-sm transition hover:shadow-md">
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Tổng doanh thu</h3>
            <p className="text-2xl font-bold mt-2 text-emerald-600">
              {isOverviewLoading ? "..." : formatCurrency(overview?.totalTuitionRevenue ?? 0)}
            </p>
            <span className="text-[10px] text-muted-foreground mt-1 block">Từ các hoá đơn đã đóng</span>
          </div>

          <div className="p-6 rounded-xl border border-border bg-surface shadow-sm transition hover:shadow-md">
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Học sinh đang học</h3>
            <p className="text-2xl font-bold mt-2 text-teal-600">
              {isOverviewLoading ? "..." : overview?.activeStudentCount ?? 0}
            </p>
            <span className="text-[10px] text-muted-foreground mt-1 block">Học viên chính thức hoạt động</span>
          </div>

          <div className="p-6 rounded-xl border border-border bg-surface shadow-sm transition hover:shadow-md">
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Lớp đang học</h3>
            <p className="text-2xl font-bold mt-2 text-indigo-600">
              {isOverviewLoading ? "..." : overview?.activeClassCount ?? 0}
            </p>
            <span className="text-[10px] text-muted-foreground mt-1 block">Các lớp đang tổ chức giảng dạy</span>
          </div>

          <div className="p-6 rounded-xl border border-border bg-surface shadow-sm transition hover:shadow-md">
            <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Cần tuyển thêm</h3>
            <p className="text-2xl font-bold mt-2 text-amber-600">
              {isOverviewLoading ? "..." : overview?.recruitingClassCount ?? 0}
            </p>
            <span className="text-[10px] text-muted-foreground mt-1 block">Sĩ số dưới mức mục tiêu tối thiểu</span>
          </div>
        </div>

        {/* Dashboard Split View */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Today's Schedules */}
          <div className="lg:col-span-2 p-6 rounded-xl border border-border bg-surface shadow-sm">
            <h2 className="text-base font-bold text-foreground mb-4">Lịch học hôm nay</h2>
            {isOperationsLoading ? (
              <p className="text-xs text-muted-foreground py-6 text-center">Đang tải lịch học...</p>
            ) : operations?.todaySchedules && operations.todaySchedules.length > 0 ? (
              <div className="overflow-x-auto">
                <table className="w-full text-left border-collapse text-sm">
                  <thead>
                    <tr className="border-b border-border text-muted-foreground text-xs font-semibold">
                      <th className="pb-3">Lớp học</th>
                      <th className="pb-3">Thời gian</th>
                      <th className="pb-3">Giáo viên</th>
                      <th className="pb-3">Phòng</th>
                      <th className="pb-3 text-right">Trạng thái</th>
                    </tr>
                  </thead>
                  <tbody>
                    {operations.todaySchedules.map((item) => (
                      <tr key={item.occurrenceId} className="border-b border-border/50 last:border-none hover:bg-slate-50/50">
                        <td className="py-3 font-medium text-foreground">{item.className}</td>
                        <td className="py-3 text-muted-foreground">
                          {item.startTime.substring(0, 5)} - {item.endTime.substring(0, 5)}
                        </td>
                        <td className="py-3 text-muted-foreground">{item.teacherName || "Chưa phân công"}</td>
                        <td className="py-3 text-muted-foreground">{item.roomName}</td>
                        <td className="py-3 text-right">
                          <span className={`inline-flex px-2.5 py-0.5 rounded-full text-[10px] font-semibold ${
                            item.status === "Đã học"
                              ? "bg-green-50 text-green-700 border border-green-200"
                              : item.status === "Đã hủy"
                              ? "bg-red-50 text-red-700 border border-red-200"
                              : "bg-blue-50 text-blue-700 border border-blue-200"
                          }`}>
                            {item.status}
                          </span>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <p className="text-xs text-muted-foreground py-10 text-center">Không có ca học nào diễn ra hôm nay.</p>
            )}
          </div>

          {/* Quick Actions & Welcome */}
          <div className="space-y-6">
            <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
              <h2 className="text-base font-bold text-foreground mb-3">Lịch học thử</h2>
              <div className="flex items-center justify-between p-4 rounded-lg bg-teal-50/50 border border-teal-100 text-teal-800">
                <div>
                  <span className="text-xs font-semibold uppercase tracking-wider text-teal-600">Hôm nay</span>
                  <p className="text-2xl font-bold mt-0.5">{operations?.trialSessionsTodayCount ?? 0} ca</p>
                </div>
                <span className="text-2xl">🎓</span>
              </div>
            </div>

            <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
              <h2 className="text-base font-bold text-foreground mb-3">Liên kết nhanh</h2>
              <div className="grid grid-cols-2 gap-2 text-center text-xs font-semibold">
                <a href="/dashboard/students" className="p-3 rounded-lg border border-border hover:bg-slate-50 transition text-foreground">
                  👥 Học sinh
                </a>
                <a href="/dashboard/classes" className="p-3 rounded-lg border border-border hover:bg-slate-50 transition text-foreground">
                  🏫 Lớp học
                </a>
                <a href="/dashboard/schedules" className="p-3 rounded-lg border border-border hover:bg-slate-50 transition text-foreground">
                  📅 Lịch học
                </a>
                <a href="/dashboard/tuition" className="p-3 rounded-lg border border-border hover:bg-slate-50 transition text-foreground">
                  💳 Học phí
                </a>
              </div>
            </div>
          </div>
        </div>

        {/* Pending Payments Table */}
        <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
          <h2 className="text-base font-bold text-foreground mb-4">Hóa đơn chờ xác nhận thanh toán</h2>
          {isOperationsLoading ? (
            <p className="text-xs text-muted-foreground py-6 text-center">Đang tải danh sách hóa đơn...</p>
          ) : operations?.pendingPaymentInvoices && operations.pendingPaymentInvoices.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse text-sm">
                <thead>
                  <tr className="border-b border-border text-muted-foreground text-xs font-semibold">
                    <th className="pb-3">Học sinh</th>
                    <th className="pb-3">Lớp</th>
                    <th className="pb-3">Tháng</th>
                    <th className="pb-3">Số tiền</th>
                    <th className="pb-3">Biên lai</th>
                    <th className="pb-3 text-right">Thao tác</th>
                  </tr>
                </thead>
                <tbody>
                  {operations.pendingPaymentInvoices.map((item) => (
                    <tr key={item.invoiceId} className="border-b border-border/50 last:border-none hover:bg-slate-50/50">
                      <td className="py-3 font-medium text-foreground">{item.studentName}</td>
                      <td className="py-3 text-muted-foreground">{item.className}</td>
                      <td className="py-3 text-muted-foreground">{item.month}</td>
                      <td className="py-3 font-semibold text-foreground">{formatCurrency(item.totalAmount)}</td>
                      <td className="py-3">
                        {item.paymentProofUrl ? (
                          <a
                            href={item.paymentProofUrl}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="text-xs text-primary underline font-medium hover:text-primary-hover"
                          >
                            Xem ảnh biên lai
                          </a>
                        ) : (
                          <span className="text-xs text-muted-foreground">Không có</span>
                        )}
                      </td>
                      <td className="py-3 text-right">
                        {confirmingInvoiceId === item.invoiceId ? (
                          <div className="inline-flex items-center gap-1.5">
                            <input
                              type="text"
                              placeholder="Ghi chú..."
                              value={confirmNotes}
                              onChange={(e) => setConfirmNotes(e.target.value)}
                              className="h-8 rounded border border-border bg-surface px-2 text-xs focus:outline-none"
                            />
                            <button
                              onClick={() => handleConfirmPayment(item.invoiceId, item.totalAmount)}
                              disabled={confirmPaymentMutation.isPending}
                              className="h-8 rounded bg-green-600 px-3 text-xs font-bold text-white hover:bg-green-700 transition"
                            >
                              {confirmPaymentMutation.isPending ? "..." : "OK"}
                            </button>
                            <button
                              onClick={() => setConfirmingInvoiceId(null)}
                              className="h-8 rounded border border-border bg-surface px-2 text-xs font-semibold hover:bg-slate-50"
                            >
                              Hủy
                            </button>
                          </div>
                        ) : (
                          <button
                            onClick={() => {
                              setConfirmingInvoiceId(item.invoiceId);
                              setConfirmNotes("");
                            }}
                            className="h-8 rounded bg-primary text-primary-foreground px-3 text-xs font-bold hover:bg-primary/95 transition shadow-sm cursor-pointer"
                          >
                            Duyệt nhanh
                          </button>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-xs text-muted-foreground py-10 text-center">Không có hóa đơn nào đang chờ duyệt.</p>
          )}
        </div>
      </div>
    </DashboardLayout>
  );
}
