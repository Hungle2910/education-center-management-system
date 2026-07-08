const accessTokenKey = "education_center_crm_access_token";
const accessTokenCookieName = "access_token";
const maxAgeSeconds = 60 * 60;

export function getAccessToken(): string | null {
  if (typeof window === "undefined") {
    return null;
  }

  const localStorageToken = window.localStorage.getItem(accessTokenKey);
  if (localStorageToken) {
    return localStorageToken;
  }

  const cookieToken = document.cookie
    .split("; ")
    .find((cookie) => cookie.startsWith(`${accessTokenCookieName}=`))
    ?.split("=")[1];

  return cookieToken ? decodeURIComponent(cookieToken) : null;
}

export function setAccessToken(token: string): void {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.setItem(accessTokenKey, token);
  document.cookie = `${accessTokenCookieName}=${encodeURIComponent(
    token,
  )}; path=/; max-age=${maxAgeSeconds}; samesite=lax`;
}

export function clearAccessToken(): void {
  if (typeof window === "undefined") {
    return;
  }

  window.localStorage.removeItem(accessTokenKey);
  document.cookie = `${accessTokenCookieName}=; path=/; max-age=0; samesite=lax`;
}
