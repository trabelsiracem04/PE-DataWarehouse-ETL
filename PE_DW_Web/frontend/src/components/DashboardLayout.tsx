import { useLocation, useNavigate } from "react-router-dom";
import type { ReactNode } from "react";

interface Props {
  children: ReactNode;
}

const NAV_ITEMS = [
  { path: "/", label: "Dashboard" },
  { path: "/reports", label: "Reports" },
];

export default function DashboardLayout({ children }: Props) {
  const location = useLocation();
  const navigate = useNavigate();

  return (
    <div className="app-layout">
      <aside className="sidebar">
        <h2>DW Analytics</h2>
        <nav>
          {NAV_ITEMS.map((item) => (
            <span
              key={item.path}
              className={`nav-item ${location.pathname === item.path ? "active" : ""}`}
              onClick={() => navigate(item.path)}
            >
              {item.label}
            </span>
          ))}
        </nav>
      </aside>
      <main className="main-content">{children}</main>
    </div>
  );
}
