import { Suspense } from "react";
import { LoginView } from "@/features/auth/login-view";

export default function LoginPage() {
  return (
    <Suspense>
      <LoginView />
    </Suspense>
  );
}
