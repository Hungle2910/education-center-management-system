"use client";

import { useState } from "react";
import { useClasses } from "./class-api";
import { ClassResponse } from "./class-types";
import { ClassWizard } from "./class-wizard";

export function ClassList() {
  const { data: classes, isLoading, error } = useClasses();
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedClass, setSelectedClass] = useState<ClassResponse | null | undefined>(undefined);

  const filteredClasses = classes?.filter((cls) => {
    const searchLower = searchTerm.toLowerCase();
    return (
      cls.name.toLowerCase().includes(searchLower) ||
      (cls.subject?.toLowerCase() ?? "").includes(searchLower) ||
      (cls.teacherName?.toLowerCase() ?? "").includes(searchLower)
    );
  });

  if (isLoading) {
    return <div className="text-center py-10 text-muted-foreground">Đang tải danh sách lớp học...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-600">Đã xảy ra lỗi khi tải dữ liệu lớp học.</div>;
  }

  return (
    <div className="space-y-6">
      {selectedClass !== undefined ? (
        <ClassWizard
          classData={selectedClass ?? undefined}
          onClose={() => setSelectedClass(undefined)}
        />
      ) : (
        <>
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Lớp học</h1>
              <p className="text-sm text-muted-foreground mt-1">
                Quản lý các lớp học, phân công giáo viên và theo dõi sĩ số tuyển sinh.
              </p>
            </div>
            <button
              onClick={() => setSelectedClass(null)}
              className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow transition-colors hover:bg-primary/90 cursor-pointer"
            >
              Tạo lớp học
            </button>
          </div>

          {/* Search */}
          <div className="flex items-center gap-2 max-w-md">
            <input
              type="text"
              placeholder="Tìm theo tên lớp, môn học, giáo viên..."
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
                  <th className="px-6 py-4">Tên lớp học</th>
                  <th className="px-6 py-4">Môn học</th>
                  <th className="px-6 py-4">Giáo viên phụ trách</th>
                  <th className="px-6 py-4">Sĩ số giới hạn</th>
                  <th className="px-6 py-4">Trạng thái</th>
                  <th className="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {filteredClasses && filteredClasses.length > 0 ? (
                  filteredClasses.map((cls) => (
                    <tr key={cls.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-6 py-4 font-semibold">{cls.name}</td>
                      <td className="px-6 py-4">
                        <span className="inline-flex items-center rounded bg-teal-50 px-2 py-0.5 text-xs font-medium text-teal-700 border border-teal-200">
                          {cls.subject ?? "Chưa rõ"}
                        </span>
                      </td>
                      <td className="px-6 py-4">{cls.teacherName ?? "--"}</td>
                      <td className="px-6 py-4">
                        <span className="text-muted-foreground">{cls.minStudents} - {cls.maxStudents} Học sinh</span>
                      </td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-semibold ${
                          cls.status === "Đang học"
                            ? "bg-green-50 text-green-700 ring-1 ring-green-600/20"
                            : cls.status === "Sắp khai giảng"
                            ? "bg-blue-50 text-blue-700 ring-1 ring-blue-600/20"
                            : "bg-gray-50 text-gray-700 ring-1 ring-gray-600/20"
                        }`}>
                          {cls.status}
                        </span>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <button
                          onClick={() => setSelectedClass(cls)}
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
                      Không tìm thấy lớp học nào.
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
