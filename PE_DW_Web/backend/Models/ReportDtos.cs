namespace PE_DW_Web.Models;

public record SalesDetailRow(
    string? SaleId,
    string? OrderDate,
    string? Product,
    string? Employee,
    string? Territory,
    string? Reseller,
    string? Promotion,
    string? Currency,
    short? Quantity,
    double? UnitPrice,
    double? TotalAmount
);

public record ProductPerformanceRow(
    string Product,
    int TotalQuantity,
    double TotalRevenue,
    double AvgPrice,
    int Rank
);

public record EmployeeSalesRow(
    string Employee,
    int TotalOrders,
    int TotalQuantity,
    double TotalRevenue,
    double AvgOrderValue,
    string? Territory
);

public record TerritorySummaryRow(
    string Territory,
    double Revenue,
    int Quantity,
    int OrderCount,
    double RevenuePercent
);

public record PromotionEffectivenessRow(
    string Promotion,
    double Revenue,
    int Quantity,
    int OrderCount,
    double AvgUnitPrice
);

public record ReportFilters(
    string? StartDate,
    string? EndDate,
    string? Territory,
    string? Product
);
