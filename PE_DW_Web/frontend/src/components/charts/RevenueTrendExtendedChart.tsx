import {
  LineChart,
  Line,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from "recharts";
import type { RevenueTrendPoint } from "../../types/dashboard";
import { colors } from "../../theme/colors";

interface Props {
  data: RevenueTrendPoint[];
  loading?: boolean;
}

export default function RevenueTrendExtendedChart({ data, loading }: Props) {
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
        <LineChart data={data}>
          <XAxis dataKey="periodLabel" tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <YAxis yAxisId="left" tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <YAxis yAxisId="right" orientation="right" tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <Tooltip
            contentStyle={{ borderRadius: 8, border: `1px solid ${colors.border}`, fontSize: 13, boxShadow: "0 4px 12px rgba(0,0,0,0.08)" }}
            formatter={(value: number) => value.toLocaleString()}
          />
          <Legend wrapperStyle={{ fontSize: 12, paddingTop: 8 }} />
          <Line yAxisId="left" type="monotone" dataKey="revenue" stroke={colors.primary} strokeWidth={2} name="Revenue" dot={false} isAnimationActive animationDuration={800} />
          <Line yAxisId="right" type="monotone" dataKey="orderCount" stroke={colors.warning} strokeWidth={2} name="Orders" connectNulls dot={false} isAnimationActive animationDuration={800} />
          <Line yAxisId="right" type="monotone" dataKey="quantity" stroke={colors.success} strokeWidth={2} name="Quantity" connectNulls dot={false} isAnimationActive animationDuration={800} />
        </LineChart>
      </ResponsiveContainer>
    </div>
  );
}
