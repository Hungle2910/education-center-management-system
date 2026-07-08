import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { apiClient } from "@/lib/http/api-client";
import type {
  TuitionInvoice,
  TuitionPreview,
  GenerateTuitionRequest,
  AdjustTuitionRequest,
  ApplyDiscountRequest,
  GenerateVietQrRequest,
  SubmitPaymentProofRequest,
  ConfirmPaymentRequest,
  PaymentSetting,
  PaymentSettingRequest,
  UpdatePaymentContentRequest,
} from "./tuition-types";

const QUERY_KEY = "tuition-invoices";

// ─── Hooks ────────────────────────────────────────────────────────────────────

export function useInvoices(classId?: string, month?: string) {
  return useQuery<TuitionInvoice[]>({
    queryKey: [QUERY_KEY, classId, month],
    queryFn: async () => {
      const params = new URLSearchParams();
      if (classId) params.set("classId", classId);
      if (month) params.set("month", month);
      const res = await apiClient.get(`/api/tuition/invoices?${params.toString()}`);
      return res.data.data;
    },
  });
}

export function usePreviewTuition() {
  return useMutation<TuitionPreview[], Error, GenerateTuitionRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post("/api/tuition/preview", req);
      return res.data.data;
    },
  });
}

export function useGenerateTuition() {
  const qc = useQueryClient();
  return useMutation<TuitionInvoice[], Error, GenerateTuitionRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post("/api/tuition/generate", req);
      return res.data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
}

export function useAdjustInvoice(id: string) {
  const qc = useQueryClient();
  return useMutation<TuitionInvoice, Error, AdjustTuitionRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post(`/api/tuition/invoice/${id}/adjust`, req);
      return res.data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
}

export function useApplyDiscount(id: string) {
  const qc = useQueryClient();
  return useMutation<TuitionInvoice, Error, ApplyDiscountRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post(`/api/tuition/invoice/${id}/apply-discount`, req);
      return res.data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
}

export function useGenerateVietQr(id: string) {
  const qc = useQueryClient();
  return useMutation<TuitionInvoice, Error, GenerateVietQrRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post(`/api/tuition/invoice/${id}/generate-vietqr`, req);
      return res.data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
}

export function useSubmitPaymentProof(id: string) {
  const qc = useQueryClient();
  return useMutation<TuitionInvoice, Error, SubmitPaymentProofRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post(`/api/tuition/invoice/${id}/submit-payment-proof`, req);
      return res.data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
}

export function useConfirmPayment(id: string) {
  const qc = useQueryClient();
  return useMutation<TuitionInvoice, Error, ConfirmPaymentRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post(`/api/tuition/invoice/${id}/confirm-payment`, req);
      return res.data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
}



export function useUpdatePaymentContent(id: string) {
  const qc = useQueryClient();
  return useMutation<TuitionInvoice, Error, UpdatePaymentContentRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.put(`/api/tuition/invoice/${id}/payment-content`, req);
      return res.data.data;
    },
    onSuccess: () => qc.invalidateQueries({ queryKey: [QUERY_KEY] }),
  });
}

// ─── Payment Settings API Hooks ──────────────────────────────────────────────

const SETTINGS_QUERY_KEY = "payment-settings";

export function usePaymentSettings() {
  return useQuery<PaymentSetting[]>({
    queryKey: [SETTINGS_QUERY_KEY],
    queryFn: async () => {
      const res = await apiClient.get("/api/payment-settings");
      return res.data.data;
    },
  });
}

export function useCreatePaymentSetting() {
  const qc = useQueryClient();
  return useMutation<PaymentSetting, Error, PaymentSettingRequest>({
    mutationFn: async (req) => {
      const res = await apiClient.post("/api/payment-settings", req);
      return res.data.data;
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: [SETTINGS_QUERY_KEY] });
      qc.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

export function useUpdatePaymentSetting() {
  const qc = useQueryClient();
  return useMutation<PaymentSetting, Error, { id: string; request: PaymentSettingRequest }>({
    mutationFn: async ({ id, request }) => {
      const res = await apiClient.put(`/api/payment-settings/${id}`, request);
      return res.data.data;
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: [SETTINGS_QUERY_KEY] });
      qc.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

export function useDeletePaymentSetting() {
  const qc = useQueryClient();
  return useMutation<boolean, Error, string>({
    mutationFn: async (id) => {
      const res = await apiClient.delete(`/api/payment-settings/${id}`);
      return res.data.data;
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: [SETTINGS_QUERY_KEY] });
      qc.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

export function useSetDefaultPaymentSetting() {
  const qc = useQueryClient();
  return useMutation<boolean, Error, string>({
    mutationFn: async (id) => {
      const res = await apiClient.post(`/api/payment-settings/${id}/set-default`);
      return res.data.data;
    },
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: [SETTINGS_QUERY_KEY] });
      qc.invalidateQueries({ queryKey: [QUERY_KEY] });
    },
  });
}

