import {
  AreaChart,
  Area,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import type { RevenueTrendPoint } from "../../types/dashboard";

interface Props {
  data: RevenueTrendPoint[];
  loading?: boolean;
}

export default function RevenueTrendChart({ data, loading }: Props) {
  if (loading) {
    return <div className="chart-container chart-skeleton">Loading chart...</div>;
  }

  if (data.length === 0) {
    return <div className="chart-container chart-empty">No revenue trend data</div>;
  }

  return (
    <div className="chart-container">
      <h3>Revenue Trend</h3>
      <ResponsiveContainer width="100%" height={300}>
        <AreaChart data={data}>
          <CartesianGrid strokeDasharray="3 3" />
          <XAxis dataKey="periodLabel" tick={{ fontSize: 12 }} />
          <YAxis tick={{ fontSize: 12 }} />
          <Tooltip formatter={(value: number) => value.toLocaleString()} />
          <Area
            type="monotone"
            dataKey="revenue"
            stroke="#4f46e5"
            fill="#818cf8"
            fillOpacity={0.3}
          />
        </AreaChart>
      </ResponsiveContainer>
    </div>
  );
}
