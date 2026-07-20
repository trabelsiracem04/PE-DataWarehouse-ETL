namespace PE_DW_Web.Models;

public record OverviewDto(
    decimal TotalRevenue,
    int TotalQuantity,
    int TotalOrders
);

public record RevenueTrendPoint(
    string PeriodLabel,
    decimal Revenue
);

public record TerritoryRevenuePoint(
    string Territory,
    decimal Revenue
);

public record ProductRevenuePoint(
    string ProductName,
    decimal Revenue
);

public record RevenueByEmployeePoint(
    string EmployeeName,
    decimal Revenue
);

public record RevenueByPromotionPoint(
    string PromotionName,
    decimal Revenue
);

public record RevenueTrendExtendedPoint(
    string PeriodLabel,
    decimal Revenue,
    int OrderCount,
    int Quantity
);

public record AovTrendPoint(
    string PeriodLabel,
    decimal AverageOrderValue
);

public record TerritoryTrendRow(
    string Period,
    Dictionary<string, decimal> TerritoryRevenues
);

public record RevenueByCurrencyPoint(
    string Currency,
    decimal Revenue
);
