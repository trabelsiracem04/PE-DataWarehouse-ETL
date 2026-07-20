import { useEffect, useState } from "react";
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
import {
  fetchOverview,
  fetchRevenueTrendExtended,
  fetchRevenueByTerritory,
  fetchRevenueByProduct,
  fetchRevenueByEmployee,
  fetchRevenueByPromotion,
  fetchAovTrend,
  fetchRevenueByTerritoryTrend,
  fetchRevenueByCurrency,
} from "../api/dashboardApi";
import { colors } from "../theme/colors";
import DashboardHeader from "../components/DashboardHeader";
import KpiCard from "../components/kpi/KpiCard";
import RevenueTrendExtendedChart from "../components/charts/RevenueTrendExtendedChart";
import RevenueByTerritoryChart from "../components/charts/RevenueByTerritoryChart";
import TopProductsChart from "../components/charts/TopProductsChart";
import RevenueByEmployeeChart from "../components/charts/RevenueByEmployeeChart";
import RevenueByPromotionChart from "../components/charts/RevenueByPromotionChart";
import AovTrendChart from "../components/charts/AovTrendChart";
import TerritoryTrendChart from "../components/charts/TerritoryTrendChart";
import RevenueByCurrencyChart from "../components/charts/RevenueByCurrencyChart";

export default function DashboardPage() {
  const [overview, setOverview] = useState<OverviewDto | null>(null);
  const [trend, setTrend] = useState<RevenueTrendPoint[]>([]);
  const [territory, setTerritory] = useState<TerritoryRevenuePoint[]>([]);
  const [products, setProducts] = useState<ProductRevenuePoint[]>([]);
  const [employees, setEmployees] = useState<RevenueByEmployeePoint[]>([]);
  const [promotions, setPromotions] = useState<RevenueByPromotionPoint[]>([]);
  const [aov, setAov] = useState<AovTrendPoint[]>([]);
  const [territoryTrend, setTerritoryTrend] = useState<TerritoryTrendRow[]>([]);
  const [currencies, setCurrencies] = useState<RevenueByCurrencyPoint[]>([]);

  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;

    async function loadAll() {
      setLoading(true);
      setError(null);
      try {
        const [ov, tr, te, pr, em, pm, av, tt, cu] = await Promise.all([
          fetchOverview(),
          fetchRevenueTrendExtended(),
          fetchRevenueByTerritory(),
          fetchRevenueByProduct(10),
          fetchRevenueByEmployee(10),
          fetchRevenueByPromotion(),
          fetchAovTrend(),
          fetchRevenueByTerritoryTrend(),
          fetchRevenueByCurrency(),
        ]);
        if (!cancelled) {
          setOverview(ov);
          setTrend(tr);
          setTerritory(te);
          setProducts(pr);
          setEmployees(em);
          setPromotions(pm);
          setAov(av);
          setTerritoryTrend(tt);
          setCurrencies(cu);
        }
      } catch (err) {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Failed to load dashboard data");
        }
      } finally {
        if (!cancelled) setLoading(false);
      }
    }

    loadAll();
    return () => { cancelled = true; };
  }, []);

  return (
    <div className="dashboard-page">
      <DashboardHeader />

      {error && <div className="error-banner">{error}</div>}

      <div className="kpi-row">
        <div className="panel" style={{ animationDelay: "0ms" }}>
          <KpiCard
            label="Total Revenue"
            value={overview ? `$${overview.totalRevenue.toLocaleString()}` : "-"}
            loading={loading}
            accentColor={colors.primary}
          />
        </div>
        <div className="panel" style={{ animationDelay: "80ms" }}>
          <KpiCard
            label="Total Quantity"
            value={overview?.totalQuantity ?? "-"}
            loading={loading}
            accentColor={colors.success}
          />
        </div>
        <div className="panel" style={{ animationDelay: "160ms" }}>
          <KpiCard
            label="Total Orders"
            value={overview?.totalOrders ?? "-"}
            loading={loading}
            accentColor={colors.warning}
          />
        </div>
      </div>

      <div className="panel chart-full" style={{ animationDelay: "240ms" }}>
        <RevenueTrendExtendedChart data={trend} loading={loading} />
      </div>

      <div className="charts-grid">
        <div className="panel" style={{ animationDelay: "320ms" }}>
          <RevenueByTerritoryChart data={territory} loading={loading} />
        </div>
        <div className="panel" style={{ animationDelay: "400ms" }}>
          <RevenueByCurrencyChart data={currencies} loading={loading} />
        </div>
        <div className="panel" style={{ animationDelay: "480ms" }}>
          <TopProductsChart data={products} loading={loading} />
        </div>
        <div className="panel" style={{ animationDelay: "560ms" }}>
          <RevenueByEmployeeChart data={employees} loading={loading} />
        </div>
      </div>

      <div className="charts-grid">
        <div className="panel" style={{ animationDelay: "640ms" }}>
          <RevenueByPromotionChart data={promotions} loading={loading} />
        </div>
        <div className="panel" style={{ animationDelay: "720ms" }}>
          <AovTrendChart data={aov} loading={loading} />
        </div>
      </div>

      <div className="panel" style={{ animationDelay: "800ms" }}>
        <TerritoryTrendChart data={territoryTrend} loading={loading} />
      </div>
    </div>
  );
}
