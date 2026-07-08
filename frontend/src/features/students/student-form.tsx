"use client";

import { useForm, useFieldArray } from "react-hook-form";
import { StudentRequest, StudentResponse } from "./student-types";
import { useCreateStudent, useUpdateStudent, useParents } from "./student-api";
import { useState } from "react";

interface StudentFormProps {
  student?: StudentResponse;
  onClose: () => void;
}

export function StudentForm({ student, onClose }: StudentFormProps) {
  const isEdit = !!student;
  const createStudent = useCreateStudent();
  const updateStudent = useUpdateStudent({ id: student?.id ?? "" });
  const { data: parentsList } = useParents();
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    control,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<StudentRequest>({
    defaultValues: {
      fullName: student?.fullName ?? "",
      studentCode: student?.studentCode ?? "",
      email: student?.email ?? "",
      phoneNumber: student?.phoneNumber ?? "",
      dateOfBirth: student?.dateOfBirth ?? undefined,
      status: student?.status ?? "Học thử",
      parents: student?.parents?.map(p => ({
        parentId: p.id,
        relationship: p.relationship ?? "Bố/Mẹ"
      })) ?? [],
    },
  });

  const { fields, append, remove } = useFieldArray({
    control,
    name: "parents",
  });

  const onSubmit = async (data: StudentRequest) => {
    try {
      setErrorMessage(null);
      // Clean up parents list to match API request
      const formattedData = {
        ...data,
        parents: data.parents?.filter(p => !!p.parentId) || [],
      };

      if (isEdit) {
        await updateStudent.mutateAsync(formattedData);
      } else {
        await createStudent.mutateAsync(formattedData);
      }
      onClose();
    } catch (err: any) {
      const apiError = err.response?.data?.message ?? "Đã xảy ra lỗi khi lưu thông tin.";
      const detailErrors = err.response?.data?.errors?.join(", ");
      setErrorMessage(detailErrors ? `${apiError}: ${detailErrors}` : apiError);
    }
  };

  const statusOptions = [
    "Học thử",
    "Đang học",
    "Tạm nghỉ",
    "Bảo lưu",
    "Chuyển lớp",
    "Đã nghỉ",
    "Hoàn thành",
  ];

  return (
    <div className="max-w-2xl mx-auto rounded-lg border border-border bg-surface p-6 shadow-sm">
      <div className="flex items-center justify-between border-b border-border pb-4 mb-6">
        <h2 className="text-xl font-bold tracking-tight text-foreground">
          {isEdit ? "Chỉnh sửa hồ sơ học sinh" : "Thêm mới học sinh"}
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
            placeholder="Nhập họ và tên học sinh"
            {...register("fullName", { required: "Vui lòng nhập họ và tên." })}
          />
          {errors.fullName && (
            <p className="mt-1 text-xs text-red-600">{errors.fullName.message}</p>
          )}
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
          {/* Student Code */}
          <div>
            <label className="block text-sm font-semibold mb-1" htmlFor="studentCode">
              Mã học sinh
            </label>
            <input
              id="studentCode"
              type="text"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
              placeholder="Ví dụ: HS001"
              {...register("studentCode")}
            />
          </div>

          {/* Date of Birth */}
          <div>
            <label className="block text-sm font-semibold mb-1" htmlFor="dateOfBirth">
              Ngày sinh
            </label>
            <input
              id="dateOfBirth"
              type="date"
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
              {...register("dateOfBirth")}
            />
          </div>
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
              placeholder="student@example.com"
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

        {/* Status */}
        <div>
          <label className="block text-sm font-semibold mb-1" htmlFor="status">
            Trạng thái học tập <span className="text-red-500">*</span>
          </label>
          <select
            id="status"
            className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
            {...register("status", { required: true })}
          >
            {statusOptions.map((opt) => (
              <option key={opt} value={opt}>
                {opt}
              </option>
            ))}
          </select>
        </div>

        {/* Linked Parents */}
        <div className="border-t border-border pt-4 mt-6">
          <div className="flex items-center justify-between mb-4">
            <h3 className="text-sm font-bold text-foreground">Phụ huynh liên kết</h3>
            <button
              type="button"
              onClick={() => append({ parentId: "", relationship: "Bố/Mẹ" })}
              className="text-xs text-primary hover:underline font-semibold cursor-pointer"
            >
              + Thêm liên kết phụ huynh
            </button>
          </div>

          <div className="space-y-3">
            {fields.map((field, index) => (
              <div key={field.id} className="flex items-center gap-3">
                <div className="flex-1">
                  <select
                    className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                    {...register(`parents.${index}.parentId` as const, { required: "Vui lòng chọn phụ huynh." })}
                  >
                    <option value="">-- Chọn phụ huynh --</option>
                    {parentsList?.map((parent) => (
                      <option key={parent.id} value={parent.id}>
                        {parent.fullName} ({parent.phoneNumber})
                      </option>
                    ))}
                  </select>
                </div>
                <div className="w-32">
                  <input
                    type="text"
                    placeholder="Mối quan hệ"
                    className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
                    {...register(`parents.${index}.relationship` as const)}
                  />
                </div>
                <button
                  type="button"
                  onClick={() => remove(index)}
                  className="text-xs text-red-600 hover:text-red-700 font-semibold cursor-pointer"
                >
                  Xóa
                </button>
              </div>
            ))}
            {fields.length === 0 && (
              <p className="text-xs text-muted-foreground italic">Chưa liên kết phụ huynh nào.</p>
            )}
          </div>
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
