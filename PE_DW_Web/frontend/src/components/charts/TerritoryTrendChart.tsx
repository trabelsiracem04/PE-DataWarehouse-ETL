import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from "recharts";
import type { TerritoryTrendRow } from "../../types/dashboard";
import { colors, categoryColor } from "../../theme/colors";

interface Props {
  data: TerritoryTrendRow[];
  loading?: boolean;
}

export default function TerritoryTrendChart({ data, loading }: Props) {
  if (loading) {
    return <div className="chart-container chart-skeleton">Loading chart...</div>;
  }

  if (data.length === 0) {
    return <div className="chart-container chart-empty">No territory trend data</div>;
  }

  const territories = data.length > 0
    ? Object.keys(data[0].territoryRevenues).sort()
    : [];

  const chartData = data.map((row) => ({
    period: row.period,
    ...row.territoryRevenues,
  }));

  return (
    <div className="chart-container">
      <h3>Revenue by Territory over Time</h3>
      <ResponsiveContainer width="100%" height={320}>
        <BarChart data={chartData}>
          <XAxis dataKey="period" tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <YAxis tick={{ fontSize: 12, fill: colors.text.secondary }} axisLine={false} tickLine={false} />
          <Tooltip
            formatter={(value: number) => `$${value.toLocaleString()}`}
            contentStyle={{ borderRadius: 8, border: `1px solid ${colors.border}`, fontSize: 13, boxShadow: "0 4px 12px rgba(0,0,0,0.08)" }}
          />
          <Legend wrapperStyle={{ fontSize: 12, paddingTop: 8 }} />
          {territories.map((t) => (
            <Bar key={t} dataKey={t} stackId="a" fill={categoryColor(t)} name={t} isAnimationActive animationDuration={800} />
          ))}
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
