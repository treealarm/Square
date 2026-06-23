import { theStore } from '../store/configureStore';

// vms_rec's API/backend origin, e.g. "http://localhost:5134" — set at build time (Square and
// vms_rec are separate origins, unlike Square's own same-origin DoFetch). Empty by default: the
// Monitor tab then simply has nothing to show, no crash (see MonitorViewer.tsx).
export const VMS_REC_BASE_URL: string = import.meta.env.VITE_VMS_REC_BASE_URL ?? '';

// Square and vms_rec both validate JWTs issued by the same Keycloak realm (see
// docs/square-integration-plan.md in vms_rec) — Square's own token is reused as-is, no separate
// login against vms_rec is needed.
export function vmsRecFetch(path: string, init?: RequestInit): Promise<Response> {
  const token = theStore.getState().authStates?.token;
  const headers = new Headers(init?.headers || {});
  if (token) {
    headers.set('Authorization', `Bearer ${token}`);
  }
  return fetch(`${VMS_REC_BASE_URL}${path}`, { ...init, headers });
}

// hls.js's own XHR loader bypasses vmsRecFetch entirely — pass this as `xhrSetup` to attach the
// same bearer token to manifest/segment requests.
export function vmsRecXhrSetup(xhr: XMLHttpRequest): void {
  const token = theStore.getState().authStates?.token;
  if (token) {
    xhr.setRequestHeader('Authorization', `Bearer ${token}`);
  }
}
