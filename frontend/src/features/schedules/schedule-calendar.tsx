"use client";

import { useState } from "react";
import { useCalendar } from "./schedule-api";
import { useTeachers, useClasses } from "@/features/classes/class-api";
import { ScheduleFormModal } from "./schedule-form-modal";
import { AttendanceSheetModal } from "@/features/attendance/attendance-sheet-modal";
import { MakeupModal } from "./makeup-modal";
import { useAuth } from "@/features/auth/auth-context";

// Helper to format DateOnly to string
const formatDate = (date: Date) => {
  return date.toISOString().split("T")[0];
};

export function ScheduleCalendar() {
  const { user } = useAuth();
  const [viewMode, setViewMode] = useState<"week" | "month">("week");
  const [currentDate, setCurrentDate] = useState(new Date());
  const [teacherFilter, setTeacherFilter] = useState<string>("ALL");
  const [classFilter, setClassFilter] = useState<string>("ALL");
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selectedOccurrenceId, setSelectedOccurrenceId] = useState<string | null>(null);
  const [makeupOccurrence, setMakeupOccurrence] = useState<{ id: string; className: string; date: string } | null>(null);
  const [showIcalModal, setShowIcalModal] = useState(false);
  const [copied, setCopied] = useState(false);

  const icalUrl = user
    ? `${process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5088/api"}/schedules/ical/${user.id}`
    : "";

  const handleCopyLink = () => {
    if (icalUrl) {
      navigator.clipboard.writeText(icalUrl);
      setCopied(true);
      setTimeout(() => setCopied(false), 2000);
    }
  };

  // Calculate start/end date for weekly view (Monday to Sunday)
  const getWeekRange = (date: Date) => {
    const currentDay = date.getDay();
    const distanceToMonday = currentDay === 0 ? -6 : 1 - currentDay;
    const monday = new Date(date);
    monday.setDate(date.getDate() + distanceToMonday);
    
    const sunday = new Date(monday);
    sunday.setDate(monday.getDate() + 6);
    
    return { monday, sunday };
  };

  // Calculate start/end date for monthly view
  const getMonthRange = (date: Date) => {
    const firstDay = new Date(date.getFullYear(), date.getMonth(), 1);
    const lastDay = new Date(date.getFullYear(), date.getMonth() + 1, 0);
    return { firstDay, lastDay };
  };

  const { monday, sunday } = getWeekRange(currentDate);
  const { firstDay, lastDay } = getMonthRange(currentDate);

  const startDateStr = viewMode === "week" ? formatDate(monday) : formatDate(firstDay);
  const endDateStr = viewMode === "week" ? formatDate(sunday) : formatDate(lastDay);

  const { data: occurrences, isLoading } = useCalendar(startDateStr, endDateStr);
  const { data: teachers } = useTeachers();
  const { data: classes } = useClasses();

  // Filter occurrences
  const filteredOccurrences = occurrences?.filter((o) => {
    const matchesTeacher = teacherFilter === "ALL" || o.teacherId === teacherFilter;
    const matchesClass = classFilter === "ALL" || o.classId === classFilter;
    return matchesTeacher && matchesClass;
  });

  const handlePrev = () => {
    const nextDate = new Date(currentDate);
    if (viewMode === "week") {
      nextDate.setDate(currentDate.getDate() - 7);
    } else {
      nextDate.setMonth(currentDate.getMonth() - 1);
    }
    setCurrentDate(nextDate);
  };

  const handleNext = () => {
    const nextDate = new Date(currentDate);
    if (viewMode === "week") {
      nextDate.setDate(currentDate.getDate() + 7);
    } else {
      nextDate.setMonth(currentDate.getMonth() + 1);
    }
    setCurrentDate(nextDate);
  };

  const handleToday = () => {
    setCurrentDate(new Date());
  };

  // Generate days array for week view
  const getWeekDays = () => {
    const days = [];
    const start = new Date(monday);
    for (let i = 0; i < 7; i++) {
      days.push(new Date(start));
      start.setDate(start.getDate() + 1);
    }
    return days;
  };

  const weekDays = getWeekDays();

  // Format month label in Vietnamese
  const getMonthYearLabel = () => {
    return `Tháng ${currentDate.getMonth() + 1}, ${currentDate.getFullYear()}`;
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight">Lịch học</h1>
          <p className="text-sm text-muted-foreground mt-1">
            Theo dõi lịch giảng dạy, phòng học và trạng thái các buổi học thực tế.
          </p>
        </div>
        <div className="flex items-center gap-3">
          <button
            onClick={() => setShowIcalModal(true)}
            className="inline-flex items-center justify-center rounded-md border border-border bg-surface px-4 py-2 text-sm font-semibold hover:bg-slate-50 transition shadow-sm cursor-pointer"
          >
            📅 Đồng bộ lịch cá nhân
          </button>
          <button
            onClick={() => setIsModalOpen(true)}
            className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow transition-colors hover:bg-primary/90 cursor-pointer"
          >
            Xếp lịch học
          </button>
        </div>
      </div>

      {/* Filters & Navigation Controls */}
      <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 bg-surface p-4 rounded-lg border border-border shadow-sm">
        {/* Navigation */}
        <div className="flex items-center gap-2">
          <button
            onClick={handlePrev}
            className="p-2 border border-border rounded-md hover:bg-slate-50 cursor-pointer"
          >
            &lt;
          </button>
          <button
            onClick={handleToday}
            className="px-3 py-2 text-sm font-semibold border border-border rounded-md hover:bg-slate-50 cursor-pointer"
          >
            Hôm nay
          </button>
          <button
            onClick={handleNext}
            className="p-2 border border-border rounded-md hover:bg-slate-50 cursor-pointer"
          >
            &gt;
          </button>
          <span className="font-semibold text-sm ml-2">{getMonthYearLabel()}</span>
        </div>

        {/* View Mode Toggle */}
        <div className="flex items-center border border-border rounded-md p-1 bg-background">
          <button
            onClick={() => setViewMode("week")}
            className={`px-3 py-1.5 text-xs font-semibold rounded ${
              viewMode === "week" ? "bg-surface shadow-sm text-primary" : "text-muted-foreground"
            }`}
          >
            Tuần
          </button>
          <button
            onClick={() => setViewMode("month")}
            className={`px-3 py-1.5 text-xs font-semibold rounded ${
              viewMode === "month" ? "bg-surface shadow-sm text-primary" : "text-muted-foreground"
            }`}
          >
            Tháng
          </button>
        </div>

        {/* Dropdown Filters */}
        <div className="flex flex-wrap items-center gap-2">
          <select
            value={classFilter}
            onChange={(e) => setClassFilter(e.target.value)}
            className="h-9 rounded-md border border-border bg-surface px-2.5 py-1 text-xs focus:ring-primary focus:outline-none"
          >
            <option value="ALL">Tất cả lớp học</option>
            {classes?.map((c) => (
              <option key={c.id} value={c.id}>
                {c.name}
              </option>
            ))}
          </select>

          <select
            value={teacherFilter}
            onChange={(e) => setTeacherFilter(e.target.value)}
            className="h-9 rounded-md border border-border bg-surface px-2.5 py-1 text-xs focus:ring-primary focus:outline-none"
          >
            <option value="ALL">Tất cả giáo viên</option>
            {teachers?.map((t) => (
              <option key={t.id} value={t.id}>
                {t.fullName}
              </option>
            ))}
          </select>
        </div>
      </div>

      {isLoading ? (
        <div className="text-center py-10 text-muted-foreground">Đang tải lịch học...</div>
      ) : (
        /* Calendar Layout */
        <div className="bg-surface rounded-lg border border-border shadow-sm p-4">
          {viewMode === "week" ? (
            /* Weekly View Grid */
            <div className="grid grid-cols-1 md:grid-cols-7 gap-4 divide-y md:divide-y-0 md:divide-x divide-border">
              {weekDays.map((day, idx) => {
                const dayStr = formatDate(day);
                const dayOccurrences = filteredOccurrences?.filter((o) => o.date === dayStr) || [];
                const isToday = formatDate(new Date()) === dayStr;

                const dayLabel = ["Chủ Nhật", "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5", "Thứ 6", "Thứ 7"][day.getDay()];

                return (
                  <div key={dayStr} className="pt-4 md:pt-0 md:px-2 first:pl-0 last:pr-0 min-h-[300px]">
                    <div className="text-center border-b border-border pb-2">
                      <span className={`block text-xs font-semibold ${isToday ? "text-primary" : "text-muted-foreground"}`}>
                        {dayLabel}
                      </span>
                      <span className={`inline-flex items-center justify-center h-7 w-7 text-sm font-bold rounded-full mt-1 ${
                        isToday ? "bg-primary text-primary-foreground" : ""
                      }`}>
                        {day.getDate()}
                      </span>
                    </div>

                    <div className="mt-3 space-y-2">
                      {dayOccurrences.map((o) => (
                        <div
                          key={o.id}
                          className="p-2.5 rounded-lg border border-border bg-background shadow-xs hover:border-primary/50 transition-colors"
                        >
                          <p className="font-semibold text-xs text-primary">{o.className}</p>
                          <p className="text-[10px] text-muted-foreground mt-0.5">
                            {o.startTime.substring(0, 5)} - {o.endTime.substring(0, 5)}
                          </p>
                          <p className="text-[10px] text-muted-foreground mt-0.5 truncate">
                            🚪 {o.roomName}
                          </p>
                          <p className="text-[10px] text-muted-foreground mt-0.5 truncate">
                            👤 {o.teacherName ?? "Chưa phân công"}
                          </p>
                          <span className={`inline-flex items-center rounded px-1.5 py-0.5 text-[9px] font-semibold mt-1.5 ${
                            o.status === "Đã học"
                              ? "bg-green-50 text-green-700 border border-green-200"
                              : o.status === "Đã nghỉ"
                              ? "bg-red-50 text-red-700 border border-red-200"
                              : "bg-blue-50 text-blue-700 border border-blue-200"
                          }`}>
                            {o.status}
                          </span>
                          <div className="flex items-center justify-between gap-2 mt-2 pt-2 border-t border-border/50">
                            <button
                              onClick={() => setSelectedOccurrenceId(o.id)}
                              className="text-[10px] font-semibold text-primary hover:text-primary/80 transition-colors cursor-pointer"
                            >
                              Điểm danh
                            </button>
                            {o.status !== "Đã hủy" && (
                              <button
                                onClick={() => setMakeupOccurrence({ id: o.id, className: o.className, date: o.date })}
                                className="text-[10px] font-semibold text-amber-600 hover:text-amber-500 transition-colors cursor-pointer"
                              >
                                Học bù/Hủy
                              </button>
                            )}
                          </div>
                        </div>
                      ))}
                      {dayOccurrences.length === 0 && (
                        <p className="text-center text-[10px] text-muted-foreground italic py-4">Trống</p>
                      )}
                    </div>
                  </div>
                );
              })}
            </div>
          ) : (
            /* Simple Monthly View List (To keep frontend code lightweight & robust) */
            <div className="space-y-4">
              <h3 className="text-sm font-semibold text-foreground">Danh sách buổi học trong tháng</h3>
              <div className="divide-y divide-border">
                {filteredOccurrences && filteredOccurrences.length > 0 ? (
                  filteredOccurrences.map((o) => (
                    <div key={o.id} className="py-3 flex items-center justify-between gap-4">
                      <div>
                        <p className="font-semibold text-sm">{o.className}</p>
                        <p className="text-xs text-muted-foreground mt-0.5">
                          📅 {o.date} | ⏰ {o.startTime.substring(0, 5)} - {o.endTime.substring(0, 5)}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-xs font-semibold">{o.roomName}</p>
                        <p className="text-xs text-muted-foreground">{o.teacherName ?? "Chưa phân công"}</p>
                        <div className="flex items-center justify-end gap-2 mt-1">
                          <span className={`inline-flex items-center rounded-md px-2 py-0.5 text-[10px] font-semibold ${
                            o.status === "Đã học" ? "bg-green-50 text-green-700" : "bg-blue-50 text-blue-700"
                          }`}>
                            {o.status}
                          </span>
                          <button
                            onClick={() => setSelectedOccurrenceId(o.id)}
                            className="text-xs font-semibold text-primary hover:text-primary/80 transition-colors cursor-pointer"
                          >
                            Điểm danh
                          </button>
                          {o.status !== "Đã hủy" && (
                            <button
                              onClick={() => setMakeupOccurrence({ id: o.id, className: o.className, date: o.date })}
                              className="text-xs font-semibold text-amber-600 hover:text-amber-500 transition-colors cursor-pointer"
                            >
                              Học bù/Hủy
                            </button>
                          )}
                        </div>
                      </div>
                    </div>
                  ))
                ) : (
                  <p className="text-center text-sm text-muted-foreground py-8 italic">Không có lịch học nào trong tháng.</p>
                )}
              </div>
            </div>
          )}
        </div>
      )}

      {isModalOpen && <ScheduleFormModal onClose={() => setIsModalOpen(false)} />}
      {selectedOccurrenceId && (
        <AttendanceSheetModal
          occurrenceId={selectedOccurrenceId}
          onClose={() => setSelectedOccurrenceId(null)}
        />
      )}
      {makeupOccurrence && (
        <MakeupModal
          occurrenceId={makeupOccurrence.id}
          className={makeupOccurrence.className}
          date={makeupOccurrence.date}
          onClose={() => setMakeupOccurrence(null)}
        />
      )}

      {showIcalModal && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/40 backdrop-blur-sm p-4">
          <div className="w-full max-w-lg bg-surface rounded-xl border border-border p-6 shadow-xl space-y-4 animate-in fade-in zoom-in-95 duration-150">
            <div className="flex items-center justify-between">
              <h3 className="text-base font-bold text-foreground">📅 Đồng bộ Lịch học cá nhân</h3>
              <button
                onClick={() => setShowIcalModal(false)}
                className="text-muted-foreground hover:text-foreground text-lg cursor-pointer"
              >
                ✕
              </button>
            </div>
            
            <p className="text-xs text-muted-foreground leading-relaxed">
              Bạn có thể đồng bộ thời gian biểu học tập/giảng dạy của mình với Google Calendar, Apple Calendar, Microsoft Outlook hoặc các ứng dụng lịch di động khác bằng link iCal dưới đây:
            </p>

            <div className="flex items-stretch gap-2">
              <input
                type="text"
                readOnly
                value={icalUrl}
                className="flex-1 rounded border border-border bg-slate-50 px-3 py-2 text-xs font-mono text-muted-foreground focus:outline-none"
              />
              <button
                onClick={handleCopyLink}
                className="rounded bg-primary text-primary-foreground px-4 text-xs font-semibold hover:bg-primary/95 transition cursor-pointer shadow-sm"
              >
                {copied ? "Đã copy" : "Copy Link"}
              </button>
            </div>

            <div className="bg-slate-50 p-4 rounded-lg space-y-2.5 text-xs text-muted-foreground border border-slate-100">
              <p className="font-bold text-slate-800">Hướng dẫn thêm vào Google Calendar:</p>
              <ol className="list-decimal pl-4 space-y-1">
                <li>Truy cập <a href="https://calendar.google.com" target="_blank" rel="noreferrer" className="text-primary hover:underline font-semibold">Google Calendar</a>.</li>
                <li>Ở cột bên trái, tìm mục <strong>"Lịch khác" (Other calendars)</strong> và click nút dấu <strong>+</strong>.</li>
                <li>Chọn <strong>"Từ URL" (From URL)</strong>.</li>
                <li>Dán đường link iCal đã copy ở trên vào và bấm <strong>"Thêm lịch" (Add calendar)</strong>.</li>
              </ol>
            </div>

            <div className="flex justify-end">
              <button
                onClick={() => setShowIcalModal(false)}
                className="rounded border border-border bg-surface px-4 py-2 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
              >
                Đóng
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
