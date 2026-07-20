import type { ReportFilters, SalesDetailRow, ProductPerformanceRow, EmployeeSalesRow, TerritorySummaryRow, PromotionEffectivenessRow } from "../types/report";

const BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5125";

async function postJson<T>(path: string, body?: unknown): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: body ? JSON.stringify(body) : undefined,
  });
  if (!res.ok) throw new Error(`API error ${res.status}: ${res.statusText}`);
  return res.json();
}

async function postBlob(path: string, body: unknown): Promise<Blob> {
  const res = await fetch(`${BASE_URL}${path}`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(body),
  });
  if (!res.ok) throw new Error(`API error ${res.status}: ${res.statusText}`);
  return res.blob();
}

export function fetchSalesDetail(filters: ReportFilters) {
  return postJson<SalesDetailRow[]>("/api/reports/sales-detail", filters);
}

export function fetchProductPerformance() {
  return postJson<ProductPerformanceRow[]>("/api/reports/product-performance");
}

export function fetchEmployeeSales(filters: ReportFilters) {
  return postJson<EmployeeSalesRow[]>("/api/reports/employee-sales", filters);
}

export function fetchTerritorySummary() {
  return postJson<TerritorySummaryRow[]>("/api/reports/territory-summary");
}

export function fetchPromotionEffectiveness(filters: ReportFilters) {
  return postJson<PromotionEffectivenessRow[]>("/api/reports/promotion-effectiveness", filters);
}

export function exportExcel(filters: ReportFilters) {
  return postBlob("/api/reports/export-excel", filters);
}

export function exportPdf(filters: ReportFilters) {
  return postBlob("/api/reports/export-pdf", filters);
}

export async function fetchDateRange(): Promise<{ minDate: string; maxDate: string }> {
  const res = await fetch(`${BASE_URL}/api/reports/date-range`);
  if (!res.ok) throw new Error(`API error ${res.status}: ${res.statusText}`);
  return res.json();
}
