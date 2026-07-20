namespace PE_DW_Web.Options;

public class SsasOptions
{
    public const string SectionName = "Ssas";

    public string Server { get; set; } = string.Empty;
    public string Catalog { get; set; } = string.Empty;
    public string Cube { get; set; } = string.Empty;

    public string BuildConnectionString()
    {
        return $"Data Source={Server};Initial Catalog={Catalog}";
    }
}
