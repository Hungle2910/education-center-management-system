"use client";

import { useState } from "react";
import { usePreviewTuition, useGenerateTuition } from "./tuition-api";
import type { TuitionPreview } from "./tuition-types";

interface Props {
  onClose: () => void;
  onSuccess: () => void;
}

const formatVND = (n: number) =>
  new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(n);

export function TuitionWizard({ onClose, onSuccess }: Props) {
  const [step, setStep] = useState<1 | 2 | 3>(1);
  const [classId, setClassId] = useState("");
  const [month, setMonth] = useState(() => {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
  });
  const [previews, setPreviews] = useState<TuitionPreview[]>([]);

  const previewMut = usePreviewTuition();
  const generateMut = useGenerateTuition();

  const handlePreview = async () => {
    const data = await previewMut.mutateAsync({ classId, month });
    setPreviews(data);
    setStep(2);
  };

  const handleGenerate = async () => {
    await generateMut.mutateAsync({ classId, month });
    setStep(3);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-xl overflow-hidden">
        {/* Header */}
        <div className="px-6 pt-6 pb-4 border-b border-border flex items-center justify-between">
          <div>
            <h2 className="text-lg font-bold text-foreground">Tạo hoá đơn học phí tháng</h2>
            <p className="text-xs text-muted-foreground mt-0.5">Bước {step} / 3</p>
          </div>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground text-xl leading-none">✕</button>
        </div>

        {/* Step indicator */}
        <div className="px-6 py-3 flex gap-2">
          {([1, 2, 3] as const).map((s) => (
            <div key={s} className={`flex-1 h-1.5 rounded-full transition-colors ${step >= s ? "bg-primary" : "bg-muted"}`} />
          ))}
        </div>

        <div className="px-6 py-4 max-h-[60vh] overflow-y-auto">
          {/* Step 1: Input */}
          {step === 1 && (
            <div className="space-y-4">
              <p className="text-sm text-muted-foreground">Chọn lớp và tháng cần tạo hoá đơn học phí.</p>
              <div>
                <label className="block text-xs font-semibold text-foreground mb-1">ID Lớp học *</label>
                <input
                  type="text"
                  value={classId}
                  onChange={(e) => setClassId(e.target.value)}
                  placeholder="Nhập ClassId (UUID)..."
                  className="w-full border border-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                />
                <p className="text-[10px] text-muted-foreground mt-1">Lấy từ trang Quản lý Lớp học.</p>
              </div>
              <div>
                <label className="block text-xs font-semibold text-foreground mb-1">Tháng *</label>
                <input
                  type="month"
                  value={month}
                  onChange={(e) => setMonth(e.target.value)}
                  className="w-full border border-border rounded-lg px-3 py-2 text-sm focus:outline-none focus:ring-2 focus:ring-primary/40"
                />
              </div>
              {previewMut.error && (
                <p className="text-xs text-red-500">{(previewMut.error as Error).message}</p>
              )}
            </div>
          )}

          {/* Step 2: Preview */}
          {step === 2 && (
            <div className="space-y-3">
              <p className="text-sm text-muted-foreground">Kiểm tra thông tin trước khi tạo hoá đơn.</p>
              {previews.length === 0 ? (
                <p className="text-center text-sm text-muted-foreground py-8 italic">Không có học sinh nào.</p>
              ) : (
                <div className="divide-y divide-border rounded-xl border border-border overflow-hidden">
                  {previews.map((p) => (
                    <div key={p.studentId} className="px-4 py-3 flex items-center justify-between">
                      <div>
                        <p className="text-sm font-semibold text-foreground">{p.studentName}</p>
                        <p className="text-xs text-muted-foreground mt-0.5">
                          {p.totalSessions} buổi · {p.cancelledSessions} buổi hủy trừ phí
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-xs text-muted-foreground line-through">{formatVND(p.baseAmount)}</p>
                        <p className="text-sm font-bold text-primary">{formatVND(p.estimatedTotal)}</p>
                      </div>
                    </div>
                  ))}
                </div>
              )}
              {generateMut.error && (
                <p className="text-xs text-red-500">{(generateMut.error as Error).message}</p>
              )}
            </div>
          )}

          {/* Step 3: Success */}
          {step === 3 && (
            <div className="flex flex-col items-center gap-4 py-6 text-center">
              <div className="w-16 h-16 rounded-full bg-green-50 flex items-center justify-center text-3xl">✅</div>
              <div>
                <p className="font-bold text-foreground">Tạo hoá đơn thành công!</p>
                <p className="text-sm text-muted-foreground mt-1">
                  {previews.length} hoá đơn đã được tạo cho tháng {month}.
                </p>
              </div>
            </div>
          )}
        </div>

        {/* Footer actions */}
        <div className="px-6 py-4 border-t border-border flex justify-between">
          {step > 1 && step < 3 && (
            <button onClick={() => setStep((s) => (s - 1) as 1 | 2 | 3)} className="px-4 py-2 text-sm rounded-lg border border-border hover:bg-muted transition-colors">
              Quay lại
            </button>
          )}
          {step < 3 && <div />}

          {step === 1 && (
            <button
              onClick={handlePreview}
              disabled={!classId.trim() || !month || previewMut.isPending}
              className="px-5 py-2 bg-primary text-white rounded-lg text-sm font-semibold hover:bg-primary/90 disabled:opacity-50 transition-colors"
            >
              {previewMut.isPending ? "Đang xem trước..." : "Xem trước →"}
            </button>
          )}

          {step === 2 && (
            <button
              onClick={handleGenerate}
              disabled={previews.length === 0 || generateMut.isPending}
              className="px-5 py-2 bg-primary text-white rounded-lg text-sm font-semibold hover:bg-primary/90 disabled:opacity-50 transition-colors"
            >
              {generateMut.isPending ? "Đang tạo..." : `Tạo ${previews.length} hoá đơn ✓`}
            </button>
          )}

          {step === 3 && (
            <button
              onClick={() => { onSuccess(); onClose(); }}
              className="px-5 py-2 bg-primary text-white rounded-lg text-sm font-semibold hover:bg-primary/90 transition-colors"
            >
              Xem danh sách hoá đơn
            </button>
          )}
        </div>
      </div>
    </div>
  );
}
