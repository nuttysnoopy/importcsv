using CsvImportApp.Data;
using CsvImportApp.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Microsoft.OpenApi.Models;
using ProductImportApp.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

builder.Host
    .UseSerilog();

// Add DbContext
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<CsvImportDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add Services
builder.Services.AddScoped<ICsvImportService, CsvImportService>();
builder.Services.AddScoped<IProductImportService, ProductImportService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "CsvImport API", Version = "v1" });
});

var app = builder.Build();

// Migrate database on startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CsvImportDbContext>();
    dbContext.Database.Migrate();
}

// Always enable Swagger/UI so it's reachable in Docker
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "CsvImport API v1");
    c.RoutePrefix = "swagger"; // serve at /swagger
});

// Only use HTTPS redirection in local Development (avoids cert issues in Docker)
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
