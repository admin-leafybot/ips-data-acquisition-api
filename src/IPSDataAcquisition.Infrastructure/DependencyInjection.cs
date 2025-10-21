using IPSDataAcquisition.Application.Common.Interfaces;
using IPSDataAcquisition.Infrastructure.Data;
using IPSDataAcquisition.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace IPSDataAcquisition.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("Default")
            ?? "Host=localhost;Port=5432;Database=ips_data_acquisition;Username=postgres;Password=postgres";
        
        services.AddDbContext<ApplicationDbContext>(options => options
            .UseNpgsql(connectionString)
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        
        // Services
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}

