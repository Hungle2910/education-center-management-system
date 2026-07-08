import { ReportsView } from "@/features/reports/reports-view";

export const metadata = {
  title: "Báo cáo & Thống kê | Education Center CRM",
};

export default function ReportsPage() {
  return (
    <div className="p-6 max-w-6xl mx-auto bg-background min-h-screen">
      <ReportsView />
    </div>
  );
}
