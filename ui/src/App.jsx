import React, { useEffect, useState } from "react";
import { Routes, Route, Navigate, useNavigate } from "react-router-dom";
import Header from "./components/Header.jsx";
import { isLoggedIn, clearToken } from "./auth/token.js";
import ProtectedRoute from "./auth/ProtectedRoute.jsx";
import Register from "./pages/Register.jsx";
import Home from "./pages/Home.jsx";
import Login from "./pages/Login.jsx";
import Cart from "./pages/Cart.jsx";
import Checkout from "./pages/Checkout.jsx";
import Orders from "./pages/Orders.jsx";

export default function App() {
  const [loggedIn, setLoggedIn] = useState(isLoggedIn());
  const nav = useNavigate();

  useEffect(() => {
    const onAuth = () => setLoggedIn(isLoggedIn());
    window.addEventListener("kubecart-auth-changed", onAuth);
    return () => window.removeEventListener("kubecart-auth-changed", onAuth);
  }, []);

  function logout() {
    clearToken();
    window.dispatchEvent(new Event("kubecart-auth-changed"));
    nav("/login");
  }

  return (
    <div>
      <Header loggedIn={loggedIn} onLogout={logout} />

      <main className="container">
        <Routes>
  <Route path="/" element={<Home />} />

  <Route path="/login" element={<Login />} />
  <Route path="/register" element={<Register />} />

  <Route
    path="/cart"
    element={
      <ProtectedRoute>
        <Cart />
      </ProtectedRoute>
    }
  />
  <Route
    path="/checkout"
    element={
      <ProtectedRoute>
        <Checkout />
      </ProtectedRoute>
    }
  />
  <Route
    path="/orders"
    element={
      <ProtectedRoute>
        <Orders />
      </ProtectedRoute>
    }
  />
  <Route path="*" element={<Navigate to="/" replace />} />
</Routes>
      </main>
    </div>
  );
}
