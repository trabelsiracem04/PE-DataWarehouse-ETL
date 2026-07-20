using Microsoft.AnalysisServices.AdomdClient;
using Microsoft.Extensions.Options;
using PE_DW_Web.Models;
using PE_DW_Web.Options;

namespace PE_DW_Web.Services;

public interface ISsasDashboardService
{
    Task<OverviewDto> GetOverviewAsync();
    Task<List<RevenueTrendPoint>> GetRevenueTrendAsync();
    Task<List<RevenueTrendExtendedPoint>> GetRevenueTrendExtendedAsync();
    Task<List<TerritoryRevenuePoint>> GetRevenueByTerritoryAsync();
    Task<List<ProductRevenuePoint>> GetRevenueByProductAsync(int top = 10);
    Task<List<RevenueByEmployeePoint>> GetRevenueByEmployeeAsync(int top = 10);
    Task<List<RevenueByPromotionPoint>> GetRevenueByPromotionAsync();
    Task<List<AovTrendPoint>> GetAovTrendAsync();
    Task<List<TerritoryTrendRow>> GetRevenueByTerritoryTrendAsync();
    Task<List<RevenueByCurrencyPoint>> GetRevenueByCurrencyAsync();
}

public class SsasDashboardService : ISsasDashboardService
{
    private readonly string _connectionString;
    private readonly string _cubeName;
    private readonly ILogger<SsasDashboardService> _logger;

    private const string TotalAmount = "[Measures].[Total Amount]";
    private const string OrderQuantity = "[Measures].[Order Quantity]";
    private const string FactSalesCount = "[Measures].[Fact Sales Nombre]";
    private const string DimDateLevel = "[Dim Date].[Date Name].[Date Name]";
    private const string DimDateYearLevel = "[Dim Date].[Year Name].[Year Name]";
    private const string DimTerritoryLevel = "[Dim Sales Territory].[Sales Territory Region].[Sales Territory Region]";
    private const string DimProductLevel = "[Dim Products].[Product Name].[Product Name]";
    private const string DimEmployeeLevel = "[Dim Employee].[Last Name].[Last Name]";
    private const string DimPromotionLevel = "[Dim Promotion].[Promotion Name].[Promotion Name]";
    private const string DimCurrencyLevel = "[Dim Currencies].[Currency Alternate Key].[Currency Alternate Key]";

    public SsasDashboardService(IOptions<SsasOptions> options, ILogger<SsasDashboardService> logger)
    {
        _connectionString = options.Value.BuildConnectionString();
        _cubeName = options.Value.Cube;
        _logger = logger;
    }

    public Task<OverviewDto> GetOverviewAsync()
    {
        var mdx = "SELECT {{" + TotalAmount + ", " + OrderQuantity + ", " + FactSalesCount + "}} ON COLUMNS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() =>
        {
            using var connection = new AdomdConnection(_connectionString);
            connection.Open();

            using var command = new AdomdCommand(mdx, connection);
            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                return new OverviewDto(
                    TotalRevenue: reader.GetDecimal(0),
                    TotalQuantity: reader.GetInt32(1),
                    TotalOrders: reader.GetInt32(2)
                );
            }

            return new OverviewDto(0, 0, 0);
        });
    }

    public Task<List<RevenueTrendPoint>> GetRevenueTrendAsync()
    {
        var mdx = "SELECT {" + TotalAmount + "} ON COLUMNS,"
                + " NON EMPTY {" + DimDateLevel + ".Members} ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() =>
        {
            var daily = ExecuteMemberQuery(mdx, (name, value) => (name, value));

            var yearly = daily
                .GroupBy(d =>
                {
                    var match = System.Text.RegularExpressions.Regex.Match(d.name, @"\b(20\d{2})\b");
                    return match.Success ? match.Groups[1].Value : "Unknown";
                })
                .Select(g => new RevenueTrendPoint(g.Key, g.Sum(x => x.value)))
                .OrderBy(p => p.PeriodLabel)
                .ToList();

            return yearly;
        });
    }

    public Task<List<RevenueTrendExtendedPoint>> GetRevenueTrendExtendedAsync()
    {
        var mdx = "SELECT {" + TotalAmount + ", " + OrderQuantity + ", " + FactSalesCount + "} ON COLUMNS,"
                + " NON EMPTY {" + DimDateLevel + ".Members} ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() =>
        {
            var daily = new List<(string name, decimal revenue, int quantity, int orders)>();

            using var connection = new AdomdConnection(_connectionString);
            connection.Open();

            using var command = new AdomdCommand(mdx, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var name = reader.GetValue(0)?.ToString() ?? "";
                var revenue = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                var quantity = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                var orders = reader.IsDBNull(3) ? 0 : reader.GetInt32(3);
                daily.Add((name, revenue, quantity, orders));
            }

            var yearly = daily
                .GroupBy(d =>
                {
                    var match = System.Text.RegularExpressions.Regex.Match(d.name, @"\b(20\d{2})\b");
                    return match.Success ? match.Groups[1].Value : "Unknown";
                })
                .Select(g => new RevenueTrendExtendedPoint(
                    g.Key,
                    g.Sum(x => x.revenue),
                    g.Sum(x => x.orders),
                    g.Sum(x => x.quantity)
                ))
                .OrderBy(p => p.PeriodLabel)
                .ToList();

            return yearly;
        });
    }

    public Task<List<TerritoryRevenuePoint>> GetRevenueByTerritoryAsync()
    {
        var mdx = "SELECT {" + TotalAmount + "} ON COLUMNS,"
                + " {" + DimTerritoryLevel + ".Members} ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() => ExecuteMemberQuery(mdx, (name, value) =>
            new TerritoryRevenuePoint(name, value)));
    }

    public Task<List<ProductRevenuePoint>> GetRevenueByProductAsync(int top = 10)
    {
        var mdx = "SELECT {" + TotalAmount + "} ON COLUMNS,"
                + " TOPCOUNT({" + DimProductLevel + ".Members}, " + top + ", " + TotalAmount + ") ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() => ExecuteMemberQuery(mdx, (name, value) =>
            new ProductRevenuePoint(name, value)));
    }

    public Task<List<RevenueByEmployeePoint>> GetRevenueByEmployeeAsync(int top = 10)
    {
        var mdx = "SELECT {" + TotalAmount + "} ON COLUMNS,"
                + " TOPCOUNT({" + DimEmployeeLevel + ".Members}, " + top + ", " + TotalAmount + ") ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() => ExecuteMemberQuery(mdx, (name, value) =>
            new RevenueByEmployeePoint(name, value)));
    }

    public Task<List<RevenueByPromotionPoint>> GetRevenueByPromotionAsync()
    {
        var mdx = "SELECT {" + TotalAmount + "} ON COLUMNS,"
                + " {" + DimPromotionLevel + ".Members} ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() => ExecuteMemberQuery(mdx, (name, value) =>
            new RevenueByPromotionPoint(name, value)));
    }

    public Task<List<AovTrendPoint>> GetAovTrendAsync()
    {
        var mdx = "SELECT {" + TotalAmount + ", " + FactSalesCount + "} ON COLUMNS,"
                + " NON EMPTY {" + DimDateLevel + ".Members} ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() =>
        {
            var results = new List<AovTrendPoint>();

            using var connection = new AdomdConnection(_connectionString);
            connection.Open();

            using var command = new AdomdCommand(mdx, connection);
            using var reader = command.ExecuteReader();

            var daily = new List<(string name, decimal revenue, int orders)>();

            while (reader.Read())
            {
                var name = reader.GetValue(0)?.ToString() ?? "";
                var revenue = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
                var orders = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                daily.Add((name, revenue, orders));
            }

            var yearly = daily
                .GroupBy(d =>
                {
                    var match = System.Text.RegularExpressions.Regex.Match(d.name, @"\b(20\d{2})\b");
                    return match.Success ? match.Groups[1].Value : "Unknown";
                })
                .Select(g => new AovTrendPoint(
                    g.Key,
                    g.Sum(x => x.orders) > 0
                        ? Math.Round(g.Sum(x => x.revenue) / g.Sum(x => x.orders), 2)
                        : 0
                ))
                .OrderBy(p => p.PeriodLabel)
                .ToList();

            return yearly;
        });
    }

    public Task<List<TerritoryTrendRow>> GetRevenueByTerritoryTrendAsync()
    {
        var mdx = "SELECT {" + TotalAmount + "} ON COLUMNS,"
                + " NON EMPTY CrossJoin("
                + "  {" + DimDateYearLevel + ".Members},"
                + "  {" + DimTerritoryLevel + ".Members}"
                + " ) ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() =>
        {
            var results = new List<TerritoryTrendRow>();

            using var connection = new AdomdConnection(_connectionString);
            connection.Open();

            using var command = new AdomdCommand(mdx, connection);
            using var reader = command.ExecuteReader();

            var raw = new List<(string year, string territory, decimal revenue)>();

            while (reader.Read())
            {
                var year = reader.GetValue(0)?.ToString() ?? "";
                var territory = reader.GetValue(1)?.ToString() ?? "";
                var revenue = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2);
                raw.Add((year, territory, revenue));
            }

            var years = raw.Select(r => r.year).Distinct().OrderBy(y => y).ToList();
            var territories = raw.Select(r => r.territory).Distinct().OrderBy(t => t).ToList();

            foreach (var year in years)
            {
                var dict = new Dictionary<string, decimal>();
                foreach (var t in territories)
                    dict[t] = 0;

                foreach (var r in raw.Where(r => r.year == year))
                    dict[r.territory] = r.revenue;

                results.Add(new TerritoryTrendRow(year, dict));
            }

            return results;
        });
    }

    public Task<List<RevenueByCurrencyPoint>> GetRevenueByCurrencyAsync()
    {
        var mdx = "SELECT {" + TotalAmount + "} ON COLUMNS,"
                + " NON EMPTY {" + DimCurrencyLevel + ".Members} ON ROWS"
                + " FROM [" + _cubeName + "]";

        return Task.Run(() => ExecuteMemberQuery(mdx, (name, value) =>
            new RevenueByCurrencyPoint(name, value)));
    }

    private List<T> ExecuteMemberQuery<T>(string mdx, Func<string, decimal, T> mapper)
    {
        var results = new List<T>();

        using var connection = new AdomdConnection(_connectionString);
        connection.Open();

        using var command = new AdomdCommand(mdx, connection);
        using var reader = command.ExecuteReader();

        while (reader.Read())
        {
            var memberName = reader.GetValue(0)?.ToString() ?? "";
            var value = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1);
            results.Add(mapper(memberName, value));
        }

        return results;
    }
}
