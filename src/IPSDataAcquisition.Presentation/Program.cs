using IPSDataAcquisition.Infrastructure;
using IPSDataAcquisition.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IPS Data Acquisition API",
        Version = "v1",
        Description = "Backend API for IPS Indoor Positioning Data Collection Mobile App"
    });
});

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(IPSDataAcquisition.Application.AssemblyMarker).Assembly);

// Infrastructure (Database, Services)
builder.Services.AddInfrastructure(builder.Configuration);

// MediatR - scan Application assembly
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(IPSDataAcquisition.Application.AssemblyMarker).Assembly);
});

// Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Run DB migrations on startup (auto-creates database and tables)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Option 1: Apply migrations (requires migration files)
    db.Database.Migrate();
    
    // Option 2 (Alternative): Create database if doesn't exist (no migration files needed)
    // db.Database.EnsureCreated();
    
    // Note: Use Migrate() for production (tracks schema changes)
    // Use EnsureCreated() only for quick testing (doesn't use migrations)
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("Swagger:Enabled"))
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "IPS Data Acquisition API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors();
app.UseIpRateLimiting();

app.UseAuthorization();

app.MapControllers();

app.Run();

