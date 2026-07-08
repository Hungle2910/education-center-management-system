"use client";

import { useForm } from "react-hook-form";
import { CreateScheduleRequest } from "./schedule-types";
import { useCreateSchedule, checkConflicts } from "./schedule-api";
import { useTeachers, useClasses } from "@/features/classes/class-api";
import { useEffect, useState } from "react";
import { apiClient } from "@/lib/http/api-client";

interface ScheduleFormModalProps {
  onClose: () => void;
}

interface RoomDto {
  id: string;
  name: string;
  capacity: number;
}

export function ScheduleFormModal({ onClose }: ScheduleFormModalProps) {
  const createSchedule = useCreateSchedule();
  const { data: teachers } = useTeachers();
  const { data: classes } = useClasses();
  const [rooms, setRooms] = useState<RoomDto[]>([]);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  // Fetch rooms on component mount
  useEffect(() => {
    apiClient.get("/rooms").then((res) => {
      setRooms(res.data.data);
    });
  }, []);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<CreateScheduleRequest>({
    defaultValues: {
      dayOfWeek: 1, // Thứ 2
      startTime: "08:00",
      endTime: "09:30",
    },
  });

  const onSubmit = async (data: CreateScheduleRequest) => {
    try {
      setErrorMessage(null);

      // Perform pre-submit conflict check for next week's date matching this DayOfWeek
      const today = new Date();
      const targetDay = Number(data.dayOfWeek);
      const daysToAdd = (targetDay - today.getDay() + 7) % 7;
      const targetDate = new Date(today);
      targetDate.setDate(today.getDate() + daysToAdd);
      const dateStr = targetDate.toISOString().split("T")[0];

      // Call API check conflicts
      const conflictResult = await checkConflicts({
        date: dateStr,
        startTime: data.startTime,
        endTime: data.endTime,
        roomId: data.roomId,
        teacherId: data.teacherId || null,
      });

      if (conflictResult.hasConflict) {
        setErrorMessage(conflictResult.message ?? "Phòng hoặc Giáo viên đang bị trùng lịch.");
        return;
      }

      // Proceed to create schedule
      const payload = {
        ...data,
        dayOfWeek: Number(data.dayOfWeek),
        teacherId: data.teacherId || null,
      };

      await createSchedule.mutateAsync(payload);
      onClose();
    } catch (err: any) {
      const apiError = err.response?.data?.message ?? "Đã xảy ra lỗi khi tạo lịch học.";
      setErrorMessage(apiError);
    }
  };

  const daysOfWeekOptions = [
    { label: "Thứ Hai", value: 1 },
    { label: "Thứ Ba", value: 2 },
    { label: "Thứ Tư", value: 3 },
    { label: "Thứ Năm", value: 4 },
    { label: "Thứ Sáu", value: 5 },
    { label: "Thứ Bảy", value: 6 },
    { label: "Chủ Nhật", value: 0 },
  ];

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4">
      <div className="w-full max-w-lg rounded-lg border border-border bg-surface p-6 shadow-lg animate-in fade-in zoom-in duration-200">
        <div className="flex items-center justify-between border-b border-border pb-3 mb-4">
          <h2 className="text-lg font-bold text-foreground">Xếp lịch học cho lớp</h2>
          <button onClick={onClose} className="text-muted-foreground hover:text-foreground text-sm cursor-pointer">
            Đóng
          </button>
        </div>

        {errorMessage && (
          <div className="mb-4 p-3 rounded-md bg-red-50 text-xs text-red-600 border border-red-200">
            {errorMessage}
          </div>
        )}

        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          {/* Lớp học */}
          <div>
            <label className="block text-xs font-semibold mb-1" htmlFor="classId">
              Chọn Lớp học <span className="text-red-500">*</span>
            </label>
            <select
              id="classId"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
              {...register("classId", { required: "Vui lòng chọn lớp học." })}
            >
              <option value="">-- Chọn lớp học --</option>
              {classes?.map((c) => (
                <option key={c.id} value={c.id}>
                  {c.name}
                </option>
              ))}
            </select>
          </div>

          {/* Thứ trong tuần */}
          <div>
            <label className="block text-xs font-semibold mb-1" htmlFor="dayOfWeek">
              Chọn Thứ trong tuần <span className="text-red-500">*</span>
            </label>
            <select
              id="dayOfWeek"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
              {...register("dayOfWeek", { required: true })}
            >
              {daysOfWeekOptions.map((opt) => (
                <option key={opt.value} value={opt.value}>
                  {opt.label}
                </option>
              ))}
            </select>
          </div>

          {/* Time range */}
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-xs font-semibold mb-1" htmlFor="startTime">
                Giờ bắt đầu <span className="text-red-500">*</span>
              </label>
              <input
                id="startTime"
                type="time"
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                {...register("startTime", { required: true })}
              />
            </div>
            <div>
              <label className="block text-xs font-semibold mb-1" htmlFor="endTime">
                Giờ kết thúc <span className="text-red-500">*</span>
              </label>
              <input
                id="endTime"
                type="time"
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                {...register("endTime", { required: true })}
              />
            </div>
          </div>

          {/* Phòng học */}
          <div>
            <label className="block text-xs font-semibold mb-1" htmlFor="roomId">
              Phòng học <span className="text-red-500">*</span>
            </label>
            <select
              id="roomId"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
              {...register("roomId", { required: "Vui lòng chọn phòng học." })}
            >
              <option value="">-- Chọn phòng học --</option>
              {rooms.map((r) => (
                <option key={r.id} value={r.id}>
                  {r.name} (Sức chứa: {r.capacity} HS)
                </option>
              ))}
            </select>
          </div>

          {/* Giáo viên */}
          <div>
            <label className="block text-xs font-semibold mb-1" htmlFor="teacherId">
              Giáo viên giảng dạy (Tùy chọn)
            </label>
            <select
              id="teacherId"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
              {...register("teacherId")}
            >
              <option value="">-- Mặc định giáo viên của lớp --</option>
              {teachers?.map((t) => (
                <option key={t.id} value={t.id}>
                  {t.fullName} ({t.subject ?? "Chưa rõ"})
                </option>
              ))}
            </select>
          </div>

          <div className="flex items-center justify-end gap-3 pt-4 border-t border-border mt-6">
            <button
              type="button"
              onClick={onClose}
              className="h-9 rounded-md border border-border bg-surface px-4 text-xs font-semibold hover:bg-slate-50 transition cursor-pointer"
            >
              Hủy bỏ
            </button>
            <button
              type="submit"
              disabled={isSubmitting}
              className="h-9 rounded-md bg-primary text-primary-foreground px-4 text-xs font-semibold hover:bg-primary/90 transition shadow disabled:opacity-50 cursor-pointer"
            >
              {isSubmitting ? "Đang lưu..." : "Xác nhận xếp lịch"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
