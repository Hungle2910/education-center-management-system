"use client";

import { useState } from "react";
import { useTuitionReport, useClassReport, useTeacherReport } from "./reports-api";
import { exportTuitionReport, exportClassReport, exportTeacherReport } from "../excel/excel-api";

export function ReportsView() {
  const [activeTab, setActiveTab] = useState<"tuition" | "classes" | "teachers">("tuition");

  const { data: tuition, isLoading: isTuitionLoading } = useTuitionReport();
  const { data: classes, isLoading: isClassesLoading } = useClassReport();
  const { data: teachers, isLoading: isTeachersLoading } = useTeacherReport();

  const formatCurrency = (amount: number) => {
    return amount.toLocaleString("vi-VN") + " đ";
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div>
        <h1 className="text-2xl font-bold tracking-tight">Báo cáo & Thống kê</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Theo dõi doanh thu trung tâm, hiệu suất lớp học và tổng hợp chi phí giảng dạy.
        </p>
      </div>

      {/* Tabs */}
      <div className="flex border-b border-border mb-6">
        <button
          onClick={() => setActiveTab("tuition")}
          className={`pb-3 text-sm font-semibold border-b-2 px-6 transition cursor-pointer ${
            activeTab === "tuition"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
        >
          Doanh thu & Học phí
        </button>
        <button
          onClick={() => setActiveTab("classes")}
          className={`pb-3 text-sm font-semibold border-b-2 px-6 transition cursor-pointer ${
            activeTab === "classes"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
        >
          Hiệu suất lớp học
        </button>
        <button
          onClick={() => setActiveTab("teachers")}
          className={`pb-3 text-sm font-semibold border-b-2 px-6 transition cursor-pointer ${
            activeTab === "teachers"
              ? "border-primary text-primary"
              : "border-transparent text-muted-foreground hover:text-foreground"
          }`}
        >
          Lương & Buổi dạy
        </button>
      </div>

      {/* Tab Contents */}
      {activeTab === "tuition" && (
        <div className="space-y-6">
          {isTuitionLoading ? (
            <p className="text-sm text-muted-foreground text-center py-10">Đang tải báo cáo doanh thu...</p>
          ) : (
            <>
              <div className="flex justify-end">
                <button
                  onClick={exportTuitionReport}
                  className="inline-flex items-center justify-center rounded-md border border-border bg-surface px-4 py-2 text-sm font-semibold hover:bg-slate-50 transition shadow-sm cursor-pointer"
                >
                  📥 Xuất Excel Doanh thu
                </button>
              </div>

              {/* Stats cards */}
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Doanh thu đã thu</h3>
                  <p className="text-2xl font-bold mt-2 text-emerald-600">
                    {formatCurrency(tuition?.totalCollected ?? 0)}
                  </p>
                </div>
                <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Còn nợ (Chưa thu)</h3>
                  <p className="text-2xl font-bold mt-2 text-amber-600">
                    {formatCurrency(tuition?.totalUnpaid ?? 0)}
                  </p>
                </div>
                <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
                  <h3 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">Hóa đơn quá hạn</h3>
                  <p className="text-2xl font-bold mt-2 text-red-600">
                    {formatCurrency(tuition?.totalOverdue ?? 0)}
                  </p>
                </div>
              </div>

              {/* Detail charts / lists */}
              <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
                  <h2 className="text-base font-bold mb-4">Doanh thu theo lớp</h2>
                  {tuition?.revenueByClass && tuition.revenueByClass.length > 0 ? (
                    <div className="space-y-3">
                      {tuition.revenueByClass.map((item) => (
                        <div key={item.classId} className="flex items-center justify-between border-b border-border/50 pb-2 last:border-none">
                          <span className="text-sm font-medium text-foreground">{item.className}</span>
                          <span className="text-sm font-bold text-emerald-600">{formatCurrency(item.revenue)}</span>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-xs text-muted-foreground text-center py-6">Chưa ghi nhận doanh thu theo lớp.</p>
                  )}
                </div>

                <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
                  <h2 className="text-base font-bold mb-4">Doanh thu theo tháng</h2>
                  {tuition?.revenueByMonth && tuition.revenueByMonth.length > 0 ? (
                    <div className="space-y-3">
                      {tuition.revenueByMonth.map((item) => (
                        <div key={item.month} className="flex items-center justify-between border-b border-border/50 pb-2 last:border-none">
                          <span className="text-sm font-medium text-foreground">Tháng {item.month}</span>
                          <span className="text-sm font-bold text-indigo-600">{formatCurrency(item.revenue)}</span>
                        </div>
                      ))}
                    </div>
                  ) : (
                    <p className="text-xs text-muted-foreground text-center py-6">Chưa ghi nhận doanh thu theo tháng.</p>
                  )}
                </div>
              </div>
            </>
          )}
        </div>
      )}

      {activeTab === "classes" && (
        <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-base font-bold">Hiệu suất lớp học</h2>
            <button
              onClick={exportClassReport}
              className="inline-flex items-center justify-center rounded-md border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 transition shadow-sm cursor-pointer"
            >
              📥 Xuất Excel Hiệu suất
            </button>
          </div>
          {isClassesLoading ? (
            <p className="text-sm text-muted-foreground text-center py-10">Đang tải báo cáo lớp học...</p>
          ) : classes && classes.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse text-sm">
                <thead>
                  <tr className="border-b border-border text-muted-foreground text-xs font-semibold">
                    <th className="pb-3">Lớp học</th>
                    <th className="pb-3">Sĩ số thực tế</th>
                    <th className="pb-3">Sĩ số mục tiêu</th>
                    <th className="pb-3">Trạng thái lớp</th>
                    <th className="pb-3 text-right">Rủi ro tài chính</th>
                  </tr>
                </thead>
                <tbody>
                  {classes.map((c) => (
                    <tr key={c.classId} className="border-b border-border/50 last:border-none hover:bg-slate-50/50">
                      <td className="py-3 font-medium text-foreground">{c.className}</td>
                      <td className="py-3 text-muted-foreground">{c.activeStudentCount} học sinh</td>
                      <td className="py-3 text-muted-foreground">{c.targetStudentCount} học sinh</td>
                      <td className="py-3">
                        <span className="inline-flex px-2 py-0.5 rounded-full text-[10px] font-semibold bg-indigo-50 text-indigo-700 border border-indigo-200">
                          {c.status}
                        </span>
                      </td>
                      <td className="py-3 text-right">
                        {c.isAtRiskOfLoss ? (
                          <span className="inline-flex px-2 py-0.5 rounded-full text-[10px] font-semibold bg-red-50 text-red-700 border border-red-200">
                            ⚠️ Nguy cơ lỗ
                          </span>
                        ) : (
                          <span className="inline-flex px-2 py-0.5 rounded-full text-[10px] font-semibold bg-green-50 text-green-700 border border-green-200">
                            Ổn định
                          </span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-xs text-muted-foreground text-center py-6">Không tìm thấy thông tin lớp học nào.</p>
          )}
        </div>
      )}

      {activeTab === "teachers" && (
        <div className="p-6 rounded-xl border border-border bg-surface shadow-sm">
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-base font-bold">Lương & Hiệu suất giảng dạy</h2>
            <button
              onClick={exportTeacherReport}
              className="inline-flex items-center justify-center rounded-md border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 transition shadow-sm cursor-pointer"
            >
              📥 Xuất Excel Bảng lương
            </button>
          </div>
          {isTeachersLoading ? (
            <p className="text-sm text-muted-foreground text-center py-10">Đang tải báo cáo giáo viên...</p>
          ) : teachers && teachers.length > 0 ? (
            <div className="overflow-x-auto">
              <table className="w-full text-left border-collapse text-sm">
                <thead>
                  <tr className="border-b border-border text-muted-foreground text-xs font-semibold">
                    <th className="pb-3">Giáo viên</th>
                    <th className="pb-3">Buổi đã dạy</th>
                    <th className="pb-3">Buổi nghỉ</th>
                    <th className="pb-3">Buổi dạy bù</th>
                    <th className="pb-3">Lương dự kiến</th>
                    <th className="pb-3 text-right">Lương đã thanh toán</th>
                  </tr>
                </thead>
                <tbody>
                  {teachers.map((t) => (
                    <tr key={t.teacherId} className="border-b border-border/50 last:border-none hover:bg-slate-50/50">
                      <td className="py-3 font-medium text-foreground">{t.teacherName}</td>
                      <td className="py-3 text-muted-foreground">{t.completedLessonsCount} buổi</td>
                      <td className="py-3 text-muted-foreground">{t.cancelledLessonsCount} buổi</td>
                      <td className="py-3 text-muted-foreground">{t.makeupLessonsCount} buổi</td>
                      <td className="py-3 font-semibold text-primary">{formatCurrency(t.projectedSalary)}</td>
                      <td className="py-3 text-right font-semibold text-emerald-600">{formatCurrency(t.paidSalary)}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          ) : (
            <p className="text-xs text-muted-foreground text-center py-6">Không tìm thấy thông tin giáo viên nào.</p>
          )}
        </div>
      )}
    </div>
  );
}
