export interface OverviewDto {
  totalRevenue: number;
  totalQuantity: number;
  totalOrders: number;
}

export interface RevenueTrendPoint {
  periodLabel: string;
  revenue: number;
}

export interface TerritoryRevenuePoint {
  territory: string;
  revenue: number;
}

export interface ProductRevenuePoint {
  productName: string;
  revenue: number;
}

export interface RevenueByEmployeePoint {
  employeeName: string;
  revenue: number;
}

export interface RevenueByPromotionPoint {
  promotionName: string;
  revenue: number;
}

export interface AovTrendPoint {
  periodLabel: string;
  averageOrderValue: number;
}

export interface TerritoryTrendRow {
  period: string;
  territoryRevenues: Record<string, number>;
}

export interface RevenueByCurrencyPoint {
  currency: string;
  revenue: number;
}
