using DerotMyBrain.Core.Entities;
using DerotMyBrain.API.Services; // Keep for IConfigurationService if it's still in API
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers
{
    /// <summary>
    /// Controller for managing global application configuration.
    /// Configuration is shared across all users.
    /// </summary>
    [ApiController]
    [Route("api/global-config")]
    public class GlobalConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<GlobalConfigurationController> _logger;

        public GlobalConfigurationController(IConfigurationService configurationService, ILogger<GlobalConfigurationController> logger)
        {
            _configurationService = configurationService;
            _logger = logger;
        }

        /// <summary>
        /// Get the global application configuration
        /// </summary>
        /// <returns>Application configuration</returns>
        [HttpGet]
        [ProducesResponseType(typeof(AppConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetConfiguration()
        {
            try
            {
                var config = await _configurationService.GetConfigurationAsync();
                _logger.LogInformation("Retrieved global configuration");
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving configuration");
                return StatusCode(500, new { error = "Failed to retrieve configuration", message = ex.Message });
            }
        }

        /// <summary>
        /// Update the global application configuration
        /// </summary>
        /// <param name="config">Updated configuration</param>
        /// <returns>Updated configuration</returns>
        [HttpPut]
        [ProducesResponseType(typeof(AppConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateConfiguration([FromBody] AppConfiguration config)
        {
            try
            {
                if (config == null)
                {
                    return BadRequest(new { error = "Configuration cannot be null" });
                }

                var updatedConfig = await _configurationService.UpdateConfigurationAsync(config);
                _logger.LogInformation("Updated global configuration");
                return Ok(updatedConfig);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid configuration data");
                return BadRequest(new { error = "Invalid configuration", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating configuration");
                return StatusCode(500, new { error = "Failed to update configuration", message = ex.Message });
            }
        }

        /// <summary>
        /// Get LLM configuration only
        /// </summary>
        /// <returns>LLM configuration</returns>
        [HttpGet("llm")]
        [ProducesResponseType(typeof(LLMConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetLLMConfiguration()
        {
            try
            {
                var llmConfig = await _configurationService.GetLLMConfigurationAsync();
                _logger.LogInformation("Retrieved LLM configuration");
                return Ok(llmConfig);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving LLM configuration");
                return StatusCode(500, new { error = "Failed to retrieve LLM configuration", message = ex.Message });
            }
        }

        /// <summary>
        /// Update LLM configuration only
        /// </summary>
        /// <param name="llmConfig">Updated LLM configuration</param>
        /// <returns>Updated LLM configuration</returns>
        [HttpPut("llm")]
        [ProducesResponseType(typeof(LLMConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateLLMConfiguration([FromBody] LLMConfiguration llmConfig)
        {
            try
            {
                if (llmConfig == null)
                {
                    return BadRequest(new { error = "LLM configuration cannot be null" });
                }

                var updatedConfig = await _configurationService.UpdateLLMConfigurationAsync(llmConfig);
                _logger.LogInformation("Updated LLM configuration");
                return Ok(updatedConfig);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid LLM configuration data");
                return BadRequest(new { error = "Invalid LLM configuration", message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating LLM configuration");
                return StatusCode(500, new { error = "Failed to update LLM configuration", message = ex.Message });
            }
        }
        /// <summary>
        /// Test the connection to the LLM server
        /// </summary>
        /// <param name="llmConfig">Optional configuration to test. If null, uses current global config.</param>
        /// <returns>Success status and message</returns>
        [HttpPost("llm/test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> TestLLMConnection([FromBody] LLMConfiguration? llmConfig)
        {
            try
            {
                // If config not provided in body, use current global config
                if (llmConfig == null)
                {
                    llmConfig = await _configurationService.GetLLMConfigurationAsync();
                }

                _logger.LogInformation("Testing LLM connection...");
                var success = await _configurationService.TestLLMConnectionAsync(llmConfig);

                if (success)
                {
                    return Ok(new { success = true, message = "Successfully connected to LLM" });
                }
                else
                {
                    return Ok(new { success = false, message = "Failed to connect to LLM. Please check URL and Port." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error testing LLM connection");
                return StatusCode(500, new { success = false, error = "Internal error during connection test", message = ex.Message });
            }
        }
    }
}
