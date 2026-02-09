import React, { useMemo } from "react";

function clamp(n, a, b) {
  return Math.max(a, Math.min(b, n));
}

function makeRating(id) {
  const base = Number(id || 1);
  const r = 3.6 + ((base * 97) % 14) / 10;
  const count = 120 + ((base * 53) % 900);
  return { rating: clamp(Number(r.toFixed(1)), 3.6, 5.0), count };
}

function Stars({ value }) {
  const full = Math.floor(value);
  const half = value - full >= 0.5;
  return (
    <span style={{ display: "inline-flex", gap: 2, alignItems: "center" }}>
      {Array.from({ length: 5 }).map((_, i) => {
        const idx = i + 1;
        const char = idx <= full ? "★" : (idx === full + 1 && half ? "⯪" : "☆");
        return (
          <span key={i} style={{ color: "#f59e0b", fontSize: 14, lineHeight: 1 }}>
            {char}
          </span>
        );
      })}
    </span>
  );
}

function localImageFor(name) {
  const n = (name || "").toLowerCase();

  if (n.includes("monitor") || n.includes("laptop") || n.includes("keyboard") || n.includes("mouse")) {
    return "/src/assets/img-tech.jpg";
  }
  if (n.includes("speaker") || n.includes("headphone") || n.includes("earbud")) {
    return "/src/assets/img-audio.jpg";
  }
  if (n.includes("trimmer") || n.includes("shaver") || n.includes("groom")) {
    return "/src/assets/img-groom.jpg";
  }
  if (n.includes("shirt") || n.includes("t-shirt") || n.includes("hoodie") || n.includes("jacket") || n.includes("shoe")) {
    return "/src/assets/img-fashion.jpg";
  }
  return "/src/assets/img-default.jpg";
}

export default function ProductCard({ product, onAdd, adding }) {
  const id = product?.id ?? product?.Id;
  const name = product?.name ?? product?.Name ?? "Product";
  const price = product?.price ?? product?.Price ?? 0;
  const stock = product?.stockQuantity ?? product?.StockQuantity ?? product?.stock ?? product?.Stock ?? null;

  const img = localImageFor(name);
  const { rating, count } = useMemo(() => makeRating(id), [id]);
  const lowStock = typeof stock === "number" && stock > 0 && stock <= 10;

  return (
    <div className="azCard">
      <div className="azCardImgWrap">
        <img
          src={img}
          alt={name}
          className="azCardImg"
          onError={(e) => {
            e.currentTarget.onerror = null;
            e.currentTarget.src = "/src/assets/img-default.jpg";
          }}
        />
      </div>

      <div className="azCardBody">
        <div className="azCardTitle" title={name}>{name}</div>

        <div className="azCardRatingRow">
          <Stars value={rating} />
          <span className="azCardRatingCount">{count.toLocaleString()}</span>
        </div>

        <div className="azCardPriceRow">
          <span className="azPrice">$</span>
          <span className="azPriceMain">{Number(price).toFixed(2)}</span>
        </div>

        <div className="azPrimeRow">
          <span className="azPrimeBadge">Prime</span>
          {lowStock ? <span className="azLowStock">Limited stock</span> : null}
          {typeof stock === "number" && stock > 0 ? (
            <span className="azStock">{stock} in stock</span>
          ) : null}
        </div>

        <button className="azAddBtn" type="button" onClick={() => onAdd(product)} disabled={!!adding}>
          {adding ? "Adding..." : "Add to Cart"}
        </button>
      </div>
    </div>
  );
}
