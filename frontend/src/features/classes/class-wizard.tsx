"use client";

import { useState } from "react";
import { useForm } from "react-hook-form";
import { ClassRequest, ClassResponse } from "./class-types";
import { useCreateClass, useUpdateClass, useTeachers } from "./class-api";

interface ClassWizardProps {
  classData?: ClassResponse;
  onClose: () => void;
}

export function ClassWizard({ classData, onClose }: ClassWizardProps) {
  const isEdit = !!classData;
  const createClass = useCreateClass();
  const updateClass = useUpdateClass({ id: classData?.id ?? "" });
  const { data: teachers } = useTeachers();

  const [step, setStep] = useState(1);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    watch,
    setValue,
    formState: { errors, isSubmitting },
  } = useForm<ClassRequest>({
    defaultValues: {
      name: classData?.name ?? "",
      subject: classData?.subject ?? "",
      teacherId: classData?.teacherId ?? undefined,
      status: classData?.status ?? "Sắp khai giảng",
      minStudents: classData?.minStudents ?? 5,
      maxStudents: classData?.maxStudents ?? 20,
    },
  });

  const formValues = watch();

  const handleNext = () => {
    if (step === 1) {
      if (!formValues.name) {
        setErrorMessage("Vui lòng nhập tên lớp học.");
        return;
      }
      setErrorMessage(null);
      setStep(2);
    } else if (step === 2) {
      setStep(3);
    }
  };

  const handleBack = () => {
    setErrorMessage(null);
    setStep((prev) => Math.max(1, prev - 1));
  };

  const onSubmit = async (data: ClassRequest) => {
    try {
      setErrorMessage(null);
      const payload = {
        ...data,
        teacherId: data.teacherId || null,
        minStudents: Number(data.minStudents),
        maxStudents: Number(data.maxStudents),
      };

      if (isEdit) {
        await updateClass.mutateAsync(payload);
      } else {
        await createClass.mutateAsync(payload);
      }
      onClose();
    } catch (err: any) {
      const apiError = err.response?.data?.message ?? "Đã xảy ra lỗi khi lưu lớp học.";
      setErrorMessage(apiError);
    }
  };

  const statusOptions = [
    "Sắp khai giảng",
    "Đang học",
    "Cần tuyển thêm",
    "Tạm dừng",
    "Đã kết thúc",
    "Đã hủy",
  ];

  return (
    <div className="max-w-2xl mx-auto rounded-lg border border-border bg-surface p-6 shadow-sm">
      <div className="flex items-center justify-between border-b border-border pb-4 mb-6">
        <h2 className="text-xl font-bold tracking-tight text-foreground">
          {isEdit ? "Chỉnh sửa lớp học" : "Tạo lớp học mới"}
        </h2>
        <button
          onClick={onClose}
          className="text-muted-foreground hover:text-foreground text-sm cursor-pointer"
        >
          Quay lại
        </button>
      </div>

      {/* Steps Indicator */}
      <div className="flex items-center justify-center gap-2 mb-6">
        <div className={`px-3 py-1 text-xs font-semibold rounded-full ${step === 1 ? "bg-primary text-primary-foreground" : "bg-slate-100 text-slate-600"}`}>
          1. Thông tin lớp
        </div>
        <div className="w-8 h-px bg-border"></div>
        <div className={`px-3 py-1 text-xs font-semibold rounded-full ${step === 2 ? "bg-primary text-primary-foreground" : "bg-slate-100 text-slate-600"}`}>
          2. Giáo viên
        </div>
        <div className="w-8 h-px bg-border"></div>
        <div className={`px-3 py-1 text-xs font-semibold rounded-full ${step === 3 ? "bg-primary text-primary-foreground" : "bg-slate-100 text-slate-600"}`}>
          3. Xác nhận
        </div>
      </div>

      {errorMessage && (
        <div className="mb-6 p-4 rounded-md bg-red-50 text-sm text-red-600 border border-red-200">
          {errorMessage}
        </div>
      )}

      <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
        {step === 1 && (
          <div className="space-y-4">
            {/* Class Name */}
            <div>
              <label className="block text-sm font-semibold mb-1" htmlFor="name">
                Tên lớp học <span className="text-red-500">*</span>
              </label>
              <input
                id="name"
                type="text"
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
                placeholder="Nhập tên lớp học"
                {...register("name", { required: true })}
              />
            </div>

            {/* Subject */}
            <div>
              <label className="block text-sm font-semibold mb-1" htmlFor="subject">
                Môn học
              </label>
              <input
                id="subject"
                type="text"
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
                placeholder="Ví dụ: Toán nâng cao lớp 9"
                {...register("subject")}
              />
            </div>

            <div className="grid grid-cols-2 gap-4">
              {/* Min Students */}
              <div>
                <label className="block text-sm font-semibold mb-1" htmlFor="minStudents">
                  Học sinh tối thiểu
                </label>
                <input
                  id="minStudents"
                  type="number"
                  className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  {...register("minStudents", { min: 1 })}
                />
              </div>

              {/* Max Students */}
              <div>
                <label className="block text-sm font-semibold mb-1" htmlFor="maxStudents">
                  Học sinh tối đa
                </label>
                <input
                  id="maxStudents"
                  type="number"
                  className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                  {...register("maxStudents", { min: 1 })}
                />
              </div>
            </div>

            {/* Status */}
            <div>
              <label className="block text-sm font-semibold mb-1" htmlFor="status">
                Trạng thái lớp <span className="text-red-500">*</span>
              </label>
              <select
                id="status"
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
                {...register("status")}
              >
                {statusOptions.map((opt) => (
                  <option key={opt} value={opt}>
                    {opt}
                  </option>
                ))}
              </select>
            </div>
          </div>
        )}

        {step === 2 && (
          <div className="space-y-4">
            <h3 className="text-sm font-semibold text-foreground mb-3">Chọn Giáo viên giảng dạy</h3>
            <div className="space-y-2 max-h-60 overflow-y-auto border border-border rounded-md p-2">
              <label className="flex items-center gap-3 p-2 rounded hover:bg-slate-50 cursor-pointer">
                <input
                  type="radio"
                  name="teacherId"
                  value=""
                  checked={!formValues.teacherId}
                  onChange={() => setValue("teacherId", undefined)}
                  className="h-4 w-4 text-primary focus:ring-primary"
                />
                <span className="text-sm">Chưa gán giáo viên</span>
              </label>

              {teachers?.map((t) => (
                <label key={t.id} className="flex items-center gap-3 p-2 rounded hover:bg-slate-50 cursor-pointer">
                  <input
                    type="radio"
                    name="teacherId"
                    value={t.id}
                    checked={formValues.teacherId === t.id}
                    onChange={() => setValue("teacherId", t.id)}
                    className="h-4 w-4 text-primary focus:ring-primary"
                  />
                  <div>
                    <span className="text-sm font-semibold block">{t.fullName}</span>
                    <span className="text-xs text-muted-foreground">Môn chuyên môn: {t.subject ?? "--"}</span>
                  </div>
                </label>
              ))}
            </div>
          </div>
        )}

        {step === 3 && (
          <div className="space-y-4 bg-slate-50 p-4 rounded-lg border border-border">
            <h3 className="font-semibold text-sm border-b border-border pb-2 text-foreground">Xác nhận thông tin lớp học</h3>
            <div className="grid grid-cols-2 gap-y-3 text-sm">
              <span className="text-muted-foreground">Tên lớp học:</span>
              <span className="font-semibold text-right">{formValues.name}</span>

              <span className="text-muted-foreground">Môn học:</span>
              <span className="font-semibold text-right">{formValues.subject || "--"}</span>

              <span className="text-muted-foreground">Giới hạn học sinh:</span>
              <span className="font-semibold text-right">{formValues.minStudents} - {formValues.maxStudents} HS</span>

              <span className="text-muted-foreground">Giáo viên:</span>
              <span className="font-semibold text-right text-primary">
                {teachers?.find(t => t.id === formValues.teacherId)?.fullName ?? "Chưa gán"}
              </span>

              <span className="text-muted-foreground">Trạng thái lớp:</span>
              <span className="font-semibold text-right">{formValues.status}</span>
            </div>
          </div>
        )}

        <div className="flex items-center justify-between pt-4 border-t border-border mt-6">
          {step > 1 ? (
            <button
              type="button"
              onClick={handleBack}
              className="h-10 rounded-md border border-border bg-surface px-4 text-sm font-semibold hover:bg-slate-50 transition cursor-pointer"
            >
              Quay lại
            </button>
          ) : (
            <div />
          )}

          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={onClose}
              className="h-10 rounded-md border border-border bg-surface px-4 text-sm font-semibold hover:bg-slate-50 transition cursor-pointer"
            >
              Hủy bỏ
            </button>
            {step < 3 ? (
              <button
                type="button"
                onClick={handleNext}
                className="h-10 rounded-md bg-primary text-primary-foreground px-4 text-sm font-semibold hover:bg-primary/90 transition shadow cursor-pointer"
              >
                Tiếp tục
              </button>
            ) : (
              <button
                type="submit"
                disabled={isSubmitting}
                className="h-10 rounded-md bg-primary text-primary-foreground px-4 text-sm font-semibold hover:bg-primary/90 transition shadow disabled:opacity-50 cursor-pointer"
              >
                {isSubmitting ? "Đang tạo..." : "Xác nhận & Hoàn tất"}
              </button>
            )}
          </div>
        </div>
      </form>
    </div>
  );
}
