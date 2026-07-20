import chroma from "chroma-js";

export const brand = {
  revenue: "#2563EB",
  orders: "#F59E0B",
  quantity: "#10B981",
  employees: "#7C3AED",
};

export const sequentialScale = (base: string, count: number) =>
  chroma.scale([chroma(base).brighten(1.5), chroma(base).darken(1)])
    .mode("lch")
    .colors(count);

export const categoricalScale = (count: number) =>
  chroma.scale(["#2563EB", "#10B981", "#F59E0B", "#EF4444", "#8B5CF6", "#06B6D4"])
    .mode("lch")
    .colors(count);
