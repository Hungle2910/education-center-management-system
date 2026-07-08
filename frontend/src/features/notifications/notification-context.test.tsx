import "@testing-library/jest-dom/vitest";
import { act, cleanup, render, screen } from "@testing-library/react";
import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { setAccessToken, clearAccessToken } from "@/lib/http/auth-token";
import { NotificationProvider } from "./notification-context";

type SignalRHandler = (...args: unknown[]) => void;

const signalRMock = vi.hoisted(() => {
  const handlers = new Map<string, SignalRHandler>();
  const connection = {
    start: vi.fn(() => Promise.resolve()),
    stop: vi.fn(() => Promise.resolve()),
    on: vi.fn((eventName: string, handler: SignalRHandler) => {
      handlers.set(eventName, handler);
    }),
    off: vi.fn((eventName: string) => {
      handlers.delete(eventName);
    }),
    onreconnecting: vi.fn(),
    onreconnected: vi.fn(),
    onclose: vi.fn(),
    state: "Disconnected",
  };
  const builder = {
    withUrl: vi.fn(() => builder),
    withAutomaticReconnect: vi.fn(() => builder),
    configureLogging: vi.fn(() => builder),
    build: vi.fn(() => connection),
  };

  class HubConnectionBuilder {
    withUrl = builder.withUrl;
    withAutomaticReconnect = builder.withAutomaticReconnect;
    configureLogging = builder.configureLogging;
    build = builder.build;
  }

  return {
    handlers,
    connection,
    builder,
    HubConnectionBuilder: vi.fn(HubConnectionBuilder),
  };
});

vi.mock("@microsoft/signalr", () => ({
  HubConnectionBuilder: signalRMock.HubConnectionBuilder,
  HubConnectionState: {
    Disconnected: "Disconnected",
  },
  LogLevel: {
    Warning: "Warning",
  },
}));

vi.mock("@/features/auth/auth-context", () => ({
  useAuth: () => ({
    isAuthenticated: true,
  }),
}));

describe("NotificationProvider", () => {
  afterEach(() => {
    cleanup();
  });

  beforeEach(() => {
    clearAccessToken();
    signalRMock.handlers.clear();
    signalRMock.connection.start.mockClear();
    signalRMock.connection.stop.mockClear();
    signalRMock.connection.on.mockClear();
    signalRMock.connection.off.mockClear();
    signalRMock.builder.withUrl.mockClear();
    signalRMock.builder.withAutomaticReconnect.mockClear();
    signalRMock.builder.configureLogging.mockClear();
    signalRMock.builder.build.mockClear();
    signalRMock.HubConnectionBuilder.mockClear();
  });

  it("khởi tạo SignalR với đúng URL notifications hub và gửi kèm JWT token", async () => {
    setAccessToken("jwt-demo-token");

    render(
      <NotificationProvider>
        <div>Ứng dụng</div>
      </NotificationProvider>,
    );

    await screen.findByText("Ứng dụng");

    expect(signalRMock.HubConnectionBuilder).toHaveBeenCalledTimes(1);
    expect(signalRMock.builder.withUrl).toHaveBeenCalledWith(
      "http://localhost:5088/hubs/notifications",
      expect.objectContaining({
        accessTokenFactory: expect.any(Function),
      }),
    );
    expect(signalRMock.builder.withAutomaticReconnect).toHaveBeenCalledTimes(1);

    const [, options] = signalRMock.builder.withUrl.mock.calls[0];
    expect(options.accessTokenFactory()).toBe("jwt-demo-token");
  });

  it("hiển thị toast khi nhận sự kiện ReceiveNotification", async () => {
    render(
      <NotificationProvider>
        <div>Ứng dụng</div>
      </NotificationProvider>,
    );

    await screen.findByText("Ứng dụng");

    await act(async () => {
      signalRMock.handlers.get("ReceiveNotification")?.({
        title: "Thanh toán mới",
        message: "Phụ huynh đã gửi biên lai.",
        level: "Thông tin",
        createdAt: new Date().toISOString(),
      });
    });

    expect(screen.getByText("Thanh toán mới")).toBeVisible();
    expect(screen.getByText("Phụ huynh đã gửi biên lai.")).toBeVisible();
  });
});
