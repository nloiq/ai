using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using NLOIQ.Application.Interfaces;
using NLOIQ.Infrastructure.Data;
namespace NLOIQ.Infrastructure;
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NloDbContext>(options => options.UseMySQL(connectionString));
        services.AddScoped<IPreviewContentReader, PreviewContentReader>();
        services.AddScoped<DevelopmentSeeder>();
        services.AddScoped<ContentArchitectureSeeder>();
        services.AddScoped<IPublishedContentReader, PublishedContentReader>();
        return services;
    }
}
