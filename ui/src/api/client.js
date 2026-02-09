import { getToken, clearToken } from "../auth/token.js";

async function readErrorMessage(res) {
  const contentType = res.headers.get("content-type") || "";
  try {
    if (contentType.includes("application/json")) {
      const data = await res.json();
      return data?.message || data?.error || JSON.stringify(data);
    }
    const text = await res.text();
    return text || `HTTP ${res.status}`;
  } catch {
    return `HTTP ${res.status}`;
  }
}

export async function apiFetch(path, options = {}) {
  // Path must be relative like "/api/catalog/products"
  const headers = new Headers(options.headers || {});
  headers.set("Accept", "application/json");

  const token = getToken();
  if (token) headers.set("Authorization", `Bearer ${token}`);

  // If sending JSON body, ensure correct header
  if (options.body && !(options.body instanceof FormData)) {
    if (!headers.has("Content-Type")) headers.set("Content-Type", "application/json");
  }

  const res = await fetch(path, { ...options, headers });

  if (res.status === 401) {
    // Token invalid/expired → wipe and force login behavior
    clearToken();
    // Optional: allow UI to react (we'll handle redirect in pages later)
    throw new Error("Unauthorized (401). Please login again.");
  }

  if (!res.ok) {
    const msg = await readErrorMessage(res);
    throw new Error(msg);
  }

  // Handle empty responses
  if (res.status === 204) return null;

  const contentType = res.headers.get("content-type") || "";
  if (contentType.includes("application/json")) return res.json();
  return res.text();
}
