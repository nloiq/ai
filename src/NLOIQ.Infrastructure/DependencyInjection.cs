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
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<NLOIQ.Application.Content.IContentWorkflow, ContentWorkflow>();
        services.AddScoped<NLOIQ.Application.Content.IContentEditor, ContentEditor>();
        services.AddScoped<PublicSiteReader>();
        services.AddScoped<NLOIQ.Application.Content.IPublicSiteReader>(provider=>provider.GetRequiredService<PublicSiteReader>());
        services.AddScoped<NLOIQ.Application.Content.IContentPreviewReader>(provider=>provider.GetRequiredService<PublicSiteReader>());
        services.AddSingleton<NLOIQ.Application.Content.IImageOptimizer, NLOIQ.Infrastructure.Media.ImageOptimizer>();
        services.AddScoped<NLOIQ.Application.Content.IMediaLibrary, NLOIQ.Infrastructure.Media.MediaLibrary>();
        services.AddScoped<NLOIQ.Application.Content.IBrandReader, BrandReader>();
        return services;
    }
}
