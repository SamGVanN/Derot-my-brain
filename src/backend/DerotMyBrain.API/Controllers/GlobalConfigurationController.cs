using DerotMyBrain.Core.Entities;
using DerotMyBrain.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using DerotMyBrain.API;

namespace DerotMyBrain.API.Controllers
{
    /// <summary>
    /// Controller for managing global application configuration.
    /// Configuration is shared across all users.
    /// </summary>
    [ApiController]
    [Route("api/global-config")]
    [Authorize]
    public class GlobalConfigurationController : ControllerBase
    {
        private readonly IConfigurationService _configurationService;
        private readonly ILogger<GlobalConfigurationController> _logger;
        private readonly IStringLocalizer<SharedResource> _localizer;

        public GlobalConfigurationController(
            IConfigurationService configurationService, 
            ILogger<GlobalConfigurationController> logger,
            IStringLocalizer<SharedResource> localizer)
        {
            _configurationService = configurationService;
            _logger = logger;
            _localizer = localizer;
        }

        /// <summary>
        /// Get the generic application configuration
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(AppConfiguration), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppConfiguration>> GetConfiguration()
        {
            var config = await _configurationService.GetConfigurationAsync();
            return Ok(config);
        }

        /// <summary>
        /// Update the entire application configuration
        /// </summary>
        [HttpPut]
        [ProducesResponseType(typeof(AppConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AppConfiguration>> UpdateConfiguration([FromBody] AppConfiguration config)
        {
            try
            {
                var updatedConfig = await _configurationService.UpdateConfigurationAsync(config);
                return Ok(updatedConfig);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid configuration update request");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update only the LLM configuration section
        /// </summary>
        [HttpPut("llm")]
        [ProducesResponseType(typeof(LLMConfiguration), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LLMConfiguration>> UpdateLLMConfiguration([FromBody] LLMConfiguration config)
        {
            try
            {
                var updatedConfig = await _configurationService.UpdateLLMConfigurationAsync(config);
                return Ok(updatedConfig);
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "Invalid LLM configuration update request");
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Test the LLM connection settings
        /// </summary>
        [HttpPost("llm/test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> TestLLMConnection([FromBody] LLMConfiguration config)
        {
            try
            {
                var success = await _configurationService.TestLLMConnectionAsync(config);
                if (success)
                {
                    return Ok(new { success = true, message = _localizer["LlmConnectionSuccess"].Value });
                }
                else
                {
                    // Return OK with success=false because it's a valid test result, not a bad request?
                    // But usually connection failure might be treated as a validation error in UI.
                    // The catch block handles exceptions. Here strictly connection failed.
                    // Let's keep it 200 OK but success false if we want the frontend to display "Test Failed" gracefully.
                    // But Client code expects { success: boolean, message: string }.
                    // Note: If I return BadRequest, axios might throw.
                    // The hook code:
                    // const response = await axios.post...
                    // return response.data;
                    // catch ... return { success: false, message: ... }
                    // So if I return BadRequest, the hook catches it and returns success:false.
                    // If I return Ok({success:false}), the hook returns {success:false}.
                    // Returning BadRequest is semantically "The config is invalid" or "The request failed".
                    // Returning Ok with false means "Test executed, result is failure".
                    // I will return Ok with success:false for connection failures, and BadRequest for validation errors.
                    return Ok(new { success = false, message = _localizer["LlmConnectionFailed"].Value });
                }
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        /// <summary>
        /// Reset configuration to default values from app-config.json
        /// </summary>
        [HttpPost("reset")]
        [ProducesResponseType(typeof(AppConfiguration), StatusCodes.Status200OK)]
        public async Task<ActionResult<AppConfiguration>> ResetToDefault()
        {
            try
            {
                var config = await _configurationService.ResetToDefaultAsync();
                _logger.LogInformation("Configuration reset to defaults");
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting configuration to defaults");
                return BadRequest(new { message = "Failed to reset configuration" });
            }
        }
    }
}
