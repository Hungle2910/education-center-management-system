"use client";

import { useState } from "react";
import type { TuitionInvoice } from "./tuition-types";
import {
  useAdjustInvoice,
  useApplyDiscount,
  useGenerateVietQr,
  useSubmitPaymentProof,
  useConfirmPayment,
  useUpdatePaymentContent,
} from "./tuition-api";

interface Props {
  invoice: TuitionInvoice;
  onClose: () => void;
}

const STATUS_COLORS: Record<string, string> = {
  "Chưa thanh toán": "bg-yellow-50 text-yellow-700 border border-yellow-200",
  "Chờ xác nhận": "bg-blue-50 text-blue-700 border border-blue-200",
  "Đã thanh toán": "bg-green-50 text-green-700 border border-green-200",
  "Thanh toán thiếu": "bg-orange-50 text-orange-700 border border-orange-200",
  "Thanh toán dư": "bg-purple-50 text-purple-700 border border-purple-200",
  "Quá hạn": "bg-red-50 text-red-700 border border-red-200",
  "Đã hủy": "bg-gray-100 text-gray-500 border border-gray-200",
  "Đã hoàn tiền": "bg-teal-50 text-teal-700 border border-teal-200",
};

const formatVND = (n: number) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(n);

type Tab = "detail" | "payment" | "adjust" | "discount";

const TAB_LABELS: Record<Tab, string> = {
  detail: "📋 Chi tiết",
  payment: "💳 Thanh toán",
  adjust: "✏️ Điều chỉnh",
  discount: "🎟 Mã giảm",
};

export function TuitionDetailModal({ invoice: initialInvoice, onClose }: Props) {
  const [invoice, setInvoice] = useState(initialInvoice);
  const [tab, setTab] = useState<Tab>("detail");
  const [feedback, setFeedback] = useState<{ msg: string; ok: boolean } | null>(null);

  // Adjust tab state
  const [adjustAmount, setAdjustAmount] = useState(invoice.adjustAmount.toString());
  const [adjustReason, setAdjustReason] = useState(invoice.adjustReason ?? "");

  // Discount tab state
  const [discountCode, setDiscountCode] = useState("");

  // Payment tab state
  const [proofUrl, setProofUrl] = useState(invoice.paymentProofUrl ?? "");
  const [paidAmount, setPaidAmount] = useState(invoice.totalAmount.toString());
  const [paymentNote, setPaymentNote] = useState("");
  const [customContent, setCustomContent] = useState(invoice.paymentContent ?? "");

  const adjustMut = useAdjustInvoice(invoice.id);
  const discountMut = useApplyDiscount(invoice.id);
  const generateQrMut = useGenerateVietQr(invoice.id);
  const submitProofMut = useSubmitPaymentProof(invoice.id);
  const confirmPaymentMut = useConfirmPayment(invoice.id);
  const updateContentMut = useUpdatePaymentContent(invoice.id);

  // Helper: update local invoice state after a mutation succeeds
  const handleSuccess = (updated: TuitionInvoice, msg: string) => {
    setInvoice(updated);
    setFeedback({ msg, ok: true });
    setCustomContent(updated.paymentContent ?? "");
  };
  const handleError = (e: unknown) => {
    const msg = e instanceof Error ? e.message : "Có lỗi xảy ra.";
    setFeedback({ msg, ok: false });
  };

  const handleAdjust = () => {
    adjustMut.mutate(
      { adjustAmount: parseFloat(adjustAmount) || 0, reason: adjustReason },
      {
        onSuccess: (data) => handleSuccess(data, "Đã điều chỉnh hoá đơn."),
        onError: handleError,
      }
    );
  };

  const handleDiscount = () => {
    discountMut.mutate(
      { discountCode },
      {
        onSuccess: (data) => handleSuccess(data, "Đã áp dụng mã giảm giá."),
        onError: handleError,
      }
    );
  };

  const handleGenerateQr = () => {
    generateQrMut.mutate(
      { overrideContent: customContent || undefined },
      {
        onSuccess: (data) => handleSuccess(data, "Tạo VietQR thành công."),
        onError: handleError,
      }
    );
  };

  const handleSubmitProof = () => {
    submitProofMut.mutate(
      { paymentProofUrl: proofUrl },
      {
        onSuccess: (data) => handleSuccess(data, "Đã gửi biên lai thanh toán."),
        onError: handleError,
      }
    );
  };

  const handleConfirmPayment = () => {
    confirmPaymentMut.mutate(
      { paidAmount: parseFloat(paidAmount) || 0, note: paymentNote || undefined },
      {
        onSuccess: (data) => handleSuccess(data, "Đã xác nhận thanh toán."),
        onError: handleError,
      }
    );
  };

  const handleUpdateContent = () => {
    updateContentMut.mutate(
      { paymentContent: customContent },
      {
        onSuccess: (data) => handleSuccess(data, "Đã cập nhật nội dung chuyển khoản. QR cần được tạo lại."),
        onError: handleError,
      }
    );
  };


  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-lg overflow-hidden">
        {/* Header */}
        <div className="px-6 pt-6 pb-4 border-b border-border">
          <div className="flex items-start justify-between">
            <div>
              <h2 className="text-lg font-bold text-foreground">Chi tiết hoá đơn</h2>
              <p className="text-sm text-muted-foreground mt-0.5">
                {invoice.studentName} · {invoice.className} · {invoice.month}
              </p>
            </div>
            <button
              onClick={onClose}
              className="text-muted-foreground hover:text-foreground transition-colors text-xl leading-none"
            >
              ✕
            </button>
          </div>

          {/* Tabs */}
          <div className="flex gap-1 mt-4 flex-wrap">
            {(["detail", "payment", "adjust", "discount"] as Tab[]).map((t) => (
              <button
                key={t}
                onClick={() => { setTab(t); setFeedback(null); }}
                className={`px-3 py-1.5 text-xs font-semibold rounded-lg transition-colors ${
                  tab === t ? "bg-primary text-white" : "text-muted-foreground hover:bg-muted"
                }`}
              >
                {TAB_LABELS[t]}
              </button>
            ))}
          </div>
        </div>

        <div className="px-6 py-5 space-y-4 max-h-[60vh] overflow-y-auto">
          {/* Feedback */}
          {feedback && (
            <p className={`text-xs font-medium px-3 py-2 rounded-lg ${
              feedback.ok ? "bg-green-50 text-green-700" : "bg-red-50 text-red-600"
            }`}>
              {feedback.ok ? "✓ " : "✕ "}{feedback.msg}
            </p>
          )}

          {/* ─── Detail Tab ─── */}
          {tab === "detail" && (
            <>
              {/* Status */}
              <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold ${
                STATUS_COLORS[invoice.status] ?? "bg-gray-100 text-gray-600"
              }`}>
                {invoice.status}
              </span>

              {/* Amount breakdown */}
              <div className="rounded-xl border border-border bg-muted/30 p-4 space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Học phí gốc</span>
                  <span className="font-medium">{formatVND(invoice.baseAmount)}</span>
                </div>
                {invoice.discountAmount > 0 && (
                  <div className="flex justify-between text-green-600">
                    <span>Giảm giá</span>
                    <span>- {formatVND(invoice.discountAmount)}</span>
                  </div>
                )}
                {invoice.deductAmount > 0 && (
                  <div className="flex justify-between text-amber-600">
                    <span>Cấn trừ buổi nghỉ</span>
                    <span>- {formatVND(invoice.deductAmount)}</span>
                  </div>
                )}
                {invoice.adjustAmount !== 0 && (
                  <div className={`flex justify-between ${invoice.adjustAmount > 0 ? "text-blue-600" : "text-red-600"}`}>
                    <span>Điều chỉnh thủ công</span>
                    <span>{invoice.adjustAmount > 0 ? "+ " : ""}{formatVND(invoice.adjustAmount)}</span>
                  </div>
                )}
                <div className="flex justify-between font-bold text-base pt-2 border-t border-border">
                  <span>Tổng cộng</span>
                  <span className="text-primary">{formatVND(invoice.totalAmount)}</span>
                </div>
                {invoice.paidAmount !== undefined && (
                  <div className={`flex justify-between text-sm pt-1 ${
                    invoice.paidAmount < invoice.totalAmount ? "text-orange-600" : "text-green-600"
                  }`}>
                    <span>Đã thực nhận</span>
                    <span>{formatVND(invoice.paidAmount)}</span>
                  </div>
                )}
              </div>

              {/* Lịch sử thao tác */}
              {invoice.operationHistory && (
                <div className="space-y-1.5 pt-2">
                  <p className="text-xs font-semibold text-foreground uppercase tracking-wide">Lịch sử thao tác</p>
                  <div className="rounded-xl border border-border bg-muted/20 p-3 max-h-[160px] overflow-y-auto text-[11px] space-y-2">
                    {(() => {
                      try {
                        const history = JSON.parse(invoice.operationHistory);
                        if (Array.isArray(history)) {
                          return history.map((item: any, idx: number) => (
                            <div key={idx} className="border-b border-border/50 last:border-0 pb-1.5 last:pb-0">
                              <div className="flex justify-between text-muted-foreground mb-0.5">
                                <span className="font-semibold text-foreground">{item.Action}</span>
                                <span>{new Date(item.At).toLocaleString("vi-VN")}</span>
                              </div>
                              {item.Data && (
                                <pre className="text-[10px] text-muted-foreground overflow-x-auto whitespace-pre-wrap font-sans">
                                  {JSON.stringify(item.Data, null, 2)}
                                </pre>
                              )}
                            </div>
                          ));
                        }
                      } catch {
                        return <p className="italic text-muted-foreground">Không thể đọc lịch sử.</p>;
                      }
                      return null;
                    })()}
                  </div>
                </div>
              )}


              {/* VietQR Preview (read-only here) */}
              {invoice.vietQrUrl && !invoice.vietQrOutdated && (
                <div className="flex flex-col items-center gap-2 pt-1">
                  <p className="text-xs text-muted-foreground font-semibold uppercase tracking-wide">Mã QR thanh toán</p>
                  {/* eslint-disable-next-line @next/next/no-img-element */}
                  <img src={invoice.vietQrUrl} alt="VietQR" className="h-40 w-40 rounded-lg border border-border object-contain" />
                  <p className="text-[10px] text-muted-foreground">Quét để chuyển khoản · {formatVND(invoice.totalAmount)}</p>
                  <p className="text-[11px] font-mono bg-muted px-2 py-1 rounded border text-muted-foreground">
                    {invoice.paymentContent}
                  </p>
                </div>
              )}
              {invoice.vietQrOutdated && invoice.vietQrUrl && (
                <p className="text-xs text-amber-600 bg-amber-50 border border-amber-200 rounded-lg px-3 py-2">
                  ⚠️ Số tiền hoặc nội dung thay đổi. Vui lòng chuyển sang tab <strong>💳 Thanh toán</strong> để tạo lại QR.
                </p>
              )}
            </>
          )}

          {/* ─── Payment Tab ─── */}
          {tab === "payment" && (
            <div className="space-y-5">
              {/* VietQR Section */}
              <div className="rounded-xl border border-border p-4 space-y-3">
                <div className="flex items-center justify-between">
                  <h3 className="text-sm font-semibold text-foreground">Mã QR thanh toán</h3>
                  {invoice.vietQrOutdated && (
                    <span className="text-[10px] font-semibold bg-amber-100 text-amber-700 px-2 py-0.5 rounded-full">
                      Cần tạo lại
                    </span>
                  )}
                </div>

                {invoice.vietQrUrl && !invoice.vietQrOutdated ? (
                  <div className="flex flex-col items-center gap-2">
                    {/* eslint-disable-next-line @next/next/no-img-element */}
                    <img src={invoice.vietQrUrl} alt="VietQR" className="h-44 w-44 rounded-lg border border-border object-contain" />
                    <p className="text-[10px] text-muted-foreground">
                      Quét để chuyển khoản · {formatVND(invoice.totalAmount)}
                    </p>
                    <p className="text-[11px] font-mono bg-muted px-2 py-1 rounded border text-muted-foreground">
                      {invoice.paymentContent}
                    </p>
                    {invoice.vietQrGeneratedAt && (
                      <p className="text-[10px] text-muted-foreground">
                        Tạo lúc: {new Date(invoice.vietQrGeneratedAt).toLocaleString("vi-VN")}
                      </p>
                    )}
                  </div>
                ) : (
                  <div className="space-y-2 py-2">
                    <p className="text-sm text-amber-600 font-medium text-center bg-amber-50 rounded-lg py-2 border border-amber-200">
                      ⚠️ Cần tạo lại VietQR (do số tiền hoặc nội dung thay đổi)
                    </p>
                    {invoice.paymentContent && (
                      <p className="text-[11px] font-mono text-center text-muted-foreground">
                        Nội dung hiện tại: {invoice.paymentContent}
                      </p>
                    )}
                  </div>
                )}

                {/* Edit Payment Content */}
                <div className="space-y-2 pt-2 border-t border-border">
                  <label className="block text-xs font-semibold text-foreground">
                    Nội dung chuyển khoản
                  </label>
                  <div className="flex gap-2">
                    <input
                      type="text"
                      value={customContent}
                      onChange={(e) => setCustomContent(e.target.value)}
                      className="flex-1 border border-border rounded-lg px-3 py-1.5 text-xs focus:outline-none focus:ring-2 focus:ring-primary/40 font-mono"
                      placeholder="Nhập nội dung chuyển khoản..."
                    />
                    <button
                      onClick={handleUpdateContent}
                      disabled={updateContentMut.isPending || customContent === invoice.paymentContent}
                      className="bg-muted border border-border text-foreground hover:bg-muted/80 disabled:opacity-50 px-3 py-1.5 rounded-lg text-xs font-semibold transition-colors"
                    >
                      {updateContentMut.isPending ? "Đang lưu..." : "Lưu nội dung"}
                    </button>
                  </div>
                </div>

                <button
                  onClick={handleGenerateQr}
                  disabled={generateQrMut.isPending}
                  className="w-full bg-primary text-white rounded-lg py-2 text-sm font-semibold hover:bg-primary/90 disabled:opacity-50 transition-colors"
                >
                  {generateQrMut.isPending
                    ? "Đang tạo..."
                    : invoice.vietQrUrl ? "🔄 Tạo lại VietQR" : "✨ Tạo VietQR"}
                </button>
              </div>

              {/* Submit Proof Section */}
              {!["Đã thanh toán"].includes(invoice.status) && (
                <div className="rounded-xl border border-border p-4 space-y-3">
                  <h3 className="text-sm font-semibold text-foreground">Báo đã thanh toán</h3>
                  <p className="text-xs text-muted-foreground">Nhập URL ảnh biên lai hoặc screenshot chuyển khoản.</p>
                  <input
                    type="url"
                    value={proofUrl}
                    onChange={(e) => setProofUrl(e.target.value)}
                    className="w-full border border-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    placeholder="https://..."
                  />
                  <button
                    onClick={handleSubmitProof}
                    disabled={!proofUrl.trim() || submitProofMut.isPending}
                    className="w-full bg-blue-600 text-white rounded-lg py-2 text-sm font-semibold hover:bg-blue-700 disabled:opacity-50 transition-colors"
                  >
                    {submitProofMut.isPending ? "Đang gửi..." : "📤 Báo đã thanh toán"}
                  </button>
                </div>
              )}

              {/* Confirm Payment Section (Admin/Staff) */}
              <div className="rounded-xl border border-border p-4 space-y-3">
                <h3 className="text-sm font-semibold text-foreground">Xác nhận thanh toán</h3>
                {invoice.paymentProofUrl && (
                  <a
                    href={invoice.paymentProofUrl}
                    target="_blank"
                    rel="noreferrer"
                    className="text-xs text-primary underline block"
                  >
                    📎 Xem biên lai phụ huynh gửi
                  </a>
                )}
                <div>
                  <label className="block text-xs font-semibold text-foreground mb-1">
                    Số tiền thực nhận (VNĐ)
                  </label>
                  <input
                    type="number"
                    value={paidAmount}
                    onChange={(e) => setPaidAmount(e.target.value)}
                    className="w-full border border-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    placeholder="Nhập số tiền đã nhận..."
                  />
                  {parseFloat(paidAmount) > 0 && parseFloat(paidAmount) !== invoice.totalAmount && (
                    <p className={`text-xs mt-1 ${parseFloat(paidAmount) < invoice.totalAmount ? "text-orange-600" : "text-purple-600"}`}>
                      {parseFloat(paidAmount) < invoice.totalAmount
                        ? `⚠️ Thiếu ${formatVND(invoice.totalAmount - parseFloat(paidAmount))}`
                        : `ℹ️ Dư ${formatVND(parseFloat(paidAmount) - invoice.totalAmount)}`}
                    </p>
                  )}
                </div>
                <div>
                  <label className="block text-xs font-semibold text-foreground mb-1">Ghi chú (tùy chọn)</label>
                  <input
                    type="text"
                    value={paymentNote}
                    onChange={(e) => setPaymentNote(e.target.value)}
                    className="w-full border border-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                    placeholder="VD: Thanh toán qua MB Bank lúc 14:30..."
                  />
                </div>
                <button
                  onClick={handleConfirmPayment}
                  disabled={!paidAmount || confirmPaymentMut.isPending}
                  className="w-full bg-green-600 text-white rounded-lg py-2 text-sm font-semibold hover:bg-green-700 disabled:opacity-50 transition-colors"
                >
                  {confirmPaymentMut.isPending ? "Đang xác nhận..." : "✅ Xác nhận thanh toán"}
                </button>
              </div>
            </div>
          )}

          {/* ─── Adjust Tab ─── */}
          {tab === "adjust" && (
            <div className="space-y-3">
              <div>
                <label className="block text-xs font-semibold text-foreground mb-1">Số tiền điều chỉnh (VNĐ)</label>
                <input
                  type="number"
                  value={adjustAmount}
                  onChange={(e) => setAdjustAmount(e.target.value)}
                  className="w-full border border-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                  placeholder="Nhập số tiền (âm để giảm)"
                />
              </div>
              <div>
                <label className="block text-xs font-semibold text-foreground mb-1">Lý do điều chỉnh *</label>
                <textarea
                  value={adjustReason}
                  onChange={(e) => setAdjustReason(e.target.value)}
                  rows={3}
                  className="w-full border border-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40 resize-none"
                  placeholder="Nhập lý do điều chỉnh..."
                />
              </div>
              <button
                onClick={handleAdjust}
                disabled={!adjustReason.trim() || adjustMut.isPending}
                className="w-full bg-primary text-white rounded-lg py-2 text-sm font-semibold hover:bg-primary/90 disabled:opacity-50 transition-colors"
              >
                {adjustMut.isPending ? "Đang lưu..." : "Lưu điều chỉnh"}
              </button>
            </div>
          )}

          {/* ─── Discount Tab ─── */}
          {tab === "discount" && (
            <div className="space-y-3">
              <div>
                <label className="block text-xs font-semibold text-foreground mb-1">Mã giảm giá</label>
                <input
                  type="text"
                  value={discountCode}
                  onChange={(e) => setDiscountCode(e.target.value.toUpperCase())}
                  className="w-full border border-border rounded-lg px-3 py-2 text-sm font-mono focus:outline-none focus:ring-2 focus:ring-primary/40"
                  placeholder="VD: GIAM20, HE2026..."
                />
              </div>
              <button
                onClick={handleDiscount}
                disabled={!discountCode.trim() || discountMut.isPending}
                className="w-full bg-primary text-white rounded-lg py-2 text-sm font-semibold hover:bg-primary/90 disabled:opacity-50 transition-colors"
              >
                {discountMut.isPending ? "Đang áp dụng..." : "Áp dụng mã"}
              </button>
            </div>
          )}
        </div>

        <div className="px-6 py-4 border-t border-border flex justify-end">
          <button onClick={onClose} className="px-4 py-2 text-sm rounded-lg border border-border hover:bg-muted transition-colors">
            Đóng
          </button>
        </div>
      </div>
    </div>
  );
}
