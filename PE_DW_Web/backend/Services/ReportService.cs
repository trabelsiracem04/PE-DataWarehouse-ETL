using Microsoft.Data.SqlClient;
using PE_DW_Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Data;

namespace PE_DW_Web.Services;

public interface IReportService
{
    Task<List<SalesDetailRow>> GetSalesDetailAsync(ReportFilters filters);
    Task<List<ProductPerformanceRow>> GetProductPerformanceAsync();
    Task<List<EmployeeSalesRow>> GetEmployeeSalesAsync(ReportFilters filters);
    Task<List<TerritorySummaryRow>> GetTerritorySummaryAsync();
    Task<List<PromotionEffectivenessRow>> GetPromotionEffectivenessAsync(ReportFilters filters);
    Task<byte[]> ExportExcelAsync(ReportFilters filters);
    Task<byte[]> ExportPdfAsync(ReportFilters filters);
    Task<(DateTime MinDate, DateTime MaxDate)> GetDateRangeAsync();
}

public class ReportService : IReportService
{
    private readonly string _connectionString;

    public ReportService(IConfiguration config)
    {
        _connectionString = config.GetConnectionString("DataWarehouse")
            ?? throw new InvalidOperationException("ConnectionStrings:DataWarehouse is not configured");
    }

    public async Task<List<SalesDetailRow>> GetSalesDetailAsync(ReportFilters filters)
    {
        var rows = new List<SalesDetailRow>();
        filters = ApplyDefaults(filters);
        var sql = @"
SELECT TOP 5000 f.SaleID, f.OrderDate, p.ProductName, 
       e.LastName + ', ' + e.FirstName AS Employee,
       t.SalesTerritoryRegion AS Territory, r.ResellerName,
       pr.PromotionName, c.CurrencyAlternateKey AS Currency,
       f.OrderQuantity, f.UnitPrice, f.TotalAmount
FROM dbo.FactSales f
LEFT JOIN dbo.DimProducts p ON f.ProductKey = p.Productkey
LEFT JOIN dbo.DimEmployee e ON f.EmployeeKey = e.EmployeeKey
LEFT JOIN dbo.DimSalesTerritory t ON f.SalesTerritoryKey = t.SalesTerritoryKey
LEFT JOIN dbo.DimResellers r ON f.ResellerKey = r.ResellerKEY
LEFT JOIN dbo.DimPromotion pr ON f.PromotionKey = pr.PromotionKey
LEFT JOIN dbo.DimCurrencies c ON f.CurrencyKey = c.CurrencyKey
WHERE 1=1
" + BuildDateFilter(filters) + BuildTerritoryFilter(filters) + BuildProductFilter(filters) + @"
ORDER BY f.OrderDate DESC";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        AddFilterParameters(cmd, filters);
        conn.Open();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new SalesDetailRow(
                reader["SaleID"] as string,
                reader["OrderDate"] is DateTime dt ? dt.ToString("yyyy-MM-dd") : null,
                reader["ProductName"] as string,
                reader["Employee"] as string,
                reader["Territory"] as string,
                reader["ResellerName"] as string,
                reader["PromotionName"] as string,
                reader["Currency"] as string,
                reader["OrderQuantity"] as short?,
                reader["UnitPrice"] as double?,
                reader["TotalAmount"] as double?
            ));
        }
        return rows;
    }

    public async Task<List<ProductPerformanceRow>> GetProductPerformanceAsync()
    {
        var rows = new List<ProductPerformanceRow>();
        var sql = @"
SELECT p.ProductName,
       ISNULL(SUM(f.OrderQuantity), 0) AS TotalQuantity,
       ISNULL(SUM(f.TotalAmount), 0) AS TotalRevenue,
       ISNULL(AVG(f.UnitPrice), 0) AS AvgPrice,
       RANK() OVER (ORDER BY ISNULL(SUM(f.TotalAmount), 0) DESC) AS Rank
FROM dbo.FactSales f
JOIN dbo.DimProducts p ON f.ProductKey = p.Productkey
GROUP BY p.ProductName
ORDER BY TotalRevenue DESC";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        conn.Open();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new ProductPerformanceRow(
                reader.GetString(0),
                Convert.ToInt32(reader.GetValue(1)),
                reader.GetDouble(2),
                reader.GetDouble(3),
                Convert.ToInt32(reader.GetValue(4))
            ));
        }
        return rows;
    }

    public async Task<List<EmployeeSalesRow>> GetEmployeeSalesAsync(ReportFilters filters)
    {
        var rows = new List<EmployeeSalesRow>();
        filters = ApplyDefaults(filters);
        var sql = @"
SELECT e.LastName + ', ' + e.FirstName AS Employee,
       COUNT(DISTINCT f.SaleID) AS TotalOrders,
       ISNULL(SUM(f.OrderQuantity), 0) AS TotalQuantity,
       ISNULL(SUM(f.TotalAmount), 0) AS TotalRevenue,
       ISNULL(SUM(f.TotalAmount), 0) / NULLIF(COUNT(DISTINCT f.SaleID), 0) AS AvgOrderValue,
       ISNULL(MIN(t.SalesTerritoryRegion), '') AS Territory
FROM dbo.FactSales f
JOIN dbo.DimEmployee e ON f.EmployeeKey = e.EmployeeKey
LEFT JOIN dbo.DimSalesTerritory t ON f.SalesTerritoryKey = t.SalesTerritoryKey
WHERE 1=1
" + BuildDateFilter(filters) + @"
GROUP BY e.LastName, e.FirstName, f.EmployeeKey
ORDER BY TotalRevenue DESC";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        AddFilterParameters(cmd, filters);
        conn.Open();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new EmployeeSalesRow(
                reader.GetString(0),
                Convert.ToInt32(reader.GetValue(1)),
                Convert.ToInt32(reader.GetValue(2)),
                reader.GetDouble(3),
                reader.GetDouble(4),
                reader["Territory"] as string
            ));
        }
        return rows;
    }

    public async Task<List<TerritorySummaryRow>> GetTerritorySummaryAsync()
    {
        var rows = new List<TerritorySummaryRow>();
        var sql = @"
SELECT t.SalesTerritoryRegion AS Territory,
       ISNULL(SUM(f.TotalAmount), 0) AS Revenue,
       ISNULL(SUM(f.OrderQuantity), 0) AS Quantity,
       COUNT(DISTINCT f.SaleID) AS OrderCount,
       ISNULL(SUM(f.TotalAmount), 0) * 100.0 / NULLIF(SUM(SUM(f.TotalAmount)) OVER (), 0) AS RevenuePercent
FROM dbo.FactSales f
JOIN dbo.DimSalesTerritory t ON f.SalesTerritoryKey = t.SalesTerritoryKey
GROUP BY t.SalesTerritoryRegion
ORDER BY Revenue DESC";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        conn.Open();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new TerritorySummaryRow(
                reader.GetString(0),
                reader.GetDouble(1),
                Convert.ToInt32(reader.GetValue(2)),
                Convert.ToInt32(reader.GetValue(3)),
                reader.GetDouble(4)
            ));
        }
        return rows;
    }

    public async Task<List<PromotionEffectivenessRow>> GetPromotionEffectivenessAsync(ReportFilters filters)
    {
        var rows = new List<PromotionEffectivenessRow>();
        filters = ApplyDefaults(filters);
        var sql = @"
SELECT pr.PromotionName,
       ISNULL(SUM(f.TotalAmount), 0) AS Revenue,
       ISNULL(SUM(f.OrderQuantity), 0) AS Quantity,
       COUNT(DISTINCT f.SaleID) AS OrderCount,
       ISNULL(AVG(f.UnitPrice), 0) AS AvgUnitPrice
FROM dbo.FactSales f
JOIN dbo.DimPromotion pr ON f.PromotionKey = pr.PromotionKey
WHERE 1=1
" + BuildDateFilter(filters) + @"
GROUP BY pr.PromotionName
ORDER BY Revenue DESC";

        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand(sql, conn) { CommandTimeout = 120 };
        AddFilterParameters(cmd, filters);
        conn.Open();
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            rows.Add(new PromotionEffectivenessRow(
                reader.GetString(0),
                reader.GetDouble(1),
                Convert.ToInt32(reader.GetValue(2)),
                Convert.ToInt32(reader.GetValue(3)),
                reader.GetDouble(4)
            ));
        }
        return rows;
    }

    public async Task<byte[]> ExportExcelAsync(ReportFilters filters)
    {
        var data = await GetSalesDetailAsync(filters);
        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var ws = workbook.Worksheets.Add("Sales Detail");

        ws.Cell(1, 1).Value = "Sale ID";
        ws.Cell(1, 2).Value = "Order Date";
        ws.Cell(1, 3).Value = "Product";
        ws.Cell(1, 4).Value = "Employee";
        ws.Cell(1, 5).Value = "Territory";
        ws.Cell(1, 6).Value = "Reseller";
        ws.Cell(1, 7).Value = "Promotion";
        ws.Cell(1, 8).Value = "Currency";
        ws.Cell(1, 9).Value = "Quantity";
        ws.Cell(1, 10).Value = "Unit Price";
        ws.Cell(1, 11).Value = "Total Amount";
        ws.Row(1).Style.Font.Bold = true;

        for (int i = 0; i < data.Count; i++)
        {
            var r = data[i];
            ws.Cell(i + 2, 1).Value = r.SaleId;
            ws.Cell(i + 2, 2).Value = r.OrderDate;
            ws.Cell(i + 2, 3).Value = r.Product;
            ws.Cell(i + 2, 4).Value = r.Employee;
            ws.Cell(i + 2, 5).Value = r.Territory;
            ws.Cell(i + 2, 6).Value = r.Reseller;
            ws.Cell(i + 2, 7).Value = r.Promotion;
            ws.Cell(i + 2, 8).Value = r.Currency;
            ws.Cell(i + 2, 9).Value = r.Quantity;
            ws.Cell(i + 2, 10).Value = r.UnitPrice;
            ws.Cell(i + 2, 11).Value = r.TotalAmount;
        }

        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return ms.ToArray();
    }

    public async Task<byte[]> ExportPdfAsync(ReportFilters filters)
    {
        var data = await GetSalesDetailAsync(filters);

        QuestPDF.Settings.License = LicenseType.Community;

        var doc = QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);
                page.DefaultTextStyle(x => x.FontSize(9));

                page.Header().AlignCenter().Text("Sales Detail Report").Bold().FontSize(16);

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1.8f);
                        c.RelativeColumn(1.5f);
                        c.RelativeColumn(1.2f);
                        c.RelativeColumn(1);
                        c.RelativeColumn(1);
                    });

                    table.Header(h =>
                    {
                        h.Cell().Background("#2563EB").Padding(4).Text("Sale ID").FontColor("#fff").Bold();
                        h.Cell().Background("#2563EB").Padding(4).Text("Date").FontColor("#fff").Bold();
                        h.Cell().Background("#2563EB").Padding(4).Text("Product").FontColor("#fff").Bold();
                        h.Cell().Background("#2563EB").Padding(4).Text("Employee").FontColor("#fff").Bold();
                        h.Cell().Background("#2563EB").Padding(4).Text("Territory").FontColor("#fff").Bold();
                        h.Cell().Background("#2563EB").Padding(4).Text("Qty").FontColor("#fff").Bold().AlignRight();
                        h.Cell().Background("#2563EB").Padding(4).Text("Total").FontColor("#fff").Bold().AlignRight();
                    });

                    foreach (var r in data)
                    {
                        table.Cell().Padding(3).Text(r.SaleId ?? "");
                        table.Cell().Padding(3).Text(r.OrderDate ?? "");
                        table.Cell().Padding(3).Text(r.Product ?? "");
                        table.Cell().Padding(3).Text(r.Employee ?? "");
                        table.Cell().Padding(3).Text(r.Territory ?? "");
                        table.Cell().Padding(3).AlignRight().Text(r.Quantity?.ToString() ?? "");
                        table.Cell().Padding(3).AlignRight().Text(r.TotalAmount?.ToString("N2") ?? "");
                    }
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Page ");
                    x.CurrentPageNumber();
                });
            });
        });

        return doc.GeneratePdf();
    }

    public async Task<(DateTime MinDate, DateTime MaxDate)> GetDateRangeAsync()
    {
        using var conn = new SqlConnection(_connectionString);
        using var cmd = new SqlCommand("SELECT MIN(OrderDate), MAX(OrderDate) FROM dbo.FactSales", conn) { CommandTimeout = 30 };
        conn.Open();
        using var reader = await cmd.ExecuteReaderAsync();
        await reader.ReadAsync();
        return (reader.GetDateTime(0), reader.GetDateTime(1));
    }

    private static ReportFilters ApplyDefaults(ReportFilters f) => f;

    private static string BuildDateFilter(ReportFilters f)
    {
        var clauses = "";
        if (!string.IsNullOrEmpty(f.StartDate)) clauses += "\n  AND f.OrderDate >= @StartDate";
        if (!string.IsNullOrEmpty(f.EndDate)) clauses += "\n  AND f.OrderDate <= @EndDate";
        return clauses;
    }

    private static string BuildTerritoryFilter(ReportFilters f)
    {
        if (string.IsNullOrEmpty(f.Territory) || f.Territory == "All") return "";
        return "\n  AND t.SalesTerritoryRegion = @Territory";
    }

    private static string BuildProductFilter(ReportFilters f)
    {
        if (string.IsNullOrEmpty(f.Product) || f.Product == "All") return "";
        return "\n  AND p.ProductName = @Product";
    }

    private static void AddFilterParameters(SqlCommand cmd, ReportFilters f)
    {
        if (!string.IsNullOrEmpty(f.StartDate)) cmd.Parameters.AddWithValue("@StartDate", DateTime.Parse(f.StartDate));
        if (!string.IsNullOrEmpty(f.EndDate)) cmd.Parameters.AddWithValue("@EndDate", DateTime.Parse(f.EndDate));
        if (!string.IsNullOrEmpty(f.Territory) && f.Territory != "All") cmd.Parameters.AddWithValue("@Territory", f.Territory);
        if (!string.IsNullOrEmpty(f.Product) && f.Product != "All") cmd.Parameters.AddWithValue("@Product", f.Product);
    }
}
