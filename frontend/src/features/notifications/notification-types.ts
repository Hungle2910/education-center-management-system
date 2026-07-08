export type NotificationLevel = "Thông tin" | "Thành công" | "Cảnh báo" | "Khẩn cấp";

export interface NotificationDto {
  title: string;
  message: string;
  level: NotificationLevel;
  createdAt: string;
}
