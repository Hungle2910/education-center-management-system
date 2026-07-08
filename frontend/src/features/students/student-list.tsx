"use client";

import { useState } from "react";
import { useStudents } from "./student-api";
import { StudentResponse } from "./student-types";
import { StudentForm } from "./student-form";
import { ExcelActions } from "../excel/excel-actions";

export function StudentList() {
  const { data: students, isLoading, error } = useStudents();
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState("ALL");
  const [selectedStudent, setSelectedStudent] = useState<StudentResponse | null | undefined>(undefined);

  const filteredStudents = students?.filter((student) => {
    const searchLower = searchTerm.toLowerCase();
    const matchesSearch =
      student.fullName.toLowerCase().includes(searchLower) ||
      (student.email?.toLowerCase() ?? "").includes(searchLower) ||
      (student.phoneNumber ?? "").includes(searchLower) ||
      (student.studentCode?.toLowerCase() ?? "").includes(searchLower);

    const matchesStatus = statusFilter === "ALL" || student.status === statusFilter;

    return matchesSearch && matchesStatus;
  });

  const statusOptions = [
    { label: "Tất cả trạng thái", value: "ALL" },
    { label: "Học thử", value: "Học thử" },
    { label: "Đang học", value: "Đang học" },
    { label: "Tạm nghỉ", value: "Tạm nghỉ" },
    { label: "Bảo lưu", value: "Bảo lưu" },
    { label: "Chuyển lớp", value: "Chuyển lớp" },
    { label: "Đã nghỉ", value: "Đã nghỉ" },
    { label: "Hoàn thành", value: "Hoàn thành" },
  ];

  if (isLoading) {
    return <div className="text-center py-10 text-muted-foreground">Đang tải danh sách học sinh...</div>;
  }

  if (error) {
    return <div className="text-center py-10 text-red-600">Đã xảy ra lỗi khi tải dữ liệu học sinh.</div>;
  }

  return (
    <div className="space-y-6">
      {selectedStudent !== undefined ? (
        <StudentForm
          student={selectedStudent ?? undefined}
          onClose={() => setSelectedStudent(undefined)}
        />
      ) : (
        <>
          <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
            <div>
              <h1 className="text-2xl font-bold tracking-tight">Học sinh</h1>
              <p className="text-sm text-muted-foreground mt-1">
                Quản lý hồ sơ học sinh, trạng thái học tập và thông tin liên hệ.
              </p>
            </div>
            <button
              onClick={() => setSelectedStudent(null)}
              className="inline-flex items-center justify-center rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground shadow transition-colors hover:bg-primary/90 cursor-pointer"
            >
              Thêm học sinh
            </button>
          </div>

          <ExcelActions />

          {/* Filters */}
          <div className="flex flex-col sm:flex-row items-stretch sm:items-center gap-4">
            <div className="flex-1 max-w-md">
              <input
                type="text"
                placeholder="Tìm theo tên, mã HS, SĐT..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm placeholder:text-muted-foreground focus:outline-none focus:ring-1 focus:ring-primary"
              />
            </div>
            <div className="w-full sm:w-48">
              <select
                value={statusFilter}
                onChange={(e) => setStatusFilter(e.target.value)}
                className="flex h-10 w-full rounded-md border border-border bg-surface px-3 py-2 text-sm focus:outline-none focus:ring-1 focus:ring-primary"
              >
                {statusOptions.map((opt) => (
                  <option key={opt.value} value={opt.value}>
                    {opt.label}
                  </option>
                ))}
              </select>
            </div>
          </div>

          {/* Table */}
          <div className="overflow-x-auto rounded-lg border border-border bg-surface shadow-sm">
            <table className="min-w-full divide-y divide-border text-left text-sm text-foreground">
              <thead className="bg-background text-muted-foreground font-medium text-xs uppercase tracking-wider">
                <tr>
                  <th className="px-6 py-4">Mã HS</th>
                  <th className="px-6 py-4">Họ và tên</th>
                  <th className="px-6 py-4">Ngày sinh</th>
                  <th className="px-6 py-4">Trạng thái</th>
                  <th className="px-6 py-4">Phụ huynh liên kết</th>
                  <th className="px-6 py-4 text-right">Thao tác</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-border">
                {filteredStudents && filteredStudents.length > 0 ? (
                  filteredStudents.map((student) => (
                    <tr key={student.id} className="hover:bg-slate-50/50 transition-colors">
                      <td className="px-6 py-4 font-mono text-xs">{student.studentCode ?? "--"}</td>
                      <td className="px-6 py-4">
                        <div>
                          <p className="font-semibold">{student.fullName}</p>
                          <p className="text-xs text-muted-foreground">{student.phoneNumber ?? student.email ?? ""}</p>
                        </div>
                      </td>
                      <td className="px-6 py-4">{student.dateOfBirth ?? "--"}</td>
                      <td className="px-6 py-4">
                        <span className={`inline-flex items-center rounded-md px-2 py-1 text-xs font-semibold ${
                          student.status === "Đang học"
                            ? "bg-green-50 text-green-700 ring-1 ring-green-600/20"
                            : student.status === "Học thử"
                            ? "bg-blue-50 text-blue-700 ring-1 ring-blue-600/20"
                            : "bg-gray-50 text-gray-700 ring-1 ring-gray-600/20"
                        }`}>
                          {student.status}
                        </span>
                      </td>
                      <td className="px-6 py-4">
                        <div className="flex flex-col gap-1">
                          {student.parents && student.parents.length > 0 ? (
                            student.parents.map((p) => (
                              <span key={p.id} className="text-xs text-muted-foreground">
                                {p.fullName} ({p.relationship ?? "Chưa rõ"}) - {p.phoneNumber}
                              </span>
                            ))
                          ) : (
                            <span className="text-xs text-muted-foreground italic">Chưa liên kết</span>
                          )}
                        </div>
                      </td>
                      <td className="px-6 py-4 text-right">
                        <button
                          onClick={() => setSelectedStudent(student)}
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
                      Không tìm thấy học sinh nào.
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
