"use client";

import { useState } from "react";
import { useInvoices } from "./tuition-api";
import type { TuitionInvoice } from "./tuition-types";
import { TuitionDetailModal } from "./tuition-detail-modal";
import { TuitionWizard } from "./tuition-wizard";
import { PaymentSettingsModal } from "./payment-settings-modal";
import { useAuth } from "@/features/auth/auth-context";

const formatVND = (n: number) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(n);

const STATUS_COLORS: Record<string, string> = {
  "Chưa thanh toán": "bg-yellow-50 text-yellow-700 border-yellow-200",
  "Chờ xác nhận": "bg-blue-50 text-blue-700 border-blue-200",
  "Đã thanh toán": "bg-green-50 text-green-700 border-green-200",
  "Quá hạn": "bg-red-50 text-red-700 border-red-200",
  "Đã hủy": "bg-gray-50 text-gray-500 border-gray-200",
};

export function TuitionList() {
  const { user } = useAuth();
  const isAdmin = user?.roles.includes("Admin") ?? false;

  const [monthFilter, setMonthFilter] = useState(() => {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
  });
  const [selected, setSelected] = useState<TuitionInvoice | null>(null);
  const [showWizard, setShowWizard] = useState(false);
  const [showSettings, setShowSettings] = useState(false);

  const { data: invoices, isLoading, refetch } = useInvoices(undefined, monthFilter);

  return (
    <div className="space-y-4">
      {/* Toolbar */}
      <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <label className="text-sm font-medium text-foreground">Tháng</label>
          <input
            type="month"
            value={monthFilter}
            onChange={(e) => setMonthFilter(e.target.value)}
            className="border border-border rounded-lg px-3 py-1.5 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
          />
        </div>
        <div className="flex gap-2">
          {isAdmin && (
            <button
              onClick={() => setShowSettings(true)}
              className="flex items-center gap-2 px-4 py-2 bg-muted border border-border text-foreground rounded-xl text-sm font-semibold hover:bg-muted/80 transition-colors shadow-sm"
            >
              ⚙️ Cấu hình VietQR
            </button>
          )}
          <button
            onClick={() => setShowWizard(true)}
            className="flex items-center gap-2 px-4 py-2 bg-primary text-white rounded-xl text-sm font-semibold hover:bg-primary/90 transition-colors shadow-sm"
          >
            + Tạo hoá đơn tháng
          </button>
        </div>
      </div>

      {/* Stats summary */}
      {invoices && invoices.length > 0 && (
        <div className="grid grid-cols-2 sm:grid-cols-4 gap-3">
          {[
            { label: "Tổng hoá đơn", value: invoices.length, color: "text-foreground" },
            {
              label: "Đã thanh toán",
              value: invoices.filter((i) => i.status === "Đã thanh toán").length,
              color: "text-green-600",
            },
            {
              label: "Chưa thanh toán",
              value: invoices.filter((i) => i.status === "Chưa thanh toán").length,
              color: "text-yellow-600",
            },
            {
              label: "Tổng thu dự kiến",
              value: formatVND(invoices.reduce((a, b) => a + b.totalAmount, 0)),
              color: "text-primary",
            },
          ].map((s) => (
            <div key={s.label} className="bg-muted/30 rounded-xl p-3 border border-border">
              <p className="text-xs text-muted-foreground">{s.label}</p>
              <p className={`font-bold text-sm mt-1 ${s.color}`}>{s.value}</p>
            </div>
          ))}
        </div>
      )}

      {/* Invoice table */}
      {isLoading ? (
        <p className="text-center text-sm text-muted-foreground py-12 italic animate-pulse">Đang tải...</p>
      ) : !invoices || invoices.length === 0 ? (
        <div className="text-center py-16 space-y-2">
          <p className="text-4xl">📄</p>
          <p className="text-sm text-muted-foreground italic">Chưa có hoá đơn nào trong tháng này.</p>
          <button onClick={() => setShowWizard(true)} className="text-xs text-primary font-semibold hover:underline">
            Tạo hoá đơn ngay →
          </button>
        </div>
      ) : (
        <div className="rounded-xl border border-border overflow-hidden">
          <div className="overflow-x-auto">
            <table className="w-full text-sm">
              <thead>
                <tr className="bg-muted/50 border-b border-border text-xs text-muted-foreground font-semibold uppercase tracking-wide">
                  <th className="px-4 py-3 text-left">Học sinh</th>
                  <th className="px-4 py-3 text-left">Lớp</th>
                  <th className="px-4 py-3 text-right">Học phí</th>
                  <th className="px-4 py-3 text-right">Giảm</th>
                  <th className="px-4 py-3 text-right">Tổng cộng</th>
                  <th className="px-4 py-3 text-center">Trạng thái</th>
                  <th className="px-4 py-3 text-center">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {invoices.map((inv) => (
                  <tr key={inv.id} className="hover:bg-muted/20 transition-colors">
                    <td className="px-4 py-3 font-medium">{inv.studentName}</td>
                    <td className="px-4 py-3 text-muted-foreground">{inv.className}</td>
                    <td className="px-4 py-3 text-right">{formatVND(inv.baseAmount)}</td>
                    <td className="px-4 py-3 text-right text-green-600">
                      {inv.discountAmount + inv.deductAmount > 0
                        ? `- ${formatVND(inv.discountAmount + inv.deductAmount)}`
                        : "—"}
                    </td>
                    <td className="px-4 py-3 text-right font-bold text-primary">{formatVND(inv.totalAmount)}</td>
                    <td className="px-4 py-3 text-center">
                      <span className={`inline-flex items-center px-2 py-0.5 rounded-full text-[10px] font-semibold border ${
                        STATUS_COLORS[inv.status] ?? "bg-gray-50 text-gray-600 border-gray-200"
                      }`}>
                        {inv.status}
                      </span>
                    </td>
                    <td className="px-4 py-3 text-center">
                      <button
                        onClick={() => setSelected(inv)}
                        className="text-xs font-semibold text-primary hover:text-primary/80 transition-colors"
                      >
                        Chi tiết
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Modals */}
      {showWizard && (
        <TuitionWizard onClose={() => setShowWizard(false)} onSuccess={() => refetch()} />
      )}
      {selected && (
        <TuitionDetailModal invoice={selected} onClose={() => setSelected(null)} />
      )}
      {showSettings && (
        <PaymentSettingsModal onClose={() => setShowSettings(false)} />
      )}
    </div>
  );
}
