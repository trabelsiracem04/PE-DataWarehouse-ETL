import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from "recharts";
import type { ProductRevenuePoint } from "../../types/dashboard";
import { colors, barShade } from "../../theme/colors";

interface Props {
  data: ProductRevenuePoint[];
  loading?: boolean;
}

export default function TopProductsChart({ data, loading }: Props) {
  if (loading) {
    return <div className="chart-container chart-skeleton">Loading chart...</div>;
  }

  if (data.length === 0) {
    return <div className="chart-container chart-empty">No product data</div>;
  }

  return (
    <div className="chart-container">
      <h3>Top Products by Revenue</h3>
      <ResponsiveContainer width="100%" height={280}>
        <BarChart data={data} layout="vertical" margin={{ left: 10, right: 16, top: 4, bottom: 4 }}>
          <XAxis type="number" tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <YAxis dataKey="productName" type="category" width={100} tick={{ fontSize: 11, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <Tooltip
            formatter={(value: number) => `$${value.toLocaleString()}`}
            contentStyle={{ borderRadius: 8, border: `1px solid ${colors.border}`, fontSize: 13, boxShadow: "0 4px 12px rgba(0,0,0,0.08)" }}
          />
          <Bar dataKey="revenue" radius={[0, 4, 4, 0]} isAnimationActive animationDuration={800}>
            {data.map((entry, i) => (
              <Cell key={entry.productName} fill={barShade(i, data.length)} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
