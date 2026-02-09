import React, { useState } from "react";
import { useNavigate, Link } from "react-router-dom";
import { apiFetch } from "../api/client.js";
import Toast from "../components/Toast.jsx";

export default function Register() {
  const nav = useNavigate();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [toast, setToast] = useState("");
  const [busy, setBusy] = useState(false);

  async function onSubmit(e) {
    e.preventDefault();
    setBusy(true);
    setToast("");

    try {
      const payload = { email: email.trim(), password };

      // Identity API per spec
      await apiFetch("/api/auth/register", {
        method: "POST",
        body: JSON.stringify(payload),
      });

      setToast("Account created ✅ Please sign in.");
      setTimeout(() => nav("/login"), 700);
    } catch (err) {
      setToast(err?.message || "Registration failed");
    } finally {
      setBusy(false);
    }
  }

  return (
    <div className="azAuthWrap">
      <Toast message={toast} onClose={() => setToast("")} />

      <div className="azAuthCard">
        <h1 className="azAuthTitle">Create account</h1>
        <p className="azAuthSub">Use your email to create a KubeCart account.</p>

        <form onSubmit={onSubmit} className="azAuthForm">
          <label className="azAuthLabel">Email</label>
          <input
            className="input"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="you@kubecart.com"
            autoComplete="email"
          />

          <label className="azAuthLabel">Password</label>
          <input
            className="input"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Create a password"
            type="password"
            autoComplete="new-password"
          />

          <button className="btn-primary azAuthBtn" type="submit" disabled={busy || !email.trim() || !password}>
            {busy ? "Creating..." : "Create your KubeCart account"}
          </button>
        </form>

        <div className="azAuthFooter">
          Already have an account? <Link to="/login">Sign in</Link>
        </div>
      </div>
    </div>
  );
}
