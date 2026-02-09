import React, { useEffect, useState } from "react";
import { apiFetch } from "../api/client.js";

export default function TodosPage() {
  const [items, setItems] = useState([]);
  const [title, setTitle] = useState("");
  const [loading, setLoading] = useState(true);
  const [err, setErr] = useState("");

  async function load() {
    setErr("");
    setLoading(true);
    try {
      const data = await apiFetch("/api/todos");
      setItems(Array.isArray(data) ? data : []);
    } catch (e) {
      const msg = String(e?.message ?? e ?? "Failed to load todos");
      // Helpful hint when HTML is returned
      if (msg.includes("Unexpected token '<'")) {
        setErr("Todos API returned HTML (not JSON). This usually means the request did not reach the API. Using apiFetch should fix it—if it still happens, confirm Identity API is running and /api/todos works in Swagger.");
      } else {
        setErr(msg);
      }
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function addTodo(e) {
    e.preventDefault();
    const t = title.trim();
    if (!t) return;

    setErr("");
    try {
      await apiFetch("/api/todos", {
        method: "POST",
        body: JSON.stringify({ title: t }),
      });
      setTitle("");
      await load();
    } catch (e) {
      setErr(String(e?.message ?? e ?? "Failed to add todo"));
    }
  }

  async function toggleTodo(todo) {
    setErr("");
    try {
      await apiFetch(`/api/todos/${todo.id}`, {
        method: "PUT",
        body: JSON.stringify({
          title: todo.title ?? "",
          isDone: !todo.isDone,
        }),
      });
      await load();
    } catch (e) {
      setErr(String(e?.message ?? e ?? "Failed to update todo"));
    }
  }

  async function deleteTodo(todo) {
    setErr("");
    try {
      await apiFetch(`/api/todos/${todo.id}`, { method: "DELETE" });
      await load();
    } catch (e) {
      setErr(String(e?.message ?? e ?? "Failed to delete todo"));
    }
  }

  return (
    <div style={{ maxWidth: 720, margin: "0 auto", padding: 16 }}>
      <h2 style={{ marginBottom: 12 }}>Todos</h2>

      {err ? (
        <div style={{ marginBottom: 12, padding: 10, border: "1px solid #f99" }}>
          {err}
        </div>
      ) : null}

      <form onSubmit={addTodo} style={{ display: "flex", gap: 8, marginBottom: 16 }}>
        <input
          value={title}
          onChange={(e) => setTitle(e.target.value)}
          placeholder="Add a todo..."
          style={{ flex: 1, padding: 10 }}
        />
        <button type="submit" style={{ padding: "10px 14px" }}>
          Add
        </button>
      </form>

      {loading ? (
        <div>Loading...</div>
      ) : items.length === 0 ? (
        <div>No todos yet.</div>
      ) : (
        <ul style={{ listStyle: "none", padding: 0, margin: 0 }}>
          {items.map((t) => (
            <li
              key={t.id}
              style={{
                display: "flex",
                alignItems: "center",
                justifyContent: "space-between",
                padding: 10,
                border: "1px solid #ddd",
                marginBottom: 8,
                gap: 12,
              }}
            >
              <label style={{ display: "flex", alignItems: "center", gap: 10, flex: 1 }}>
                <input type="checkbox" checked={!!t.isDone} onChange={() => toggleTodo(t)} />
                <span style={{ textDecoration: t.isDone ? "line-through" : "none" }}>{t.title}</span>
              </label>

              <button onClick={() => deleteTodo(t)} style={{ padding: "8px 10px" }}>
                Delete
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
