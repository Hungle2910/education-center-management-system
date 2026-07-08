import { TuitionList } from "@/features/tuition/tuition-list";

export default function TuitionPage() {
  return (
    <div className="p-6 max-w-6xl mx-auto">
      <div className="mb-6">
        <h1 className="text-2xl font-bold text-foreground">Quản lý Học phí</h1>
        <p className="text-sm text-muted-foreground mt-1">
          Tạo hoá đơn hàng tháng, theo dõi trạng thái thanh toán và quản lý giảm giá.
        </p>
      </div>
      <TuitionList />
    </div>
  );
}
