// ===== Tuition Types =====
export type TuitionStatus =
  | "Chưa thanh toán"
  | "Chờ xác nhận"
  | "Đã thanh toán"
  | "Thanh toán thiếu"
  | "Thanh toán dư"
  | "Quá hạn"
  | "Đã hủy"
  | "Đã hoàn tiền";

export interface TuitionInvoice {
  id: string;
  studentId: string;
  studentName: string;
  classId: string;
  className: string;
  month: string; // "YYYY-MM"
  baseAmount: number;
  discountAmount: number;
  deductAmount: number;
  adjustAmount: number;
  totalAmount: number;
  adjustReason?: string;
  status: TuitionStatus;
  paymentProofUrl?: string;
  vietQrUrl?: string;
  vietQrOutdated: boolean;
  vietQrGeneratedAt?: string;
  paidAmount?: number;
  paymentNote?: string;
  paymentContent?: string;
  operationHistory?: string;
  createdAtUtc: string;
}

export interface TuitionPreview {
  studentId: string;
  studentName: string;
  baseAmount: number;
  totalSessions: number;
  cancelledSessions: number;
  deductAmount: number;
  estimatedTotal: number;
}

export interface GenerateTuitionRequest {
  classId: string;
  month: string; // "YYYY-MM"
}

export interface AdjustTuitionRequest {
  adjustAmount: number;
  reason: string;
}

export interface ApplyDiscountRequest {
  discountCode: string;
}

export interface GenerateVietQrRequest {
  overrideContent?: string; // null = auto-generate from spec
}

export interface SubmitPaymentProofRequest {
  paymentProofUrl: string;
}

export interface ConfirmPaymentRequest {
  paidAmount: number;
  note?: string;
}

export interface PaymentSetting {
  id: string;
  bankName: string;
  bankId: string;
  accountNo: string;
  accountName: string;
  vietQrTemplate: string;
  isDefault: boolean;
  isActive: boolean;
  createdAtUtc: string;
  updatedAtUtc?: string;
}

export interface PaymentSettingRequest {
  bankName: string;
  bankId: string;
  accountNo: string;
  accountName: string;
  vietQrTemplate: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdatePaymentContentRequest {
  paymentContent: string;
}

