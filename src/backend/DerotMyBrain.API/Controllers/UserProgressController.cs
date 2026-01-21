using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/history")]
    public class UserProgressController : ControllerBase
    {
        private readonly IActivityRepository _repository;

        public UserProgressController(IActivityRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserActivity>>> GetHistory(string userId)
        {
            var history = await _repository.GetAllAsync(userId);
            return Ok(history);
        }

        [HttpPost]
        public async Task<ActionResult<UserActivity>> AddActivity(string userId, [FromBody] UserActivity activity)
        {
            // Ensure the activity is linked to the user
            activity.UserId = userId;
            if (activity.SessionDate == default)
            {
                activity.SessionDate = DateTime.UtcNow;
            }

            try 
            {
                await _repository.CreateAsync(activity);
            }
            catch (Exception ex) 
            {
                return StatusCode(500, new { message = "Error creating activity", details = ex.Message });
            }

            return CreatedAtAction(nameof(GetHistory), new { userId }, activity);
        }


    }
}
