import React, { useEffect } from "react";

export default function Toast({ message, onClose }) {
  useEffect(() => {
    if (!message) return;
    const t = setTimeout(() => onClose?.(), 2200);
    return () => clearTimeout(t);
  }, [message, onClose]);

  if (!message) return null;

  return (
    <div
      style={{
        position: "fixed",
        bottom: 18,
        right: 18,
        zIndex: 50,
        background: "#111827",
        color: "white",
        padding: "12px 14px",
        borderRadius: 12,
        boxShadow: "0 18px 45px rgba(0,0,0,0.18)",
        maxWidth: 320
      }}
      role="status"
      aria-live="polite"
    >
      {message}
    </div>
  );
}
