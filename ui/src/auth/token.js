const TOKEN_KEY = "kubecart_token";

export function getToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token) {
  localStorage.setItem(TOKEN_KEY, token);
}

export function clearToken() {
  localStorage.removeItem(TOKEN_KEY);
}

export function isLoggedIn() {
  return !!getToken();
}

import { getRoleFromToken } from "./jwt.js";

export function getRole() {
  return getRoleFromToken(getToken());
}

export function isAdmin() {
  return getRole() === "Admin";
}
