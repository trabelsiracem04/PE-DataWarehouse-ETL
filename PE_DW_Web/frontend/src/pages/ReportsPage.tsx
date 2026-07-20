import { useEffect, useState, useCallback } from "react";
import type {
  ReportFilters,
  SalesDetailRow,
  ProductPerformanceRow,
  EmployeeSalesRow,
  TerritorySummaryRow,
  PromotionEffectivenessRow,
} from "../types/report";
import {
  fetchSalesDetail,
  fetchProductPerformance,
  fetchEmployeeSales,
  fetchTerritorySummary,
  fetchPromotionEffectiveness,
  exportExcel,
  exportPdf,
  fetchDateRange,
} from "../api/reportApi";

type Tab = "sales-detail" | "product-performance" | "employee-sales" | "territory-summary" | "promotion-effectiveness";

const TABS: { id: Tab; label: string }[] = [
  { id: "sales-detail", label: "Sales Detail" },
  { id: "product-performance", label: "Product Performance" },
  { id: "employee-sales", label: "Employee Sales" },
  { id: "territory-summary", label: "Territory Summary" },
  { id: "promotion-effectiveness", label: "Promotion Effectiveness" },
];

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<Tab>("sales-detail");
  const [dateRange, setDateRange] = useState<{ minDate: string; maxDate: string } | null>(null);
  const [filters, setFilters] = useState<ReportFilters>({});
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [salesDetail, setSalesDetail] = useState<SalesDetailRow[]>([]);
  const [productPerformance, setProductPerformance] = useState<ProductPerformanceRow[]>([]);
  const [employeeSales, setEmployeeSales] = useState<EmployeeSalesRow[]>([]);
  const [territorySummary, setTerritorySummary] = useState<TerritorySummaryRow[]>([]);
  const [promotionEffectiveness, setPromotionEffectiveness] = useState<PromotionEffectivenessRow[]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      switch (activeTab) {
        case "sales-detail":
          setSalesDetail(await fetchSalesDetail(filters));
          break;
        case "product-performance":
          setProductPerformance(await fetchProductPerformance());
          break;
        case "employee-sales":
          setEmployeeSales(await fetchEmployeeSales(filters));
          break;
        case "territory-summary":
          setTerritorySummary(await fetchTerritorySummary());
          break;
        case "promotion-effectiveness":
          setPromotionEffectiveness(await fetchPromotionEffectiveness(filters));
          break;
      }
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to load report");
    } finally {
      setLoading(false);
    }
  }, [activeTab, filters]);

  useEffect(() => {
    fetchDateRange().then((dr) => {
      setDateRange(dr);
      setFilters({ startDate: dr.minDate, endDate: dr.maxDate });
    }).catch(() => {});
  }, []);

  useEffect(() => { if (dateRange) loadData(); }, [loadData]);

  const handleExport = async (format: "excel" | "pdf") => {
    try {
      const blob = format === "excel"
        ? await exportExcel(filters)
        : await exportPdf(filters);
      const ext = format === "excel" ? "xlsx" : "pdf";
      const url = URL.createObjectURL(blob);
      const a = document.createElement("a");
      a.href = url;
      a.download = `SalesDetail.${ext}`;
      a.click();
      URL.revokeObjectURL(url);
    } catch {
      setError(`Failed to export ${format}`);
    }
  };

  return (
    <div className="reports-page">
      <h1>Reports</h1>

      <div className="reports-toolbar">
        <div className="filters-row">
          <label>
            Start Date
            <input
              type="date"
              min={dateRange?.minDate}
              max={dateRange?.maxDate}
              value={filters.startDate ?? ""}
              onChange={(e) => setFilters((f) => ({ ...f, startDate: e.target.value || undefined }))}
            />
          </label>
          <label>
            End Date
            <input
              type="date"
              min={dateRange?.minDate}
              max={dateRange?.maxDate}
              value={filters.endDate ?? ""}
              onChange={(e) => setFilters((f) => ({ ...f, endDate: e.target.value || undefined }))}
            />
          </label>
          <button className="btn btn-primary" onClick={loadData} disabled={loading}>
            {loading ? "Loading..." : "Run"}
          </button>
          <span className="export-group">
            <button className="btn btn-success" onClick={() => handleExport("excel")}>
              Export Excel
            </button>
            <button className="btn btn-outline" onClick={() => handleExport("pdf")}>
              Export PDF
            </button>
          </span>
        </div>
      </div>

      <div className="tabs">
        {TABS.map((tab) => (
          <button
            key={tab.id}
            className={`tab ${activeTab === tab.id ? "active" : ""}`}
            onClick={() => setActiveTab(tab.id)}
          >
            {tab.label}
          </button>
        ))}
      </div>

      {error && <div className="error-banner">{error}</div>}

      {loading ? (
        <div className="loading-spinner">Loading...</div>
      ) : (
        <div className="report-content">
          {activeTab === "sales-detail" && <SalesDetailTable data={salesDetail} />}
          {activeTab === "product-performance" && <ProductPerformanceTable data={productPerformance} />}
          {activeTab === "employee-sales" && <EmployeeSalesTable data={employeeSales} />}
          {activeTab === "territory-summary" && <TerritorySummaryTable data={territorySummary} />}
          {activeTab === "promotion-effectiveness" && <PromotionEffectivenessTable data={promotionEffectiveness} />}
        </div>
      )}
    </div>
  );
}

function SalesDetailTable({ data }: { data: SalesDetailRow[] }) {
  return (
    <div className="table-wrapper">
      <table className="report-table">
        <thead>
          <tr>
            <th>Sale ID</th><th>Date</th><th>Product</th><th>Employee</th>
            <th>Territory</th><th>Reseller</th><th>Promotion</th><th>Currency</th>
            <th>Qty</th><th>Unit Price</th><th>Total</th>
          </tr>
        </thead>
        <tbody>
          {data.map((r, i) => (
            <tr key={i}>
              <td>{r.saleId}</td>
              <td>{r.orderDate}</td>
              <td>{r.product}</td>
              <td>{r.employee}</td>
              <td>{r.territory}</td>
              <td>{r.reseller}</td>
              <td>{r.promotion}</td>
              <td>{r.currency}</td>
              <td>{r.quantity}</td>
              <td>{r.unitPrice?.toFixed(2)}</td>
              <td>{r.totalAmount?.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function ProductPerformanceTable({ data }: { data: ProductPerformanceRow[] }) {
  return (
    <div className="table-wrapper">
      <table className="report-table">
        <thead>
          <tr><th>Rank</th><th>Product</th><th>Total Qty</th><th>Revenue</th><th>Avg Price</th></tr>
        </thead>
        <tbody>
          {data.map((r) => (
            <tr key={r.product}>
              <td>{r.rank}</td>
              <td>{r.product}</td>
              <td>{r.totalQuantity}</td>
              <td>{r.totalRevenue.toFixed(2)}</td>
              <td>{r.avgPrice.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function EmployeeSalesTable({ data }: { data: EmployeeSalesRow[] }) {
  return (
    <div className="table-wrapper">
      <table className="report-table">
        <thead>
          <tr><th>Employee</th><th>Orders</th><th>Qty</th><th>Revenue</th><th>Avg Order Value</th><th>Territories</th></tr>
        </thead>
        <tbody>
          {data.map((r) => (
            <tr key={r.employee}>
              <td>{r.employee}</td>
              <td>{r.totalOrders}</td>
              <td>{r.totalQuantity}</td>
              <td>{r.totalRevenue.toFixed(2)}</td>
              <td>{r.avgOrderValue.toFixed(2)}</td>
              <td>{r.territory}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function TerritorySummaryTable({ data }: { data: TerritorySummaryRow[] }) {
  return (
    <div className="table-wrapper">
      <table className="report-table">
        <thead>
          <tr><th>Territory</th><th>Revenue</th><th>Qty</th><th>Orders</th><th>% of Total</th></tr>
        </thead>
        <tbody>
          {data.map((r) => (
            <tr key={r.territory}>
              <td>{r.territory}</td>
              <td>{r.revenue.toFixed(2)}</td>
              <td>{r.quantity}</td>
              <td>{r.orderCount}</td>
              <td>{r.revenuePercent.toFixed(1)}%</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

function PromotionEffectivenessTable({ data }: { data: PromotionEffectivenessRow[] }) {
  return (
    <div className="table-wrapper">
      <table className="report-table">
        <thead>
          <tr><th>Promotion</th><th>Revenue</th><th>Qty</th><th>Orders</th><th>Avg Unit Price</th></tr>
        </thead>
        <tbody>
          {data.map((r) => (
            <tr key={r.promotion}>
              <td>{r.promotion}</td>
              <td>{r.revenue.toFixed(2)}</td>
              <td>{r.quantity}</td>
              <td>{r.orderCount}</td>
              <td>{r.avgUnitPrice.toFixed(2)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}
