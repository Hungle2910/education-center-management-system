"use client";

import { useRef, useState } from "react";
import { downloadFile, useImportStudents } from "./excel-api";

export function ExcelActions() {
  const [selectedMonth, setSelectedMonth] = useState(() => {
    const d = new Date();
    return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, "0")}`;
  });
  
  const fileInputRef = useRef<HTMLInputElement>(null);
  const importStudentsMutation = useImportStudents();
  const [importResult, setImportResult] = useState<{
    success: boolean;
    message: string;
    importedCount?: number;
    errors?: string[];
  } | null>(null);

  const handleFileChange = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    setImportResult(null);
    try {
      const response = await importStudentsMutation.mutateAsync(file);
      if (response.success) {
        setImportResult({
          success: true,
          message: response.message,
          importedCount: (response.data as any).importedCount,
          errors: (response.data as any).errors,
        });
      } else {
        setImportResult({
          success: false,
          message: response.message || "Nhập danh sách học sinh thất bại.",
          errors: response.errors,
        });
      }
    } catch (err: any) {
      setImportResult({
        success: false,
        message: err?.response?.data?.message || "Đã xảy ra lỗi hệ thống khi nhập Excel.",
        errors: err?.response?.data?.errors,
      });
    }

    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <div className="p-6 rounded-xl border border-border bg-surface shadow-sm space-y-6">
      <div>
        <h2 className="text-base font-bold text-foreground">Nhập & Xuất Dữ liệu Excel</h2>
        <p className="text-xs text-muted-foreground mt-0.5">
          Tải file mẫu nhập học sinh hoặc kết xuất dữ liệu báo cáo trung tâm ra file Excel (.xlsx).
        </p>
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Left column: Imports */}
        <div className="space-y-4 border-r border-border/50 pr-0 lg:pr-6">
          <h3 className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Nhập Học sinh Hàng loạt</h3>
          
          <div className="flex flex-wrap items-center gap-3">
            <button
              onClick={() => downloadFile("/excel/templates/students", "Mau_Nhap_Hoc_Sinh.xlsx")}
              className="inline-flex items-center justify-center rounded-md border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
            >
              📥 Tải File Excel Mẫu
            </button>

            <label className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-xs font-semibold text-primary-foreground hover:bg-primary/95 transition shadow-sm cursor-pointer">
              📁 Chọn File Excel Tải Lên
              <input
                type="file"
                ref={fileInputRef}
                accept=".xlsx"
                className="hidden"
                onChange={handleFileChange}
                disabled={importStudentsMutation.isPending}
              />
            </label>
          </div>

          {importStudentsMutation.isPending && (
            <p className="text-xs text-indigo-600 animate-pulse font-medium">Đang xử lý dữ liệu và tạo hồ sơ học sinh...</p>
          )}

          {importResult && (
            <div className={`p-4 rounded-lg text-xs space-y-2 ${importResult.success ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-red-50 text-red-800 border border-red-200"}`}>
              <p className="font-bold">{importResult.message}</p>
              {importResult.importedCount !== undefined && (
                <p>Số lượng học sinh đã lưu: <strong className="text-emerald-700">{importResult.importedCount} học sinh</strong></p>
              )}
              {importResult.errors && importResult.errors.length > 0 && (
                <div className="space-y-1 mt-2">
                  <p className="font-semibold text-amber-900">Chi tiết cảnh báo/lỗi:</p>
                  <ul className="list-disc pl-4 space-y-0.5 max-h-32 overflow-y-auto">
                    {importResult.errors.map((err, i) => (
                      <li key={i}>{err}</li>
                    ))}
                  </ul>
                </div>
              )}
            </div>
          )}
        </div>

        {/* Right column: Exports */}
        <div className="space-y-4">
          <h3 className="text-xs font-bold uppercase tracking-wider text-muted-foreground">Xuất Dữ liệu CRM</h3>

          <div className="flex flex-col sm:flex-row sm:items-center gap-3">
            <button
              onClick={() => downloadFile("/excel/export/students", "Danh_Sach_Hoc_Sinh.xlsx")}
              className="inline-flex items-center justify-center rounded-md border border-border bg-surface px-4 py-2.5 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
            >
              📋 Xuất Học sinh (.xlsx)
            </button>

            <button
              onClick={() => downloadFile("/excel/export/classes", "Danh_Sach_Lop_Hoc.xlsx")}
              className="inline-flex items-center justify-center rounded-md border border-border bg-surface px-4 py-2.5 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
            >
              🏫 Xuất Lớp học (.xlsx)
            </button>
          </div>

          <div className="flex items-center gap-2 pt-2 border-t border-border/40">
            <div className="flex-1 max-w-[150px]">
              <input
                type="month"
                value={selectedMonth}
                onChange={(e) => setSelectedMonth(e.target.value)}
                className="flex h-9 w-full rounded border border-border bg-surface px-2 py-1 text-xs focus:outline-none focus:ring-1 focus:ring-primary"
              />
            </div>
            <button
              onClick={() => downloadFile(`/excel/export/invoices?month=${selectedMonth}`, `Bao_Cao_Hoc_Phi_${selectedMonth}.xlsx`)}
              className="inline-flex h-9 items-center justify-center rounded bg-indigo-600 px-4 text-xs font-semibold text-white hover:bg-indigo-700 transition cursor-pointer shadow-sm"
            >
              💰 Xuất Học phí Tháng
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
