import type {
  OverviewDto,
  RevenueTrendPoint,
  TerritoryRevenuePoint,
  ProductRevenuePoint,
  RevenueByEmployeePoint,
  RevenueByPromotionPoint,
  AovTrendPoint,
  TerritoryTrendRow,
  RevenueByCurrencyPoint,
} from "../types/dashboard";

const BASE_URL = import.meta.env.VITE_API_BASE_URL || "http://localhost:5125";

async function fetchJson<T>(path: string): Promise<T> {
  const res = await fetch(`${BASE_URL}${path}`);
  if (!res.ok) {
    throw new Error(`API error ${res.status}: ${res.statusText}`);
  }
  return res.json();
}

export function fetchOverview(): Promise<OverviewDto> {
  return fetchJson<OverviewDto>("/api/dashboard/overview");
}

export function fetchRevenueTrend(): Promise<RevenueTrendPoint[]> {
  return fetchJson<RevenueTrendPoint[]>("/api/dashboard/revenue-trend");
}

export function fetchRevenueTrendExtended(): Promise<RevenueTrendPoint[]> {
  return fetchJson<RevenueTrendPoint[]>("/api/dashboard/revenue-trend-extended");
}

export function fetchRevenueByTerritory(): Promise<TerritoryRevenuePoint[]> {
  return fetchJson<TerritoryRevenuePoint[]>("/api/dashboard/revenue-by-territory");
}

export function fetchRevenueByProduct(top = 10): Promise<ProductRevenuePoint[]> {
  return fetchJson<ProductRevenuePoint[]>(`/api/dashboard/revenue-by-product?top=${top}`);
}

export function fetchRevenueByEmployee(top = 10): Promise<RevenueByEmployeePoint[]> {
  return fetchJson<RevenueByEmployeePoint[]>(`/api/dashboard/revenue-by-employee?top=${top}`);
}

export function fetchRevenueByPromotion(): Promise<RevenueByPromotionPoint[]> {
  return fetchJson<RevenueByPromotionPoint[]>("/api/dashboard/revenue-by-promotion");
}

export function fetchAovTrend(): Promise<AovTrendPoint[]> {
  return fetchJson<AovTrendPoint[]>("/api/dashboard/aov-trend");
}

export function fetchRevenueByTerritoryTrend(): Promise<TerritoryTrendRow[]> {
  return fetchJson<TerritoryTrendRow[]>("/api/dashboard/revenue-by-territory-trend");
}

export function fetchRevenueByCurrency(): Promise<RevenueByCurrencyPoint[]> {
  return fetchJson<RevenueByCurrencyPoint[]>("/api/dashboard/revenue-by-currency");
}
