using APIContagem;
using APIContagem.Data;
using APIContagem.Tracing;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ContagemSqlServerContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BaseContagemSqlServer"));
});

builder.Services.AddDbContext<ContagemPostgresContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("BaseContagemPostgres"),
        o => o.UseNodaTime());
});

builder.Services.AddDbContext<ContagemMySqlContext>(options =>
{
    options.UseMySQL(
        builder.Configuration.GetConnectionString("BaseContagemMySql")!);
});

var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: OpenTelemetryExtensions.ServiceName,
        serviceVersion: OpenTelemetryExtensions.ServiceVersion);
builder.Services.AddOpenTelemetry()
    .WithTracing((traceBuilder) =>
    {
        traceBuilder
            .AddSource(OpenTelemetryExtensions.ServiceName)
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(cfg => // Valido para uso com SQL Server
            {     
                cfg.SetDbStatementForText = true;
            })
            .AddOtlpExporter(cfg =>
            {
                cfg.Endpoint = new Uri(builder.Configuration["OtlpExporter:Endpoint"]!);
            })
            .AddConsoleExporter();
    });

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddScoped<ContagemRepository>();
builder.Services.AddScoped<ContagemSecundariaRepository>();
builder.Services.AddScoped<ContagemRegressivaRepository>();
builder.Services.AddSingleton<Contador>();

builder.Services.AddCors();

var app = builder.Build();

app.MapOpenApi();

app.UseAuthorization();

app.MapControllers();

app.Run();