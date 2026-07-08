"use client";

import { ReactNode } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { AuthGuard } from "@/features/auth/auth-guard";
import { useAuth } from "@/features/auth/auth-context";

interface DashboardLayoutProps {
  children: ReactNode;
}

export function DashboardLayout({ children }: DashboardLayoutProps) {
  const pathname = usePathname();
  const router = useRouter();
  const { logout, user } = useAuth();

  const menuItems = [
    { name: "Tổng quan", path: "/dashboard" },
    { name: "Tuyển sinh", path: "/dashboard/admissions" },
    { name: "Học sinh", path: "/dashboard/students" },
    { name: "Phụ huynh", path: "/dashboard/parents" },
    { name: "Giáo viên", path: "/dashboard/teachers" },
    { name: "Lớp học", path: "/dashboard/classes" },
    { name: "Lịch học", path: "/dashboard/schedules" },
    { name: "Học phí", path: "/dashboard/tuition" },
    { name: "Báo cáo", path: "/dashboard/reports" },
  ];

  function handleLogout() {
    logout();
    router.replace("/login");
  }

  return (
    <AuthGuard>
      <div className="flex min-h-screen bg-background text-foreground">
        {/* Sidebar */}
        <aside className="w-64 border-r border-border bg-surface flex flex-col justify-between">
          <div>
            <div className="px-6 py-5 border-b border-border">
              <span className="text-lg font-bold text-primary tracking-tight">
                EDU-CRM
              </span>
              <p className="text-xs text-muted-foreground font-medium">Trung tâm giáo dục</p>
            </div>
            <nav className="mt-6 px-4 space-y-1">
              {menuItems.map((item) => {
                const isActive = pathname === item.path;
                return (
                  <Link
                    key={item.path}
                    href={item.path}
                    className={`flex items-center px-4 py-2.5 text-sm font-medium rounded-md transition-colors ${
                      isActive
                        ? "bg-primary/10 text-primary"
                        : "text-muted-foreground hover:bg-background hover:text-foreground"
                    }`}
                  >
                    {item.name}
                  </Link>
                );
              })}
            </nav>
          </div>

          {/* User info at bottom */}
          <div className="p-4 border-t border-border bg-background/50">
            <div className="flex items-center justify-between gap-2">
              <div className="overflow-hidden">
                <p className="text-sm font-semibold truncate">{user?.fullName}</p>
                <p className="text-xs text-muted-foreground truncate">{user?.email}</p>
              </div>
              <button
                onClick={handleLogout}
                className="text-xs text-red-600 hover:text-red-700 font-semibold cursor-pointer shrink-0"
              >
                Đăng xuất
              </button>
            </div>
          </div>
        </aside>

        {/* Content area */}
        <div className="flex-1 flex flex-col">
          <main className="flex-1 p-6 md:p-8 max-w-7xl w-full mx-auto">
            {children}
          </main>
        </div>
      </div>
    </AuthGuard>
  );
}
