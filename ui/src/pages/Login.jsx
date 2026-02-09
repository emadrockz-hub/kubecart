import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { apiFetch } from "../api/client.js";
import { setToken } from "../auth/token.js";
import Toast from "../components/Toast.jsx";

export default function Login() {
  const nav = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [toast, setToast] = useState("");

  async function onSubmit(e) {
    e.preventDefault();
    setBusy(true);
    setToast("");

    try {
      const res = await apiFetch("/api/auth/login", {
        method: "POST",
        body: JSON.stringify({ email, password }),
      });

      // your API returns { userId, email, token }
      const token = res?.token || res?.Token || res;
      if (!token) throw new Error("Token missing from response.");

      setToken(token);
      window.dispatchEvent(new Event("kubecart-auth-changed"));

      setToast("Signed in ✅");
      setTimeout(() => nav("/"), 300);
    } catch (err) {
      setToast(err?.message || "Login failed");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div style={{ display: "grid", placeItems: "center", padding: "28px 0" }}>
      <div style={{ width: 360 }}>
        <div style={{ textAlign: "center", marginBottom: 14, fontWeight: 900, fontSize: 22 }}>
          kubecart
        </div>

        <div className="card" style={{ borderRadius: 10, boxShadow: "none" }}>
          <div style={{ padding: 18 }}>
            <div style={{ fontSize: 28, fontWeight: 700, marginBottom: 10 }}>Sign in</div>

            <form onSubmit={onSubmit} style={{ display: "grid", gap: 12 }}>
              <div style={{ display: "grid", gap: 6 }}>
                <label style={{ fontWeight: 700, fontSize: 13 }}>Email</label>
                <input
                  className="input"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  placeholder="you@kubecart.com"
                  autoComplete="username"
                />
              </div>

              <div style={{ display: "grid", gap: 6 }}>
                <label style={{ fontWeight: 700, fontSize: 13 }}>Password</label>
                <input
                  className="input"
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="Your password"
                  autoComplete="current-password"
                />
              </div>

              <button
                type="submit"
                disabled={busy}
                style={{
                  height: 38,
                  borderRadius: 8,
                  border: "1px solid #FCD200",
                  background: "#FFD814",
                  cursor: "pointer",
                  fontWeight: 800,
                  color: "#111",
                }}
              >
                {busy ? "Signing in..." : "Sign in"}
              </button>
            </form>

            {/* ✅ REGISTER LINK (MUST BE INSIDE RETURN) */}
            <div style={{ marginTop: 12, fontSize: 13 }}>
              New to KubeCart? <Link to="/register">Create your account</Link>
            </div>

            <div style={{ marginTop: 12, fontSize: 12, color: "#6b7280", lineHeight: 1.4 }}>
              By continuing, you agree this is a capstone demo UI.
            </div>
          </div>
        </div>
      </div>

      <Toast message={toast} onClose={() => setToast("")} />
    </div>
  );
}
