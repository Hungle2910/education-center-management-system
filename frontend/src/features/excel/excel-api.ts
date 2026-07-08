import { useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];
}

export const downloadFile = async (url: string, filename: string) => {
  try {
    const response = await apiClient.get(url, { responseType: "blob" });
    const blob = new Blob([response.data], {
      type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
    });
    const downloadUrl = window.URL.createObjectURL(blob);
    const link = document.createElement("a");
    link.href = downloadUrl;
    link.setAttribute("download", filename);
    document.body.appendChild(link);
    link.click();
    link.remove();
    window.URL.revokeObjectURL(downloadUrl);
  } catch (error) {
    console.error("Lỗi khi tải file:", error);
    alert("Không thể tải file Excel. Vui lòng thử lại sau.");
  }
};

export const useImportStudents = () => {
  const queryClient = useQueryClient();
  return useMutation<
    ApiResponse<{ importedCount: number; errors?: string[] }>,
    Error,
    File
  >({
    mutationFn: async (file) => {
      const formData = new FormData();
      formData.append("file", file);
      const response = await apiClient.post<
        ApiResponse<{ importedCount: number; errors?: string[] }>
      >("/excel/import/students", formData, {
        headers: {
          "Content-Type": "multipart/form-data",
        },
      });
      return response.data;
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["students"] });
      queryClient.invalidateQueries({ queryKey: ["parents"] });
    },
  });
};

export const exportTuitionReport = async () => {
  await downloadFile("/excel/export/tuition-report", `Bao_Cao_Doanh_Thu_${new Date().toISOString().split("T")[0]}.xlsx`);
};

export const exportClassReport = async () => {
  await downloadFile("/excel/export/class-report", `Bao_Cao_Lop_Hoc_${new Date().toISOString().split("T")[0]}.xlsx`);
};

export const exportTeacherReport = async () => {
  await downloadFile("/excel/export/teacher-report", `Bao_Cao_Luong_Giao_Vien_${new Date().toISOString().split("T")[0]}.xlsx`);
};
