"use client";

import { useForm } from "react-hook-form";
import { TeacherRequest, TeacherResponse } from "@/features/classes/class-types";
import { useCreateTeacher, useUpdateTeacher } from "@/features/classes/class-api";
import { useState } from "react";

interface TeacherFormProps {
  teacher?: TeacherResponse;
  onClose: () => void;
}

export function TeacherForm({ teacher, onClose }: TeacherFormProps) {
  const isEdit = !!teacher;
  const createTeacher = useCreateTeacher();
  const updateTeacher = useUpdateTeacher({ id: teacher?.id ?? "" });
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<TeacherRequest>({
    defaultValues: {
      fullName: teacher?.fullName ?? "",
      email: teacher?.email ?? "",
      phoneNumber: teacher?.phoneNumber ?? "",
      subject: teacher?.subject ?? "",
      isActive: teacher?.isActive ?? true,
    },
  });

  const onSubmit = async (data: TeacherRequest) => {
    try {
      setErrorMessage(null);
      if (isEdit) {
        await updateTeacher.mutateAsync(data);
      } else {
        await createTeacher.mutateAsync(data);
      }
      onClose();
    } catch (err: any) {
      const apiError = err.response?.data?.message ?? "Đã xảy ra lỗi khi lưu thông tin.";
      const detailErrors = err.response?.data?.errors?.join(", ");
      setErrorMessage(detailErrors ? `${apiError}: ${detailErrors}` : apiError);
    }
  };

  return (
    <div className="max-w-2xl mx-auto rounded-lg border border-border bg-surface p-6 shadow-sm">
      <div className="flex items-center justify-between border-b border-border pb-4 mb-6">
        <h2 className="text-xl font-bold tracking-tight text-foreground">
          {isEdit ? "Chỉnh sửa thông tin giáo viên" : "Thêm mới giáo viên"}
        </h2>
        <button
          onClick={onClose}
          className="text-muted-foreground hover:text-foreground text-sm cursor-pointer"
        >
          Quay lại
        </button>
      </div>

      {errorMessage && (
        <div className="mb-6 p-4 rounded-md bg-red-50 text-sm text-red-600 border border-red-200">
          {errorMessage}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {/* Full Name */}
        <div>
          <label className="block text-sm font-semibold mb-1" htmlFor="fullName">
            Họ và tên <span className="text-red-500">*</span>
          </label>
          <input
            id="fullName"
            type="text"
            className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
            placeholder="Nhập họ và tên giáo viên"
            {...register("fullName", { required: "Vui lòng nhập họ và tên." })}
          />
          {errors.fullName && (
            <p className="mt-1 text-xs text-red-600">{errors.fullName.message}</p>
          )}
        </div>

        {/* Subject */}
        <div>
          <label className="block text-sm font-semibold mb-1" htmlFor="subject">
            Môn dạy phụ trách
          </label>
          <input
            id="subject"
            type="text"
            className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
            placeholder="Ví dụ: Toán học, Vật lý, Tiếng Anh..."
            {...register("subject")}
          />
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Phone Number */}
          <div>
            <label className="block text-sm font-semibold mb-1" htmlFor="phoneNumber">
              Số điện thoại
            </label>
            <input
              id="phoneNumber"
              type="text"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
              placeholder="Ví dụ: 0909123456"
              {...register("phoneNumber", {
                pattern: {
                  value: /^[0-9+.\-\s]{9,15}$/,
                  message: "Số điện thoại không hợp lệ.",
                },
              })}
            />
            {errors.phoneNumber && (
              <p className="mt-1 text-xs text-red-600">{errors.phoneNumber.message}</p>
            )}
          </div>

          {/* Email */}
          <div>
            <label className="block text-sm font-semibold mb-1" htmlFor="email">
              Email
            </label>
            <input
              id="email"
              type="email"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
              placeholder="teacher@example.com"
              {...register("email", {
                pattern: {
                  value: /^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/,
                  message: "Email không hợp lệ.",
                },
              })}
            />
            {errors.email && (
              <p className="mt-1 text-xs text-red-600">{errors.email.message}</p>
            )}
          </div>
        </div>

        {/* Is Active */}
        <div className="flex items-center gap-2 pt-2">
          <input
            id="isActive"
            type="checkbox"
            className="h-4 w-4 rounded border-border text-primary focus:ring-primary"
            {...register("isActive")}
          />
          <label className="text-sm font-medium text-foreground cursor-pointer select-none" htmlFor="isActive">
            Kích hoạt tài khoản giảng dạy
          </label>
        </div>

        <div className="flex items-center justify-end gap-3 pt-4 border-t border-border mt-6">
          <button
            type="button"
            onClick={onClose}
            className="h-10 rounded-md border border-border bg-surface px-4 text-sm font-semibold hover:bg-slate-50 transition cursor-pointer"
          >
            Hủy bỏ
          </button>
          <button
            type="submit"
            disabled={isSubmitting}
            className="h-10 rounded-md bg-primary text-primary-foreground px-4 text-sm font-semibold hover:bg-primary/90 transition shadow disabled:opacity-50 cursor-pointer"
          >
            {isSubmitting ? "Đang lưu..." : "Lưu thông tin"}
          </button>
        </div>
      </form>
    </div>
  );
}
