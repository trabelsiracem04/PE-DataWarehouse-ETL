using Microsoft.AspNetCore.Mvc;
using PE_DW_Web.Models;
using PE_DW_Web.Services;

namespace PE_DW_Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _report;

    public ReportsController(IReportService report) => _report = report;

    [HttpPost("sales-detail")]
    public async Task<IActionResult> SalesDetail([FromBody] ReportFilters filters) =>
        Ok(await _report.GetSalesDetailAsync(filters));

    [HttpPost("product-performance")]
    public async Task<IActionResult> ProductPerformance() =>
        Ok(await _report.GetProductPerformanceAsync());

    [HttpPost("employee-sales")]
    public async Task<IActionResult> EmployeeSales([FromBody] ReportFilters filters) =>
        Ok(await _report.GetEmployeeSalesAsync(filters));

    [HttpPost("territory-summary")]
    public async Task<IActionResult> TerritorySummary() =>
        Ok(await _report.GetTerritorySummaryAsync());

    [HttpPost("promotion-effectiveness")]
    public async Task<IActionResult> PromotionEffectiveness([FromBody] ReportFilters filters) =>
        Ok(await _report.GetPromotionEffectivenessAsync(filters));

    [HttpPost("export-excel")]
    public async Task<IActionResult> ExportExcel([FromBody] ReportFilters filters)
    {
        var bytes = await _report.ExportExcelAsync(filters);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SalesDetail.xlsx");
    }

    [HttpPost("export-pdf")]
    public async Task<IActionResult> ExportPdf([FromBody] ReportFilters filters)
    {
        var bytes = await _report.ExportPdfAsync(filters);
        return File(bytes, "application/pdf", "SalesDetail.pdf");
    }

    [HttpGet("date-range")]
    public async Task<IActionResult> DateRange()
    {
        var (min, max) = await _report.GetDateRangeAsync();
        return Ok(new { minDate = min.ToString("yyyy-MM-dd"), maxDate = max.ToString("yyyy-MM-dd") });
    }
}
