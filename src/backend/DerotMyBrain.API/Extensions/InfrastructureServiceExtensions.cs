using DerotMyBrain.Core.Interfaces.Clients;
using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Infrastructure.Clients;
using DerotMyBrain.Infrastructure.Services;

namespace DerotMyBrain.API.Extensions;

public static class InfrastructureServiceExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
    {
        // 2. External Services (Infrastructure)
        services.AddHttpClient();
        services.AddScoped<IWikipediaClient, WikipediaClient>();
        
        // Note: IAuthService implementation is in Core (AuthService), but it's infrastructure-like. 
        // We'll keep it here or in Identity. Let's put it in Identity extensions as it relates to Auth.
        
        services.AddScoped<ICategoryService, CategoryService>();

        // Register Content Sources as a collection
        services.AddScoped<IContentSource, WikipediaContentSource>();
        services.AddScoped<IContentSource, FileContentSource>();

        // Register LLM Service
        services.AddScoped<ILlmService, OllamaLlmService>();

        // Register Utilities
        services.AddScoped<ITextExtractor, TextExtractor>();
        
        // Register File Storage
        services.AddScoped<IFileStorageService, FileSystemStorageService>();

        // Register Global Configuration Service (Singleton for thread-safe file access)
        services.AddSingleton<IConfigurationService, ConfigurationService>();

        return services;
    }
}
