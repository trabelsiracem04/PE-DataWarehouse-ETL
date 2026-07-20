import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Cell,
} from "recharts";
import type { RevenueByCurrencyPoint } from "../../types/dashboard";
import { colors, barShade } from "../../theme/colors";

interface Props {
  data: RevenueByCurrencyPoint[];
  loading?: boolean;
}

export default function RevenueByCurrencyChart({ data, loading }: Props) {
  if (loading) {
    return <div className="chart-container chart-skeleton">Loading chart...</div>;
  }

  if (data.length === 0) {
    return <div className="chart-container chart-empty">No currency data</div>;
  }

  const sorted = [...data].sort((a, b) => b.revenue - a.revenue);

  return (
    <div className="chart-container">
      <h3>Revenue by Currency</h3>
      <ResponsiveContainer width="100%" height={280}>
        <BarChart data={sorted} layout="vertical" margin={{ left: 10, right: 16, top: 4, bottom: 4 }}>
          <XAxis type="number" tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <YAxis dataKey="currency" type="category" width={50} tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <Tooltip
            formatter={(value: number) => `$${value.toLocaleString()}`}
            contentStyle={{ borderRadius: 8, border: `1px solid ${colors.border}`, fontSize: 13, boxShadow: "0 4px 12px rgba(0,0,0,0.08)" }}
          />
          <Bar dataKey="revenue" radius={[0, 4, 4, 0]} isAnimationActive animationDuration={800}>
            {sorted.map((entry, i) => (
              <Cell key={entry.currency} fill={barShade(i, sorted.length)} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
