import React, { useEffect, useRef, useState } from "react";
import { Link, useLocation, useNavigate } from "react-router-dom";
import { apiFetch } from "../api/client.js";
import { isLoggedIn, getRole } from "../auth/token.js";
import cartIcon from "../assets/cart.svg";

export default function Header({ loggedIn, onLogout }) {
  const [cartCount, setCartCount] = useState(0);
  const [q, setQ] = useState("");
  const [menuOpen, setMenuOpen] = useState(false);

  const nav = useNavigate();
  const location = useLocation();

  const timerRef = useRef(null);
  const skipNavRef = useRef(false);
  const menuRef = useRef(null);

  const role = loggedIn ? (getRole() || "Customer") : null;
  const displayName = !loggedIn ? null : (role === "Admin" ? "Emad" : "Guest");

  async function loadCartCount() {
    if (!isLoggedIn()) {
      setCartCount(0);
      return;
    }
    try {
      const data = await apiFetch("/api/orders/carts/active/items");
      const items = data?.items || data?.Items || [];
      const count = items.reduce((sum, it) => sum + Number(it?.quantity ?? it?.Quantity ?? 0), 0);
      setCartCount(count);
    } catch {
      setCartCount(0);
    }
  }

  useEffect(() => {
    loadCartCount();
    const onCartChanged = () => loadCartCount();
    const onAuthChanged = () => loadCartCount();

    window.addEventListener("kubecart-cart-changed", onCartChanged);
    window.addEventListener("kubecart-auth-changed", onAuthChanged);

    return () => {
      window.removeEventListener("kubecart-cart-changed", onCartChanged);
      window.removeEventListener("kubecart-auth-changed", onAuthChanged);
    };
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  useEffect(() => {
    function onDocClick(e) {
      if (!menuRef.current) return;
      if (!menuRef.current.contains(e.target)) setMenuOpen(false);
    }
    document.addEventListener("mousedown", onDocClick);
    return () => document.removeEventListener("mousedown", onDocClick);
  }, []);

  useEffect(() => {
    const params = new URLSearchParams(location.search);
    const current = params.get("q") || "";
    skipNavRef.current = true;
    setQ(current);
  }, [location.search]);

  useEffect(() => {
    if (skipNavRef.current) {
      skipNavRef.current = false;
      return;
    }
    if (location.pathname !== "/") return;

    if (timerRef.current) clearTimeout(timerRef.current);

    timerRef.current = setTimeout(() => {
      const term = q.trim();
      const target = term ? `/?q=${encodeURIComponent(term)}` : "/";
      if (location.pathname + location.search !== target) {
        nav(target, { replace: true });
      }
    }, 250);

    return () => {
      if (timerRef.current) clearTimeout(timerRef.current);
    };
  }, [q, nav, location.pathname, location.search]);

  function goSearchNow() {
    const term = q.trim();
    nav(term ? `/?q=${encodeURIComponent(term)}` : "/");
  }

  function goOrders() {
    if (!isLoggedIn()) nav("/login");
    else nav("/orders");
  }

  function goCart() {
    if (!isLoggedIn()) nav("/login");
    else nav("/cart");
  }

  return (
    <div className="azHeaderWrap">
      <div className="azTopBar">
        <div className="azTopInner">
          <Link to="/" className="azLogo" aria-label="KubeCart Home">
            kubecart
          </Link>

          <div className="azDeliver">
            <div className="azDeliverSmall">Delivering to Columbus 43211</div>
            <div className="azDeliverBig">Update location</div>
          </div>

          <div className="azSearch">
            <button className="azAllBtn" type="button">
              All <span className="azCaret">▾</span>
            </button>

            <input
              className="azSearchInput"
              placeholder="Search KubeCart"
              value={q}
              onChange={(e) => setQ(e.target.value)}
              onKeyDown={(e) => {
                if (e.key === "Enter") goSearchNow();
              }}
            />

            <button className="azSearchIcon" type="button" onClick={goSearchNow} aria-label="Search" />
          </div>

          <div className="azRight">
            <div className="azMini">
              <div className="azMiniTop">EN</div>
              <div className="azMiniBottom">🌐</div>
            </div>

            <div className="azAccount" ref={menuRef}>
              <button className="azMini azAccountBtn" type="button" onClick={() => setMenuOpen((v) => !v)}>
                <div className="azMiniTop">
                  {!loggedIn ? "Hello, sign in" : `Hello, ${displayName}`}
                </div>
                <div className="azMiniBottom">
                  Account &amp; Lists <span className="azCaret">▾</span>
                </div>
              </button>

              {menuOpen ? (
                <div className="azDropdown">
                  {!loggedIn ? (
                    <>
                      <button
                        className="azDropPrimary"
                        type="button"
                        onClick={() => {
                          setMenuOpen(false);
                          nav("/login");
                        }}
                      >
                        Sign in
                      </button>
                      <div className="azDropHint">New customer? (skip)</div>
                    </>
                  ) : (
                    <>
                      <button
                        className="azDropItem"
                        type="button"
                        onClick={() => {
                          setMenuOpen(false);
                          nav("/orders");
                        }}
                      >
                        Your Orders
                      </button>
                      <button
                        className="azDropItem"
                        type="button"
                        onClick={() => {
                          setMenuOpen(false);
                          nav("/cart");
                        }}
                      >
                        Your Cart
                      </button>
                      <div className="azDropDivider" />
                      <button
                        className="azDropItem"
                        type="button"
                        onClick={() => {
                          setMenuOpen(false);
                          onLogout();
                        }}
                      >
                        Sign out
                      </button>
                    </>
                  )}
                </div>
              ) : null}
            </div>

            <button type="button" className="azMini" onClick={goOrders} style={{ background: "transparent" }}>
              <div className="azMiniTop">Returns</div>
              <div className="azMiniBottom">&amp; Orders</div>
            </button>

            <button type="button" className="azCart" onClick={goCart} aria-label="Cart">
              <img src={cartIcon} alt="" className="azCartSvg" />
              <div className="azCartCount">{cartCount}</div>
              <div className="azCartText">Cart</div>
            </button>
          </div>
        </div>
      </div>

      <div className="azNavBar">
        <div className="azNavInner">
          <button className="azHamburger" type="button">
            ☰ <span>All</span>
          </button>

          <div className="azNavLinks">
            <a className="azNavLink" href="#">Amazon Haul</a>
            <a className="azNavLink" href="#">Medical Care</a>
            <a className="azNavLink" href="#">Amazon Basics</a>
            <a className="azNavLink" href="#">Best Sellers</a>
            <a className="azNavLink" href="#">Books</a>
            <a className="azNavLink" href="#">Prime</a>
            <a className="azNavLink" href="#">Registry</a>
            <a className="azNavLink" href="#">Gift Cards</a>
            <a className="azNavLink" href="#">Smart Home</a>
            <a className="azNavLink" href="#">New Releases</a>
          </div>
        </div>
      </div>
    </div>
  );
}
