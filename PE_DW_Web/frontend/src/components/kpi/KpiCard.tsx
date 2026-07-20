interface KpiCardProps {
  label: string;
  value: string | number;
  loading?: boolean;
  accentColor?: string;
  trend?: number;
}

export default function KpiCard({ label, value, loading, accentColor, trend }: KpiCardProps) {
  return (
    <div className="kpi-card" style={accentColor ? { borderLeftColor: accentColor } : undefined}>
      <span className="kpi-label">{label}</span>
      {loading ? (
        <div className="kpi-skeleton" />
      ) : (
        <div className="kpi-value-row">
          <span className="kpi-value">{value}</span>
          {trend !== undefined && (
            <span className={`kpi-trend ${trend >= 0 ? "positive" : "negative"}`}>
              {trend >= 0 ? "\u25B2" : "\u25BC"} {Math.abs(trend).toFixed(1)}%
            </span>
          )}
        </div>
      )}
    </div>
  );
}
