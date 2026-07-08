"use client";

import { useState } from "react";
import {
  usePaymentSettings,
  useCreatePaymentSetting,
  useUpdatePaymentSetting,
  useDeletePaymentSetting,
  useSetDefaultPaymentSetting,
} from "./tuition-api";
import type { PaymentSetting } from "./tuition-types";

interface Props {
  onClose: () => void;
}

export function PaymentSettingsModal({ onClose }: Props) {
  const { data: settings, isLoading, refetch } = usePaymentSettings();
  const createMut = useCreatePaymentSetting();
  const updateMut = useUpdatePaymentSetting();
  const deleteMut = useDeletePaymentSetting();
  const setDefaultMut = useSetDefaultPaymentSetting();

  const [editing, setEditing] = useState<PaymentSetting | null>(null);
  const [isAdding, setIsAdding] = useState(false);
  const [feedback, setFeedback] = useState<{ msg: string; ok: boolean } | null>(null);

  // Form states
  const [bankName, setBankName] = useState("");
  const [bankId, setBankId] = useState("");
  const [accountNo, setAccountNo] = useState("");
  const [accountName, setAccountName] = useState("");
  const [vietQrTemplate, setVietQrTemplate] = useState("compact2");
  const [isDefault, setIsDefault] = useState(false);
  const [isActive, setIsActive] = useState(true);

  const resetForm = () => {
    setBankName("");
    setBankId("");
    setAccountNo("");
    setAccountName("");
    setVietQrTemplate("compact2");
    setIsDefault(false);
    setIsActive(true);
    setFeedback(null);
  };

  const handleEdit = (setting: PaymentSetting) => {
    setEditing(setting);
    setIsAdding(false);
    setBankName(setting.bankName);
    setBankId(setting.bankId);
    setAccountNo(setting.accountNo);
    setAccountName(setting.accountName);
    setVietQrTemplate(setting.vietQrTemplate);
    setIsDefault(setting.isDefault);
    setIsActive(setting.isActive);
    setFeedback(null);
  };

  const handleSuccess = (msg: string) => {
    setFeedback({ msg, ok: true });
    refetch();
    setEditing(null);
    setIsAdding(false);
    resetForm();
  };

  const handleError = (err: unknown) => {
    const msg = err instanceof Error ? err.message : "Thao tác thất bại.";
    setFeedback({ msg, ok: false });
  };

  const handleSave = () => {
    const req = {
      bankName,
      bankId,
      accountNo,
      accountName,
      vietQrTemplate,
      isDefault,
      isActive,
    };

    if (editing) {
      updateMut.mutate(
        { id: editing.id, request: req },
        {
          onSuccess: () => handleSuccess("Cập nhật cấu hình thành công."),
          onError: handleError,
        }
      );
    } else {
      createMut.mutate(req, {
        onSuccess: () => handleSuccess("Thêm cấu hình thành công."),
        onError: handleError,
      });
    }
  };

  const handleDelete = (id: string) => {
    if (!confirm("Bạn có chắc chắn muốn xóa cấu hình này?")) return;
    deleteMut.mutate(id, {
      onSuccess: () => {
        setFeedback({ msg: "Đã xóa cấu hình ngân hàng.", ok: true });
        refetch();
      },
      onError: handleError,
    });
  };

  const handleSetDefault = (id: string) => {
    setDefaultMut.mutate(id, {
      onSuccess: () => {
        setFeedback({ msg: "Đã thiết lập ngân hàng mặc định mới.", ok: true });
        refetch();
      },
      onError: handleError,
    });
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
      <div className="bg-white rounded-2xl shadow-2xl w-full max-w-2xl overflow-hidden flex flex-col max-h-[90vh]">
        {/* Header */}
        <div className="px-6 py-4 border-b border-border flex justify-between items-center">
          <div>
            <h2 className="text-lg font-bold text-foreground">Cấu hình VietQR</h2>
            <p className="text-xs text-muted-foreground">Quản lý các tài khoản ngân hàng nhận học phí của trung tâm</p>
          </div>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground text-lg">✕</button>
        </div>

        <div className="p-6 overflow-y-auto space-y-4 flex-1">
          {feedback && (
            <div className={`px-4 py-2 text-xs font-semibold rounded-lg ${feedback.ok ? "bg-green-50 text-green-700 border border-green-200" : "bg-red-50 text-red-700 border border-red-200"}`}>
              {feedback.msg}
            </div>
          )}

          {/* Form Editor */}
          {(isAdding || editing) ? (
            <div className="bg-muted/30 border border-border rounded-xl p-4 space-y-4">
              <h3 className="text-xs font-bold text-primary uppercase tracking-wide">
                {editing ? "✏️ Chỉnh sửa cấu hình" : "✨ Thêm cấu hình mới"}
              </h3>
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                <div>
                  <label className="block text-xs font-semibold mb-1">Tên ngân hàng (VD: Vietcombank)</label>
                  <input type="text" value={bankName} onChange={(e) => setBankName(e.target.value)} className="w-full border border-border rounded-lg px-3 py-2 text-sm" placeholder="VD: Vietcombank" />
                </div>
                <div>
                  <label className="block text-xs font-semibold mb-1">Mã ngân hàng (ID VietQR, VD: vietcombank)</label>
                  <input type="text" value={bankId} onChange={(e) => setBankId(e.target.value)} className="w-full border border-border rounded-lg px-3 py-2 text-sm" placeholder="VD: vietcombank, mbbank" />
                </div>
                <div>
                  <label className="block text-xs font-semibold mb-1">Số tài khoản</label>
                  <input type="text" value={accountNo} onChange={(e) => setAccountNo(e.target.value)} className="w-full border border-border rounded-lg px-3 py-2 text-sm" placeholder="VD: 1021965186" />
                </div>
                <div>
                  <label className="block text-xs font-semibold mb-1">Tên tài khoản (Không dấu)</label>
                  <input type="text" value={accountName} onChange={(e) => setAccountName(e.target.value.toUpperCase())} className="w-full border border-border rounded-lg px-3 py-2 text-sm font-semibold" placeholder="VD: LE DOAN GIA HUNG" />
                </div>
                <div>
                  <label className="block text-xs font-semibold mb-1">VietQR Template</label>
                  <select value={vietQrTemplate} onChange={(e) => setVietQrTemplate(e.target.value)} className="w-full border border-border rounded-lg px-3 py-2 text-sm">
                    <option value="compact2">Compact 2 (Nhỏ gọn kèm Logo)</option>
                    <option value="compact">Compact (Nhỏ gọn)</option>
                    <option value="qr_only">QR Only (Chỉ ảnh QR)</option>
                    <option value="print">Print (Chất lượng in ấn)</option>
                  </select>
                </div>
                <div className="flex items-center gap-6 pt-6">
                  <label className="flex items-center gap-2 text-xs font-semibold cursor-pointer">
                    <input type="checkbox" checked={isDefault} onChange={(e) => setIsDefault(e.target.checked)} className="rounded text-primary focus:ring-primary border-border" />
                    Mặc định
                  </label>
                  <label className="flex items-center gap-2 text-xs font-semibold cursor-pointer">
                    <input type="checkbox" checked={isActive} onChange={(e) => setIsActive(e.target.checked)} className="rounded text-primary focus:ring-primary border-border" />
                    Kích hoạt
                  </label>
                </div>
              </div>
              <div className="flex justify-end gap-2 pt-2">
                <button onClick={() => { setIsAdding(false); setEditing(null); }} className="px-3 py-1.5 text-xs border border-border rounded-lg hover:bg-muted">Hủy</button>
                <button onClick={handleSave} disabled={!bankName || !bankId || !accountNo || !accountName} className="px-3 py-1.5 text-xs bg-primary text-white rounded-lg hover:bg-primary/90 disabled:opacity-50">Lưu</button>
              </div>
            </div>
          ) : (
            <div className="flex justify-between items-center">
              <span className="text-xs text-muted-foreground">Danh sách ngân hàng nhận thanh toán học phí</span>
              <button onClick={() => { setIsAdding(true); resetForm(); }} className="px-3 py-1.5 text-xs bg-primary text-white rounded-lg font-semibold hover:bg-primary/90 transition-colors">+ Thêm tài khoản</button>
            </div>
          )}

          {/* Settings List */}
          {isLoading ? (
            <p className="text-center text-xs text-muted-foreground py-8 italic animate-pulse">Đang tải cấu hình...</p>
          ) : !settings || settings.length === 0 ? (
            <div className="text-center py-8 border border-dashed border-border rounded-xl">
              <p className="text-xs text-muted-foreground italic">Chưa cấu hình tài khoản ngân hàng nào.</p>
            </div>
          ) : (
            <div className="space-y-3">
              {settings.map((item) => (
                <div key={item.id} className={`border rounded-xl p-4 flex flex-col sm:flex-row justify-between items-start sm:items-center gap-3 transition-colors ${item.isDefault ? "border-primary/40 bg-primary/5" : "border-border bg-background"}`}>
                  <div>
                    <div className="flex items-center gap-2">
                      <span className="font-semibold text-sm">{item.bankName}</span>
                      <span className="text-[10px] uppercase font-mono px-1.5 py-0.5 rounded bg-muted text-muted-foreground">{item.bankId}</span>
                      {item.isDefault && <span className="text-[9px] font-bold bg-primary text-white px-2 py-0.5 rounded-full">Mặc định</span>}
                      {!item.isActive && <span className="text-[9px] font-bold bg-red-100 text-red-700 px-2 py-0.5 rounded-full">Khóa</span>}
                    </div>
                    <p className="text-xs text-muted-foreground mt-1">Số tài khoản: <span className="font-semibold font-mono text-foreground">{item.accountNo}</span></p>
                    <p className="text-[11px] text-muted-foreground mt-0.5">Tên: <span className="font-semibold">{item.accountName}</span> · Mẫu: <span className="font-mono">{item.vietQrTemplate}</span></p>
                  </div>
                  <div className="flex gap-1.5">
                    {!item.isDefault && item.isActive && (
                      <button onClick={() => handleSetDefault(item.id)} className="text-[10px] font-semibold text-primary border border-primary/20 hover:bg-primary/10 px-2 py-1 rounded">Đặt mặc định</button>
                    )}
                    <button onClick={() => handleEdit(item)} className="text-[10px] font-semibold text-foreground border border-border hover:bg-muted px-2 py-1 rounded">Sửa</button>
                    {!item.isDefault && (
                      <button onClick={() => handleDelete(item.id)} className="text-[10px] font-semibold text-red-600 border border-red-200 hover:bg-red-50 px-2 py-1 rounded">Xóa</button>
                    )}
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        <div className="px-6 py-4 border-t border-border flex justify-end">
          <button onClick={onClose} className="px-4 py-2 text-sm rounded-lg border border-border hover:bg-muted transition-colors">Đóng</button>
        </div>
      </div>
    </div>
  );
}
