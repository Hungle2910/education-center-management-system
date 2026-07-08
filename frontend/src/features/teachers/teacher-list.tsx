"use client";

import { useState } from "react";
import { useTeachers } from "@/features/classes/class-api";
import { TeacherResponse } from "@/features/classes/class-types";
import { TeacherForm } from "./teacher-form";

export function TeacherList() {
  const { data: teachers, isLoading, error } = useTeachers();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedTeacher, setSelectedTeacher] = useState<TeacherResponse | null | undefined>(undefined);

  const filteredTeachers = teachers?.filter((teacher) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      teacher.fullName.toLowerCase().includes(searchLower) ||
      (teacher.email?.toLowerCase() ?? "").includes(searchLower) ||
      (teacher.phoneNumber ?? "").includes(searchLower) ||
      (teacher.subject?.toLowerCase() ?? "").includes(searchLower)
    );
  });

  if (isLoading) {
    return <div className="text-center py-10 text-muted-foreground">Đang tải danh sách giáo viên...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-600">Đã xảy ra lỗi khi tải dữ liệu.</div>;
  }

  return (
    <div className="space-y-6">
      {selectedTeacher !== undefined ? (
        <TeacherForm
          teacher={selectedTeacher ?? undefined}
          onClose={() => setSelectedTeacher(undefined)}
        />
      ) : (
        <>
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Giáo viên</h1>
              <p className="text-sm text-muted-foreground mt-1">
                Quản lý hồ sơ giảng dạy, môn học phụ trách và thông tin liên lạc của giáo viên.
              </p>
            </div>
            <button
              onClick={() => setSelectedTeacher(null)}
              className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow transition-colors hover:bg-primary/90 cursor-pointer"
            >
              Thêm giáo viên
            </button>
          </div>

          {/* Search */}
          <div className="flex items-center gap-2 max-w-md">
            <input
              type="text"
              placeholder="Tìm theo tên, môn học, số điện thoại..."
              value={searchTerm}
              onChange={(e) => setSearchTerm(e.target.value)}
              className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
            />
          </div>

          {/* Table */}
          <div className="overflow-x-auto rounded-lg border border-border bg-surface shadow-sm">
            <table className="min-w-full divide-y divide-border text-left text-sm text-foreground">
              <thead className="bg-background text-muted-foreground font-medium text-xs uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-4">Họ và tên</th>
                  <th className="px-6 py-4">Môn dạy</th>
                  <th className="px-6 py-4">Số điện thoại</th>
                  <th className="px-6 py-4">Email</th>
                  <th className="px-6 py-4">Trạng thái</th>
                  <th className="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {filteredTeachers && filteredTeachers.length > 0 ? (
                  filteredTeachers.map((teacher) => (
                    <tr key={teacher.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-6 py-4 font-semibold">{teacher.fullName}</td>
                      <td className="px-6 py-4">
                        <span className="inline-flex items-center rounded bg-indigo-50 px-2.5 py-0.5 text-xs font-medium text-indigo-700 border border-indigo-200">
                          {teacher.subject ?? "Chưa gán"}
                        </span>
                      </td>
                      <td className="px-6 py-4">{teacher.phoneNumber ?? "--"}</td>
                      <td className="px-6 py-4 text-muted-foreground">{teacher.email ?? "--"}</td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-semibold ${
                          teacher.isActive
                            ? "bg-green-50 text-green-700 ring-1 ring-green-600/20"
                            : "bg-red-50 text-red-700 ring-1 ring-red-600/20"
                        }`}>
                          {teacher.isActive ? "Hoạt động" : "Ngừng hoạt động"}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <button
                          onClick={() => setSelectedTeacher(teacher)}
                          className="text-primary hover:underline text-sm font-semibold cursor-pointer"
                        >
                          Chỉnh sửa
                        </button>
                      </td>
                    </tr>
                  ))
                ) : (
                  <tr>
                    <td colSpan={6} className="px-6 py-10 text-center text-muted-foreground">
                      Không tìm thấy giáo viên nào.
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          </div>
        </>
      )}
    </div>
  );
}
