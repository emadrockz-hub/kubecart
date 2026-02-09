export function decodeJwtPayload(token) {
  try {
    if (!token) return null;
    const payload = token.split(".")[1];
    if (!payload) return null;

    // base64url -> base64
    const b64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const json = decodeURIComponent(
      atob(b64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(json);
  } catch {
    return null;
  }
}

export function getRoleFromToken(token) {
  const p = decodeJwtPayload(token);
  if (!p) return null;

  // common keys
  const roleKey = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
  return p[roleKey] || p.role || p.Role || null;
}
