import { theStore } from './configureStore.ts';
import { refreshToken, logout } from "./authSlice";

let isRefreshing = false;

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

  // ⚠️ 401 — пробуем refresh ОДИН раз
  if (isRefreshing) {
    return response;
  }

  isRefreshing = true;

  try {
    await theStore.dispatch(refreshToken()).unwrap();

    const newToken = theStore.getState().auth.token;
    if (!newToken) {
      theStore.dispatch(logout());
      return response;
    }

    const retryHeaders = new Headers(init?.headers || {});
    retryHeaders.set("Authorization", `Bearer ${newToken}`);

    response = await fetch(input, {
      ...init,
      headers: retryHeaders,
    });

    return response;
  } catch {
    theStore.dispatch(logout());
    return response;
  } finally {
    isRefreshing = false;
  }
}
