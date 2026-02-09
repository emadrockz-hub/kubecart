import React, { useEffect, useMemo, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { apiFetch } from "../api/client.js";
import ProductCard from "../components/ProductCard.jsx";
import Toast from "../components/Toast.jsx";
import { isLoggedIn } from "../auth/token.js";

export default function Home() {
  const location = useLocation();
  const nav = useNavigate();

  const [toast, setToast] = useState("");

  const [products, setProducts] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState("");
  const [addingId, setAddingId] = useState(null);

  const q = useMemo(() => {
    const params = new URLSearchParams(location.search);
    return (params.get("q") || "").trim();
  }, [location.search]);

  async function load() {
    setLoading(true);
    setError("");
    try {
      const data = await apiFetch("/api/catalog/products");
      const list = Array.isArray(data) ? data : (data?.items || data?.Products || []);
      setProducts(list);
    } catch (err) {
      setError(err?.message || "Failed to load products");
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  const filtered = useMemo(() => {
    const needle = q.toLowerCase();
    if (!needle) return products;
    return products.filter((p) => {
      const name = (p?.name || p?.Name || "").toLowerCase();
      return name.includes(needle);
    });
  }, [products, q]);

  async function addToCart(product) {
    // ✅ If not logged in, show toast + redirect to login
    if (!isLoggedIn()) {
      setToast("Please sign in to add items to cart.");
      setTimeout(() => nav("/login"), 400);
      return;
    }

    try {
      const productId = product?.id || product?.Id;
      if (!productId) throw new Error("Product id missing from API response.");

      setAddingId(productId);
      await apiFetch("/api/orders/carts/items", {
        method: "POST",
        body: JSON.stringify({ productId, quantity: 1 })
      });

      window.dispatchEvent(new Event("kubecart-cart-changed"));
      setToast("Added to cart ✅");
    } catch (err) {
      setToast(err?.message || "Add to cart failed.");
    } finally {
      setAddingId(null);
    }
  }

  return (
    <div style={{ display: "grid", gap: 14 }}>
      <Toast message={toast} onClose={() => setToast("")} />

      <div className="card">
        <div style={{ padding: 16, display: "flex", justifyContent: "space-between", alignItems: "center", gap: 10 }}>
          <div>
            <h1 className="pageTitle">Shop</h1>
            {q ? (
              <p className="pageSub">
                Showing results for <b>{q}</b>
              </p>
            ) : (
              <p className="pageSub">Top picks for you</p>
            )}
          </div>

          <button className="btn" onClick={load} type="button" disabled={loading}>
            {loading ? "Loading..." : "Refresh"}
          </button>
        </div>

        {error ? (
          <div style={{ padding: "0 16px 16px 16px" }}>
            <div className="notice">{error}</div>
          </div>
        ) : null}
      </div>

      {loading ? (
        <div className="card">
          <div style={{ padding: 16, color: "#6b7280" }}>Loading products…</div>
        </div>
      ) : (
        <div
          style={{
            display: "grid",
            gap: 14,
            gridTemplateColumns: "repeat(auto-fill, minmax(240px, 1fr))"
          }}
        >
          {filtered.map((p) => {
            const id = p?.id || p?.Id;
            return (
              <ProductCard
                key={id || Math.random()}
                product={p}
                onAdd={addToCart}
                adding={addingId === id}
              />
            );
          })}
        </div>
      )}
    </div>
  );
}
