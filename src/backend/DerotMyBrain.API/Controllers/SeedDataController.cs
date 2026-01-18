using DerotMyBrain.API.Models;
using DerotMyBrain.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers
{
    /// <summary>
    /// Controller for accessing seed data (Wikipedia categories and themes).
    /// Seed data is read-only and initialized at application startup.
    /// </summary>
    [ApiController]
    [Route("api")]
    public class SeedDataController : ControllerBase
    {
        private readonly ISeedDataService _seedDataService;
        private readonly ILogger<SeedDataController> _logger;

        public SeedDataController(ISeedDataService seedDataService, ILogger<SeedDataController> logger)
        {
            _seedDataService = seedDataService;
            _logger = logger;
        }

        /// <summary>
        /// Get all Wikipedia categories (13 official categories)
        /// </summary>
        /// <returns>List of Wikipedia categories</returns>
        [HttpGet("categories")]
        [ProducesResponseType(typeof(List<WikipediaCategory>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _seedDataService.GetCategoriesAsync();
                _logger.LogInformation("Retrieved {Count} Wikipedia categories", categories.Count);
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving Wikipedia categories");
                return StatusCode(500, new { error = "Failed to retrieve categories", message = ex.Message });
            }
        }

        /// <summary>
        /// Get all UI themes (5 available themes)
        /// </summary>
        /// <returns>List of UI themes</returns>
        [HttpGet("themes")]
        [ProducesResponseType(typeof(List<Theme>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetThemes()
        {
            try
            {
                var themes = await _seedDataService.GetThemesAsync();
                _logger.LogInformation("Retrieved {Count} UI themes", themes.Count);
                return Ok(themes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving UI themes");
                return StatusCode(500, new { error = "Failed to retrieve themes", message = ex.Message });
            }
        }
    }
}
