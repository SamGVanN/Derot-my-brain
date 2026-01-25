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
        services.AddScoped<IUserFocusService, UserFocusService>();
        services.AddScoped<IWikipediaService, WikipediaService>();

        // Infrastructure Utilities
        services.AddSingleton<IJsonSerializer, JsonSerializerWrapper>();

        // Legacy / Helper Services
        services.AddSingleton<ISeedDataService, SeedDataService>();
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<IInitializationService, InitializationService>();

        return services;
    }
}
