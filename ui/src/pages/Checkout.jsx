import React, { useEffect, useMemo, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiFetch } from "../api/client.js";
import Toast from "../components/Toast.jsx";

function genTxnId() {
  const s = Math.random().toString(16).slice(2, 10).toUpperCase();
  return `TXN-${s}`;
}

function getItemName(it) {
  return it?.name ?? it?.Name ?? it?.productName ?? it?.ProductName ?? "Item";
}

function getItemPrice(it) {
  // Your API uses `price`
  const p =
    it?.price ??
    it?.Price ??
    it?.unitPrice ??
    it?.UnitPrice ??
    it?.productPrice ??
    it?.ProductPrice ??
    0;

  return Number(p) || 0;
}

function getItemQty(it) {
  const q = it?.quantity ?? it?.Quantity ?? 0;
  return Number(q) || 0;
}

export default function Checkout() {
  const nav = useNavigate();
  const [toast, setToast] = useState("");
  const [busy, setBusy] = useState(false);

  const [cart, setCart] = useState(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");

  const [method, setMethod] = useState("Card");
  const [provider, setProvider] = useState("Test");
  const [currency, setCurrency] = useState("USD");

  async function loadCart() {
    setLoading(true);
    setError("");
    try {
      const data = await apiFetch("/api/orders/carts/active/items");
      setCart(data);
    } catch (err) {
      setError(err?.message || "Failed to load cart");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    loadCart();
  }, []);

  const totals = useMemo(() => {
    const items = cart?.items || cart?.Items || [];
    const subtotal = items.reduce((sum, it) => {
      const price = getItemPrice(it);
      const qty = getItemQty(it);
      return sum + price * qty;
    }, 0);

    const amount = Math.round(subtotal * 100) / 100;
    return { items, subtotal: amount };
  }, [cart]);

  async function placeOrder() {
    setBusy(true);
    setToast("");

    try {
      const payment = {
        method,
        provider,
        transactionId: genTxnId(),
        amount: totals.subtotal, // number
        currency
      };

      // ✅ send in BOTH shapes: root + envelope (covers either backend DTO)
      const payload = {
        payment,
        request: { payment }
      };

      const res = await apiFetch("/api/orders/checkout", {
        method: "POST",
        body: JSON.stringify(payload)
      });

      window.dispatchEvent(new Event("kubecart-cart-changed"));

      const orderId = res?.id || res?.orderId || res?.OrderId;
      setToast(orderId ? `Order placed ✅ (${orderId})` : "Order placed ✅");

      setTimeout(() => nav("/orders"), 600);
    } catch (err) {
      setToast(err?.message || "Checkout failed");
    } finally {
      setBusy(false);
    }
  }

  if (loading) {
    return (
      <div className="card">
        <div style={{ padding: 16, color: "#6b7280" }}>Loading checkout…</div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="card">
        <div style={{ padding: 16 }}>
          <div className="notice">{error}</div>
        </div>
      </div>
    );
  }

  return (
    <div style={{ display: "grid", gap: 14 }}>
      <Toast message={toast} onClose={() => setToast("")} />

      <div className="card">
        <div style={{ padding: 16 }}>
          <h1 className="pageTitle">Checkout</h1>
          <p className="pageSub">Review your order and place payment.</p>
        </div>
      </div>

      <div style={{ display: "grid", gridTemplateColumns: "1.2fr 0.8fr", gap: 14 }}>
        <div className="card">
          <div style={{ padding: 16, display: "grid", gap: 10 }}>
            <div style={{ fontWeight: 900 }}>Items</div>

            {totals.items.length === 0 ? (
              <div style={{ color: "#6b7280" }}>Your cart is empty.</div>
            ) : (
              <div style={{ display: "grid", gap: 10 }}>
                {totals.items.map((it) => {
                  const name = getItemName(it);
                  const price = getItemPrice(it);
                  const qty = getItemQty(it);
                  const lineTotal = price * qty;

                  return (
                    <div
                      key={it?.id || it?.Id || `${it?.productId}-${name}`}
                      style={{
                        display: "grid",
                        gridTemplateColumns: "64px 1fr auto",
                        gap: 10,
                        alignItems: "center",
                        padding: 10,
                        border: "1px solid #eee",
                        borderRadius: 12,
                        background: "#fff"
                      }}
                    >
                      <img
                        src={it?.imageUrl || it?.ImageUrl || "/src/assets/img-default.jpg"}
                        alt={name}
                        style={{
                          width: 64,
                          height: 64,
                          borderRadius: 10,
                          objectFit: "cover",
                          background: "#fff"
                        }}
                        onError={(e) => {
                          e.currentTarget.src = "/src/assets/img-default.jpg";
                        }}
                      />

                      <div>
                        <div style={{ fontWeight: 800 }}>{name}</div>
                        <div style={{ color: "#6b7280", fontSize: 12 }}>
                          Qty: {qty} • ${price.toFixed(2)}
                        </div>
                      </div>

                      <div style={{ fontWeight: 900 }}>${lineTotal.toFixed(2)}</div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        </div>

        <div className="card">
          <div style={{ padding: 16, display: "grid", gap: 12 }}>
            <div style={{ fontWeight: 900 }}>Order Summary</div>

            <div style={{ display: "flex", justifyContent: "space-between" }}>
              <span style={{ color: "#6b7280" }}>Subtotal</span>
              <span style={{ fontWeight: 900 }}>${totals.subtotal.toFixed(2)}</span>
            </div>

            <div style={{ height: 1, background: "#eee" }} />

            <div style={{ display: "grid", gap: 10 }}>
              <div style={{ fontWeight: 900 }}>Payment (Test)</div>

              <div style={{ display: "grid", gap: 6 }}>
                <label style={{ fontWeight: 800, fontSize: 12 }}>Method</label>
                <select className="input" value={method} onChange={(e) => setMethod(e.target.value)}>
                  <option value="Card">Card</option>
                  <option value="Cash">Cash</option>
                </select>
              </div>

              <div style={{ display: "grid", gap: 6 }}>
                <label style={{ fontWeight: 800, fontSize: 12 }}>Provider</label>
                <select className="input" value={provider} onChange={(e) => setProvider(e.target.value)}>
                  <option value="Test">Test</option>
                  <option value="Sandbox">Sandbox</option>
                </select>
              </div>

              <div style={{ display: "grid", gap: 6 }}>
                <label style={{ fontWeight: 800, fontSize: 12 }}>Currency</label>
                <select className="input" value={currency} onChange={(e) => setCurrency(e.target.value)}>
                  <option value="USD">USD</option>
                  <option value="EUR">EUR</option>
                </select>
              </div>

              <button
                className="btn-primary"
                type="button"
                onClick={placeOrder}
                disabled={busy || totals.items.length === 0}
                style={{ height: 44, borderRadius: 12 }}
              >
                {busy ? "Placing order..." : `Place Order • $${totals.subtotal.toFixed(2)}`}
              </button>
            </div>

            <div style={{ fontSize: 12, color: "#6b7280" }}>
              This is a test payment for the capstone. No real charges.
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
