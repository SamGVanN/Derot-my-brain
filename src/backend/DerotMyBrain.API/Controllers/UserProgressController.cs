using DerotMyBrain.API.Models;
using DerotMyBrain.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DerotMyBrain.API.Controllers
{
    [ApiController]
    [Route("api/users/{userId}/history")]
    public class UserProgressController : ControllerBase
    {
        private readonly IJsonRepository<List<UserActivity>> _repository;

        public UserProgressController(IJsonRepository<List<UserActivity>> repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public async Task<ActionResult<List<UserActivity>>> GetHistory(string userId)
        {
            var history = await _repository.GetAsync($"history_{userId}.json");
            return Ok(history);
        }

        [HttpPost]
        public async Task<ActionResult<UserActivity>> AddActivity(string userId, [FromBody] UserActivity activity)
        {
            // Ensure the activity is linked to the user
            activity.UserId = userId;
            if (string.IsNullOrEmpty(activity.Id))
            {
                activity.Id = Guid.NewGuid().ToString();
            }
            if (activity.Timestamp == default)
            {
                activity.Timestamp = DateTime.UtcNow;
            }

            var history = await _repository.GetAsync($"history_{userId}.json");
            history.Add(activity);
            await _repository.SaveAsync($"history_{userId}.json", history);

            return CreatedAtAction(nameof(GetHistory), new { userId }, activity);
        }

    }
}
