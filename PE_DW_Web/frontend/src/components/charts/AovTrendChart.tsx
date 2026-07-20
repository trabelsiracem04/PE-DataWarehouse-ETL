import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
} from "recharts";
import type { AovTrendPoint } from "../../types/dashboard";
import { colors } from "../../theme/colors";

interface Props {
  data: AovTrendPoint[];
  loading?: boolean;
}

export default function AovTrendChart({ data, loading }: Props) {
  if (loading) {
    return <div className="chart-container chart-skeleton">Loading chart...</div>;
  }

  if (data.length === 0) {
    return <div className="chart-container chart-empty">No AOV trend data</div>;
  }

  return (
    <div className="chart-container">
      <h3>Average Order Value Trend</h3>
      <ResponsiveContainer width="100%" height={280}>
        <LineChart data={data}>
          <XAxis dataKey="periodLabel" tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <YAxis tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <Tooltip
            formatter={(value: number) => `$${value.toLocaleString()}`}
            contentStyle={{ borderRadius: 8, border: `1px solid ${colors.border}`, fontSize: 13, boxShadow: "0 4px 12px rgba(0,0,0,0.08)" }}
          />
          <Line type="monotone" dataKey="averageOrderValue" stroke={colors.primary} strokeWidth={2} dot={{ r: 3, fill: colors.primary }} isAnimationActive animationDuration={800} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
