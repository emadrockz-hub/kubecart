import React, { useEffect, useState } from "react";
import { apiFetch } from "../api/client.js";

function money(n) {
  const x = Number(n || 0);
  return `$${x.toFixed(2)}`;
}

export default function Cart() {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [cart, setCart] = useState(null);

  async function load() {
    setLoading(true);
    setError("");
    try {
      const data = await apiFetch("/api/orders/carts/active/items");
      setCart(data);
    } catch (err) {
      setError(err?.message || "Failed to load cart (are you logged in?)");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  const items =
    cart?.items || cart?.Items || cart || []; // supports different shapes

  const subtotal = items.reduce((sum, it) => {
    const price = it?.price ?? it?.Price ?? it?.productPrice ?? it?.ProductPrice ?? 0;
    const qty = it?.quantity ?? it?.Quantity ?? 0;
    return sum + Number(price) * Number(qty);
  }, 0);

  async function removeItem(item) {
    try {
      const id = item?.id || item?.Id;
      if (!id) throw new Error("Cart item id missing.");
      await apiFetch(`/api/orders/carts/items/${id}`, { method: "DELETE" });
      await load();
      window.dispatchEvent(new Event("kubecart-cart-changed"));
    } catch (err) {
      alert(err?.message || "Remove failed");
    }
  }

  async function updateQty(item, nextQty) {
    try {
      const id = item?.id || item?.Id;
      if (!id) throw new Error("Cart item id missing.");
      if (nextQty <= 0) return removeItem(item);

      await apiFetch(`/api/orders/carts/items/${id}`, {
        method: "PUT",
        body: JSON.stringify({ quantity: nextQty })
      });

      await load();
      window.dispatchEvent(new Event("kubecart-cart-changed"));
    } catch (err) {
      alert(err?.message || "Update qty failed");
    }
  }

  return (
    <div style={{ display: "grid", gap: 14 }}>
      <div className="card">
        <div style={{ padding: 16 }}>
          <h1 className="pageTitle">Cart</h1>
          <p className="pageSub">Your active cart items (Orders API, JWT required).</p>
        </div>
      </div>

      {loading ? (
        <div className="card">
          <div style={{ padding: 16, color: "#6b7280" }}>Loading cart…</div>
        </div>
      ) : error ? (
        <div className="card">
          <div style={{ padding: 16 }}>
            <div className="notice">{error}</div>
            <div style={{ marginTop: 10 }}>
              <button className="btn" onClick={load} type="button">Retry</button>
            </div>
          </div>
        </div>
      ) : items.length === 0 ? (
        <div className="card">
          <div style={{ padding: 16, color: "#6b7280" }}>Your cart is empty.</div>
        </div>
      ) : (
        <div style={{ display: "grid", gap: 14, gridTemplateColumns: "1fr 340px" }}>
          {/* Items */}
          <div style={{ display: "grid", gap: 10 }}>
            {items.map((it) => {
              const id = it?.id || it?.Id || Math.random();
              const name = it?.productName || it?.ProductName || it?.name || it?.Name || "Item";
              const price = it?.price ?? it?.Price ?? it?.productPrice ?? it?.ProductPrice ?? 0;
              const qty = it?.quantity ?? it?.Quantity ?? 0;
              const line = Number(price) * Number(qty);

              return (
                <div key={id} className="card">
                  <div style={{ padding: 14, display: "flex", justifyContent: "space-between", gap: 12 }}>
                    <div style={{ minWidth: 0 }}>
                      <div style={{ fontWeight: 800 }}>{name}</div>
                      <div style={{ color: "#6b7280", marginTop: 4 }}>
                        {money(price)} each • Line: <b>{money(line)}</b>
                      </div>
                    </div>

                    <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                      <button className="btn" type="button" onClick={() => updateQty(it, Number(qty) - 1)}>-</button>
                      <div style={{ width: 34, textAlign: "center", fontWeight: 700 }}>{qty}</div>
                      <button className="btn" type="button" onClick={() => updateQty(it, Number(qty) + 1)}>+</button>
                      <button className="btn" type="button" onClick={() => removeItem(it)}>Remove</button>
                    </div>
                  </div>
                </div>
              );
            })}
          </div>

          {/* Summary */}
          <div className="card" style={{ height: "fit-content" }}>
            <div style={{ padding: 16, display: "grid", gap: 10 }}>
              <div style={{ fontWeight: 900, fontSize: 16 }}>Order Summary</div>
              <div style={{ display: "flex", justifyContent: "space-between" }}>
                <span style={{ color: "#6b7280" }}>Subtotal</span>
                <b>{money(subtotal)}</b>
              </div>
              <button
                className="btn btn-primary"
                type="button"
                onClick={() => (window.location.href = "/checkout")}
              >
                Proceed to Checkout
              </button>
            
              </div>
            </div>
          </div>
      )}
    </div>
  );
}
