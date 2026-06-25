import { theStore } from './configureStore.ts';
import { refreshToken, logout } from "./authSlice";

let refreshPromise: Promise<boolean> | null = null;

// Shared by DoFetch's 401-retry and AuthGuard's pre-emptive scheduled refresh, so concurrent
// callers await the same network request instead of each independently calling Keycloak's
// refresh_token grant. Keycloak rotates the refresh token on use, so two concurrent refreshes
// with the same (now-stale) refresh_token end up with the second one rejected (400).
export function refreshTokenSafely(): Promise<boolean> {
  if (!refreshPromise) {
    refreshPromise = theStore.dispatch(refreshToken()).unwrap()
      .then(() => true)
      .catch(() => {
        theStore.dispatch(logout());
        return false;
      })
      .finally(() => {
        refreshPromise = null;
      });
  }
  return refreshPromise;
}

export async function DoFetch(
  input: RequestInfo,
  init?: RequestInit
): Promise<Response> {

  const state = theStore.getState();
  const token = state.authStates.token;

  const headers = new Headers(init?.headers || {});
  if (token) {
    headers.set("Authorization", `Bearer ${token}`);
  }

  let response = await fetch(input, {
    ...init,
    headers,
  });

  if (response.status !== 401) {
    return response;
  }

  // ⚠️ 401 — пробуем refresh
  const refreshed = await refreshTokenSafely();
  if (!refreshed) {
    return response;
  }

  const newToken = theStore.getState().authStates.token;

  const retryHeaders = new Headers(init?.headers || {});
  if (newToken) {
    retryHeaders.set("Authorization", `Bearer ${newToken}`);
  }

  response = await fetch(input, {
    ...init,
    headers: retryHeaders,
  });

  return response;
}
