using DerotMyBrain.API.Services;

namespace DerotMyBrain.API.Services
{
    /// <summary>
    /// Service responsible for orchestrating application initialization.
    /// Coordinates seed data and configuration initialization at startup.
    /// </summary>
    public class InitializationService : IInitializationService
    {
        private readonly ISeedDataService _seedDataService;
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<InitializationService> _logger;

        public InitializationService(
            ISeedDataService seedDataService,
            IConfigurationService configurationService,
            ILogger<InitializationService> logger)
        {
            _seedDataService = seedDataService;
            _configurationService = configurationService;
            _logger = logger;
        }

        /// <summary>
        /// Initialize the application by setting up seed data and configuration.
        /// This method is called at application startup.
        /// </summary>
        public async Task InitializeAsync()
        {
            _logger.LogInformation("=== Application Initialization Started ===");

            try
            {
                // Initialize seed data (categories and themes)
                _logger.LogInformation("Step 1/2: Initializing seed data...");
                await _seedDataService.InitializeAsync();
                _logger.LogInformation("Step 1/2: Seed data initialization completed");

                // Initialize global configuration
                _logger.LogInformation("Step 2/2: Initializing global configuration...");
                await _configurationService.InitializeAsync();
                _logger.LogInformation("Step 2/2: Configuration initialization completed");

                _logger.LogInformation("=== Application Initialization Completed Successfully ===");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Application initialization failed: {Message}", ex.Message);
                throw new InvalidOperationException("Failed to initialize application. See logs for details.", ex);
            }
        }
    }
}
