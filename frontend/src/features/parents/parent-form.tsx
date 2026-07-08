"use client";

import { useForm } from "react-hook-form";
import { ParentRequest, ParentResponse } from "@/features/students/student-types";
import { useCreateParent, useUpdateParent } from "@/features/students/student-api";
import { useState } from "react";

interface ParentFormProps {
  parent?: ParentResponse;
  onClose: () => void;
}

export function ParentForm({ parent, onClose }: ParentFormProps) {
  const isEdit = !!parent;
  const createParent = useCreateParent();
  const updateParent = useUpdateParent({ id: parent?.id ?? "" });
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ParentRequest>({
    defaultValues: {
      fullName: parent?.fullName ?? "",
      email: parent?.email ?? "",
      phoneNumber: parent?.phoneNumber ?? "",
      students: [],
    },
  });

  const onSubmit = async (data: ParentRequest) => {
    try {
      setErrorMessage(null);
      if (isEdit) {
        await updateParent.mutateAsync(data);
      } else {
        await createParent.mutateAsync(data);
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
          {isEdit ? "Chỉnh sửa thông tin phụ huynh" : "Thêm mới phụ huynh"}
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
            placeholder="Nhập họ và tên phụ huynh"
            {...register("fullName", { required: "Vui lòng nhập họ và tên." })}
          />
          {errors.fullName && (
            <p className="mt-1 text-xs text-red-600">{errors.fullName.message}</p>
          )}
        </div>

        {/* Phone Number */}
        <div>
          <label className="block text-sm font-semibold mb-1" htmlFor="phoneNumber">
            Số điện thoại <span className="text-red-500">*</span>
          </label>
          <input
            id="phoneNumber"
            type="text"
            className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
            placeholder="Ví dụ: 0909123456"
            {...register("phoneNumber", {
              required: "Vui lòng nhập số điện thoại.",
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
            placeholder="parent@example.com"
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
