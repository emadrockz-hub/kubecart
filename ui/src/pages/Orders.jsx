import React, { useEffect, useMemo, useState } from "react";
import { apiFetch } from "../api/client.js";
import placeholder from "../assets/placeholder.svg";

function money(n) {
  const x = Number(n || 0);
  return `$${x.toFixed(2)}`;
}

export default function Orders() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [orders, setOrders] = useState([]);
  const [openId, setOpenId] = useState(null);

  async function load() {
    setLoading(true);
    setError("");
    try {
      const data = await apiFetch("/api/orders/orders");
      setOrders(Array.isArray(data) ? data : (data?.items || []));
    } catch (err) {
      setError(err?.message || "Failed to load orders (are you logged in?)");
      setOrders([]);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  const sorted = useMemo(() => {
    return [...orders].sort((a, b) => {
      const da = new Date(a?.createdAtUtc || 0).getTime();
      const db = new Date(b?.createdAtUtc || 0).getTime();
      return db - da;
    });
  }, [orders]);

  return (
    <div style={{ display: "grid", gap: 14 }}>
      <div className="card">
        <div style={{ padding: 16, display: "flex", justifyContent: "space-between", gap: 10, alignItems: "center" }}>
          <div>
            <h1 className="pageTitle">My Orders</h1>
            <p className="pageSub">Track your recent purchases.</p>
          </div>

          <button className="btn" onClick={load} disabled={loading} type="button">
            {loading ? "Loading..." : "Refresh"}
          </button>
        </div>
      </div>

      {loading ? (
        <div className="card">
          <div style={{ padding: 16, color: "#6b7280" }}>Loading orders…</div>
        </div>
      ) : error ? (
        <div className="card">
          <div style={{ padding: 16, display: "grid", gap: 10 }}>
            <div className="notice">{error}</div>
            <button className="btn" onClick={load} type="button">Retry</button>
          </div>
        </div>
      ) : sorted.length === 0 ? (
        <div className="card">
          <div style={{ padding: 16, color: "#6b7280" }}>
            No orders yet. Place an order from Checkout.
          </div>
        </div>
      ) : (
        <div style={{ display: "grid", gap: 10 }}>
          {sorted.map((o) => {
            const id = o?.id;
            const status = o?.status || "—";
            const total = o?.totalAmount ?? 0;
const created = o?.createdAtUtc
  ? new Date(o.createdAtUtc + "Z").toLocaleString() // treat as UTC then display in local timezone
  : "";
            const items = o?.items || [];
            const isOpen = openId === id;

            return (
              <div key={id} className="card">
                <div style={{ padding: 14, display: "grid", gap: 10 }}>
                  <div style={{ display: "flex", justifyContent: "space-between", gap: 10, alignItems: "center" }}>
                    <div style={{ display: "grid", gap: 4 }}>
                      <div style={{ fontWeight: 900 }}>Order #{String(id).slice(0, 8)}</div>
                      <div style={{ color: "#6b7280", fontSize: 13 }}>{created}</div>
                    </div>

                    <div style={{ display: "flex", gap: 10, flexWrap: "wrap", justifyContent: "flex-end" }}>
                      <span className="pill">
                        Status: <b style={{ marginLeft: 6 }}>{status}</b>
                      </span>
                      <span className="pill">
                        Total: <b style={{ marginLeft: 6 }}>{money(total)}</b>
                      </span>
                      <button
                        className="btn"
                        type="button"
                        onClick={() => setOpenId(isOpen ? null : id)}
                      >
                        {isOpen ? "Hide items" : `View items (${items.length})`}
                      </button>
                    </div>
                  </div>

                  {isOpen ? (
                    <div style={{ borderTop: "1px solid #e5e7eb", paddingTop: 12, display: "grid", gap: 10 }}>
                      {items.map((it) => {
                        const key = `${id}-${it.productId}-${it.productName}`;
                        return (
                          <div key={key} style={{ display: "flex", gap: 12, alignItems: "center" }}>
                            <img
                              src={it?.imageUrl || placeholder}
                              onError={(e) => (e.currentTarget.src = placeholder)}
                              alt={it?.productName || "item"}
                              style={{ width: 56, height: 56, borderRadius: 12, objectFit: "cover", border: "1px solid #e5e7eb" }}
                            />
                            <div style={{ flex: 1, minWidth: 0 }}>
                              <div style={{ fontWeight: 800, whiteSpace: "nowrap", overflow: "hidden", textOverflow: "ellipsis" }}>
                                {it?.productName}
                              </div>
                              <div style={{ color: "#6b7280", fontSize: 13 }}>
                                {it?.quantity} × {money(it?.unitPrice)} • Line: <b>{money(it?.lineTotal)}</b>
                              </div>
                            </div>
                          </div>
                        );
                      })}
                    </div>
                  ) : null}
                </div>
              </div>
            );
          })}
        </div>
      )}
    </div>
  );
}
