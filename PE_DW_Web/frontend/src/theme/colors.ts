export const colors = {
  primary: "#2563EB",
  secondary: "#60A5FA",
  accent: "#0EA5E9",
  success: "#10B981",
  warning: "#F59E0B",
  error: "#EF4444",
  text: {
    primary: "#111827",
    secondary: "#6B7280",
  },
  border: "#E5E7EB",
  background: "#F8FAFC",
  cardBackground: "#FFFFFF",
};

export function barShade(i: number, total: number, base = colors.primary): string {
  if (total <= 1) return base;
  const lightness = 92 - (i / (total - 1)) * 32;
  return `${base}${Math.round(lightness * 2.55).toString(16).padStart(2, "0")}`;
}

export function categoryColor(category: string): string {
  let hash = 0;
  for (let i = 0; i < category.length; i++)
    hash = category.charCodeAt(i) + ((hash << 5) - hash);
  const catPalette = [
    "#2563EB", "#60A5FA", "#0EA5E9", "#10B981",
    "#8B5CF6", "#F59E0B", "#EC4899", "#06B6D4",
  ];
  return catPalette[Math.abs(hash) % catPalette.length];
}
