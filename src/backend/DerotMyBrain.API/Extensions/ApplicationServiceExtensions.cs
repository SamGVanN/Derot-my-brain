using DerotMyBrain.Core.Interfaces.Services;
using DerotMyBrain.Core.Interfaces.Utils;
using DerotMyBrain.Core.Services;
using DerotMyBrain.Infrastructure.Services;
using DerotMyBrain.Infrastructure.Utils;

namespace DerotMyBrain.API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config)
    {
        // 3. Domain Services (Core)
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IActivityService, ActivityService>();
        services.AddScoped<IWikipediaService, WikipediaService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<ISourceService, SourceService>();
        services.AddScoped<IBacklogService, BacklogService>();
        services.AddScoped<ISessionService, SessionService>();
        services.AddScoped<IQuizService, QuizService>();

        // Infrastructure Utilities
        services.AddSingleton<IJsonSerializer, JsonSerializerWrapper>();
        
        // Content Extraction Queue and Background Service
        services.AddSingleton<IContentExtractionQueue, ContentExtractionQueue>();
        services.AddHostedService<ContentExtractionService>();

        // Legacy / Helper Services (Scoped because they depend on Scoped services like IConfigurationService)
        services.AddScoped<ISeedDataService, SeedDataService>();
        services.AddScoped<IInitializationService, InitializationService>();

        return services;
    }
}
