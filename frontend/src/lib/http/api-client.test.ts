import type { AxiosAdapter } from "axios";
import { afterEach, describe, expect, it } from "vitest";
import { apiClient } from "./api-client";
import { clearAccessToken, setAccessToken } from "./auth-token";

describe("apiClient", () => {
  afterEach(() => {
    clearAccessToken();
  });

  it("gắn header Authorization khi có token trong bộ nhớ", async () => {
    setAccessToken("jwt-demo-token");

    const adapter: AxiosAdapter = async (config) => ({
      config,
      data: null,
      headers: {},
      status: 200,
      statusText: "OK",
    });

    const response = await apiClient.get("/health", { adapter });

    expect(response.config.headers.Authorization).toBe(
      "Bearer jwt-demo-token",
    );
  });
});
