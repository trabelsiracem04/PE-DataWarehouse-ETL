using Microsoft.AspNetCore.Mvc;
using Microsoft.AnalysisServices;
using PE_DW_Web.Options;
using PE_DW_Web.Services;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;

namespace PE_DW_Web.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly ISsasDashboardService _ssasService;
    private readonly ILogger<DashboardController> _logger;
    private readonly SsasOptions _ssasOptions;

    public DashboardController(ISsasDashboardService ssasService, ILogger<DashboardController> logger, IOptions<SsasOptions> ssasOptions)
    {
        _ssasService = ssasService;
        _logger = logger;
        _ssasOptions = ssasOptions.Value;
    }

    [HttpPost("process-cube")]
    public IActionResult ProcessCube()
    {
        try
        {
            using var server = new Server();
            server.Connect($"Data Source={_ssasOptions.Server}");

            var db = server.Databases.FindByName(_ssasOptions.Catalog);
            if (db == null)
                return NotFound(new { error = $"Database '{_ssasOptions.Catalog}' not found" });

            var warnings = new List<string>();

            var ds = db.DataSources.FindByName("My Data Warehouse");
            if (ds != null)
            {
                ds.ConnectionString = "Provider=MSOLEDBSQL.1;Data Source=pc-de-tipon;Integrated Security=SSPI;Initial Catalog=MyDataWarehouse";
                ds.ImpersonationInfo = new ImpersonationInfo(ImpersonationMode.ImpersonateServiceAccount);
                ds.Update();
            }

            foreach (Dimension dim in db.Dimensions)
            {
                try { dim.Process(ProcessType.ProcessFull); }
                catch (Exception ex) { warnings.Add($"Dimension {dim.Name}: {ex.Message}"); }
            }

            var cube = db.Cubes.FindByName(_ssasOptions.Cube);
            if (cube != null)
            {
                try { cube.Process(ProcessType.ProcessFull); }
                catch (Exception ex) { warnings.Add($"Cube processing: {ex.Message}"); }
            }

            return Ok(new { message = $"Database '{_ssasOptions.Catalog}' processing completed", warnings });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process SSAS database");
            return StatusCode(500, new { error = "Failed to process database", detail = ex.Message });
        }
    }

    [HttpPost("deploy-cube")]
    public IActionResult DeployCube()
    {
        try
        {
            var cubePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "PE_DW_BI", "MyCube.cube");
            if (!System.IO.File.Exists(cubePath))
                return NotFound(new { error = "Cube file not found", path = cubePath });

            var xml = System.IO.File.ReadAllText(cubePath);

            // Remove design-time attributes and namespaces (dwd:*)
            xml = Regex.Replace(xml, @"\s+dwd:[^=]+="".*?""", "");
            xml = Regex.Replace(xml, @"\s+xmlns:dwd[^=]+="".*?""", "");

            // Remove design-time only elements
            xml = Regex.Replace(xml, @"<CreatedTimestamp>[^<]*</CreatedTimestamp>", "");
            xml = Regex.Replace(xml, @"<LastSchemaUpdate>[^<]*</LastSchemaUpdate>", "");
            xml = Regex.Replace(xml, @"<LastProcessed>[^<]*</LastProcessed>", "");
            xml = Regex.Replace(xml, @"<State>[^<]*</State>", "");

            // Remove extra date attributes that cause duplicate key errors
            xml = Regex.Replace(xml, @"<Attribute>\s*<AttributeID>Quarter Name</AttributeID>\s*</Attribute>", "");
            xml = Regex.Replace(xml, @"<Attribute>\s*<AttributeID>Month Name</AttributeID>\s*</Attribute>", "");
            xml = Regex.Replace(xml, @"<Attribute>\s*<AttributeID>Week</AttributeID>\s*</Attribute>", "");
            xml = Regex.Replace(xml, @"<Attribute>\s*<AttributeID>Week Name</AttributeID>\s*</Attribute>", "");
            xml = Regex.Replace(xml, @"<Attribute>\s*<AttributeID>Year Name</AttributeID>\s*</Attribute>", "");
            // Remove empty hierarchy that references unneeded attributes
            xml = Regex.Replace(xml, @"<Hierarchies>\s*<Hierarchy[^>]*>\s*<HierarchyID>[^<]*</HierarchyID>\s*</Hierarchy>\s*</Hierarchies>", "");

            // Strip Dim Date entirely from cube dimensions
            xml = Regex.Replace(xml,
                @"<Dimension[^>]*>\s*<ID>Dim Date</ID>.*?</Dimension>",
                "",
                RegexOptions.Singleline);

            // Strip Dim Date reference from Fact Sales measure group dimensions
            xml = Regex.Replace(xml,
                @"<Dimension[^>]*>\s*<CubeDimensionID>Dim Date</CubeDimensionID>.*?</Dimension>",
                "",
                RegexOptions.Singleline);

            // Merge partitions from .partitions file
            var partitionsPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "PE_DW_BI", "MyCube.partitions");
            if (System.IO.File.Exists(partitionsPath))
            {
                var partitionsXml = System.IO.File.ReadAllText(partitionsPath);
                partitionsXml = Regex.Replace(partitionsXml, @"\s+dwd:[^=]+="".*?""", "");
                partitionsXml = Regex.Replace(partitionsXml, @"\s+xmlns:dwd[^=]+="".*?""", "");
                // Extract <Partitions> from the partitions file
                var partMatch = Regex.Match(partitionsXml, @"<Partitions>.*?</Partitions>", RegexOptions.Singleline);
                if (partMatch.Success)
                    xml = Regex.Replace(xml, @"<Partitions\s*/>", partMatch.Value);
            }

            using var server = new Server();
            server.Connect($"Data Source={_ssasOptions.Server}");

            var db = server.Databases.FindByName(_ssasOptions.Catalog);

            var ds = db.DataSources.FindByName("My Data Warehouse");
            if (ds != null)
            {
                ds.ConnectionString = "Provider=MSOLEDBSQL.1;Data Source=pc-de-tipon;Integrated Security=SSPI;Initial Catalog=MyDataWarehouse";
                ds.ImpersonationInfo = new ImpersonationInfo(ImpersonationMode.ImpersonateServiceAccount);
                ds.Update();
            }

            // Use Create with AllowOverwrite to deploy the cube
            var cubeXml = $@"<Create AllowOverwrite=""true"" xmlns=""http://schemas.microsoft.com/analysisservices/2003/engine"">
  <ParentObject>
    <DatabaseID>{_ssasOptions.Catalog}</DatabaseID>
  </ParentObject>
  <ObjectDefinition>
    {xml}
  </ObjectDefinition>
</Create>";

            try { server.Execute(cubeXml); }
            catch (Exception createEx)
            {
                _logger.LogError(createEx, "Create failed. XML length: {Len}", cubeXml.Length);
                return StatusCode(500, new { error = "Create command failed", detail = createEx.Message, xmlLength = cubeXml.Length });
            }

            // Verify cube was created
            db.Refresh();
            var cube = db.Cubes.FindByName(_ssasOptions.Cube);
            if (cube == null)
            {
                var allCubes = db.Cubes.OfType<Cube>().Select(c => $"{c.Name} ({c.ID})").ToList();
                return StatusCode(500, new { error = "Cube not found after deploy", cubes = allCubes, xmlLength = cubeXml.Length });
            }

            cube.Process(ProcessType.ProcessDefault);

            return Ok(new { message = "Cube deployed and processed successfully" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { error = ex.Message, detail = ex.ToString() });
        }
    }

    [HttpGet("overview")]
    public async Task<IActionResult> GetOverview()
    {
        try
        {
            var result = await _ssasService.GetOverviewAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch overview from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve data from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-trend")]
    public async Task<IActionResult> GetRevenueTrend()
    {
        try
        {
            var result = await _ssasService.GetRevenueTrendAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch revenue trend from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve revenue trend from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-trend-extended")]
    public async Task<IActionResult> GetRevenueTrendExtended()
    {
        try
        {
            var result = await _ssasService.GetRevenueTrendExtendedAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch revenue trend from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve revenue trend from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-by-territory")]
    public async Task<IActionResult> GetRevenueByTerritory()
    {
        try
        {
            var result = await _ssasService.GetRevenueByTerritoryAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch territory revenue from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve territory data from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-by-product")]
    public async Task<IActionResult> GetRevenueByProduct([FromQuery] int top = 10)
    {
        try
        {
            var result = await _ssasService.GetRevenueByProductAsync(top);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch product revenue from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve product data from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-by-employee")]
    public async Task<IActionResult> GetRevenueByEmployee([FromQuery] int top = 10)
    {
        try
        {
            var result = await _ssasService.GetRevenueByEmployeeAsync(top);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch employee revenue from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve employee data from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-by-promotion")]
    public async Task<IActionResult> GetRevenueByPromotion()
    {
        try
        {
            var result = await _ssasService.GetRevenueByPromotionAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch promotion revenue from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve promotion data from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("aov-trend")]
    public async Task<IActionResult> GetAovTrend()
    {
        try
        {
            var result = await _ssasService.GetAovTrendAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch AOV trend from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve AOV data from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-by-territory-trend")]
    public async Task<IActionResult> GetRevenueByTerritoryTrend()
    {
        try
        {
            var result = await _ssasService.GetRevenueByTerritoryTrendAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch territory trend from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve territory trend data from SSAS cube.", detail = ex.ToString() });
        }
    }

    [HttpGet("revenue-by-currency")]
    public async Task<IActionResult> GetRevenueByCurrency()
    {
        try
        {
            var result = await _ssasService.GetRevenueByCurrencyAsync();
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch currency revenue from SSAS");
            return StatusCode(503, new { error = "Unable to retrieve currency data from SSAS cube.", detail = ex.ToString() });
        }
    }
}
