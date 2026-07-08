"use client";

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import {
  createContext,
  ReactNode,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { getAccessToken } from "@/lib/http/auth-token";
import { useAuth } from "@/features/auth/auth-context";
import type { NotificationDto, NotificationLevel } from "./notification-types";

type ConnectionStatus = "disconnected" | "connecting" | "connected" | "reconnecting";

type ToastNotification = NotificationDto & {
  id: string;
};

type NotificationContextValue = {
  connectionStatus: ConnectionStatus;
  unreadWarningCount: number;
  latestNotification: NotificationDto | null;
  clearWarningBadge: () => void;
};

const NotificationContext = createContext<NotificationContextValue | null>(null);

const receiveNotificationEvent = "ReceiveNotification";
const urgentLevel: NotificationLevel = "Khẩn cấp";
const warningLevel: NotificationLevel = "Cảnh báo";
const autoDismissLevels = new Set<NotificationLevel>(["Thông tin", "Thành công", "Cảnh báo"]);

type NotificationProviderProps = {
  children: ReactNode;
};

export function NotificationProvider({ children }: NotificationProviderProps) {
  const { isAuthenticated } = useAuth();
  const [connectionStatus, setConnectionStatus] = useState<ConnectionStatus>("disconnected");
  const [toasts, setToasts] = useState<ToastNotification[]>([]);
  const [urgentNotification, setUrgentNotification] = useState<NotificationDto | null>(null);
  const [latestNotification, setLatestNotification] = useState<NotificationDto | null>(null);
  const [unreadWarningCount, setUnreadWarningCount] = useState(0);
  const connectionRef = useRef<HubConnection | null>(null);

  const handleNotification = useCallback((notification: NotificationDto) => {
    setLatestNotification(notification);

    if (notification.level === warningLevel) {
      setUnreadWarningCount((count) => count + 1);
    }

    if (notification.level === urgentLevel) {
      setUrgentNotification(notification);
      return;
    }

    if (autoDismissLevels.has(notification.level)) {
      const id = `${Date.now()}-${Math.random().toString(36).slice(2)}`;
      setToasts((current) => [...current, { ...notification, id }]);

      window.setTimeout(() => {
        setToasts((current) => current.filter((toast) => toast.id !== id));
      }, 4500);
    }
  }, []);

  useEffect(() => {
    if (!isAuthenticated) {
      void stopConnection(connectionRef.current);
      connectionRef.current = null;
      window.queueMicrotask(() => setConnectionStatus("disconnected"));
      return;
    }

    const connection = createNotificationConnection();
    connectionRef.current = connection;
    connection.on(receiveNotificationEvent, handleNotification);
    connection.onreconnecting(() => setConnectionStatus("reconnecting"));
    connection.onreconnected(() => setConnectionStatus("connected"));
    connection.onclose(() => setConnectionStatus("disconnected"));

    let isMounted = true;

    async function startConnection() {
      try {
        setConnectionStatus("connecting");
        await connection.start();

        if (isMounted) {
          setConnectionStatus("connected");
        }
      } catch {
        if (isMounted) {
          setConnectionStatus("disconnected");
        }
      }
    }

    void startConnection();

    return () => {
      isMounted = false;
      connection.off(receiveNotificationEvent, handleNotification);
      void stopConnection(connection);
      connectionRef.current = null;
    };
  }, [handleNotification, isAuthenticated]);

  const clearWarningBadge = useCallback(() => setUnreadWarningCount(0), []);

  const value = useMemo<NotificationContextValue>(
    () => ({
      connectionStatus,
      unreadWarningCount,
      latestNotification,
      clearWarningBadge,
    }),
    [clearWarningBadge, connectionStatus, latestNotification, unreadWarningCount],
  );

  return (
    <NotificationContext.Provider value={value}>
      {children}
      <NotificationToasts toasts={toasts} />
      {urgentNotification ? (
        <UrgentNotificationModal
          notification={urgentNotification}
          onClose={() => setUrgentNotification(null)}
        />
      ) : null}
    </NotificationContext.Provider>
  );
}

export function useNotification(): NotificationContextValue {
  const value = useContext(NotificationContext);

  if (!value) {
    throw new Error("useNotification must be used inside NotificationProvider.");
  }

  return value;
}

export function createNotificationConnection(): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(getNotificationHubUrl(), {
      accessTokenFactory: () => getAccessToken() ?? "",
    })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}

function getNotificationHubUrl(): string {
  const apiBaseUrl = process.env.NEXT_PUBLIC_API_BASE_URL ?? "http://localhost:5088/api";
  return `${apiBaseUrl.replace(/\/api\/?$/, "")}/hubs/notifications`;
}

async function stopConnection(connection: HubConnection | null): Promise<void> {
  if (!connection || connection.state === HubConnectionState.Disconnected) {
    return;
  }

  await connection.stop();
}

function NotificationToasts({ toasts }: { toasts: ToastNotification[] }) {
  if (toasts.length === 0) {
    return null;
  }

  return (
    <div className="fixed right-4 top-4 z-50 flex w-[min(24rem,calc(100vw-2rem))] flex-col gap-3">
      {toasts.map((toast) => (
        <div
          key={toast.id}
          role="status"
          className="rounded-md border border-border bg-surface p-4 text-sm shadow-lg"
        >
          <div className="flex items-start justify-between gap-3">
            <div>
              <p className="font-semibold text-foreground">{toast.title}</p>
              <p className="mt-1 text-muted-foreground">{toast.message}</p>
            </div>
            <span className="shrink-0 rounded-full bg-background px-2 py-1 text-xs text-muted-foreground">
              {toast.level}
            </span>
          </div>
        </div>
      ))}
    </div>
  );
}

function UrgentNotificationModal({
  notification,
  onClose,
}: {
  notification: NotificationDto;
  onClose: () => void;
}) {
  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-black/45 p-4">
      <div
        role="alertdialog"
        aria-modal="true"
        aria-labelledby="urgent-notification-title"
        className="w-full max-w-md rounded-lg bg-surface p-5 shadow-xl"
      >
        <p className="text-xs font-semibold uppercase text-red-600">{notification.level}</p>
        <h2 id="urgent-notification-title" className="mt-2 text-lg font-semibold text-foreground">
          {notification.title}
        </h2>
        <p className="mt-3 text-sm leading-6 text-muted-foreground">{notification.message}</p>
        <button
          type="button"
          onClick={onClose}
          className="mt-5 w-full rounded-md bg-primary px-4 py-2 text-sm font-semibold text-primary-foreground"
        >
          Đã hiểu
        </button>
      </div>
    </div>
  );
}
