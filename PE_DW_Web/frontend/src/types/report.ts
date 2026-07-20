export interface ReportFilters {
  startDate?: string;
  endDate?: string;
  territory?: string;
  product?: string;
}

export interface SalesDetailRow {
  saleId: string | null;
  orderDate: string | null;
  product: string | null;
  employee: string | null;
  territory: string | null;
  reseller: string | null;
  promotion: string | null;
  currency: string | null;
  quantity: number | null;
  unitPrice: number | null;
  totalAmount: number | null;
}

export interface ProductPerformanceRow {
  product: string;
  totalQuantity: number;
  totalRevenue: number;
  avgPrice: number;
  rank: number;
}

export interface EmployeeSalesRow {
  employee: string;
  totalOrders: number;
  totalQuantity: number;
  totalRevenue: number;
  avgOrderValue: number;
  territory: string | null;
}

export interface TerritorySummaryRow {
  territory: string;
  revenue: number;
  quantity: number;
  orderCount: number;
  revenuePercent: number;
}

export interface PromotionEffectivenessRow {
  promotion: string;
  revenue: number;
  quantity: number;
  orderCount: number;
  avgUnitPrice: number;
}
