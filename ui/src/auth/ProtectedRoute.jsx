import React from "react";
import { Navigate, useLocation } from "react-router-dom";
import { isLoggedIn } from "./token.js";

export default function ProtectedRoute({ children }) {
  const loc = useLocation();
  if (!isLoggedIn()) {
    return <Navigate to="/login" replace state={{ from: loc.pathname }} />;
  }
  return children;
}
