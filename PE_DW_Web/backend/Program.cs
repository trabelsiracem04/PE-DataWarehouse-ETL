using PE_DW_Web.Options;
using PE_DW_Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<SsasOptions>(
    builder.Configuration.GetSection(SsasOptions.SectionName));

builder.Services.AddSingleton<ISsasDashboardService, SsasDashboardService>();
builder.Services.AddScoped<IReportService, ReportService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowFrontend");

app.MapControllers();

app.Run();
