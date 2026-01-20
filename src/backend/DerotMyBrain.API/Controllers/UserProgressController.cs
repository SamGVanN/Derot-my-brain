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
            if (string.IsNullOrEmpty(activity.Id))
            {
                activity.Id = Guid.NewGuid().ToString();
            }
            if (activity.FirstAttemptDate == default)
            {
                activity.FirstAttemptDate = DateTime.UtcNow;
            }
            if (activity.LastAttemptDate == default)
            {
                activity.LastAttemptDate = DateTime.UtcNow;
            }

            // In SQLite/EF Core, we should check if it exists or just add. 
            // The JSON logic was "Add", implying a new entry or appending to list.
            // If ID exists, we might need Update, but typically AddActivity implies creation.
            // However, the client might be sending updates this way? 
            // The JSON code was plain "history.Add(activity)", which would create duplicates if ID existed in list but JSON list just appends.
            // But EF Core will throw if PK exists.
            
            // Let's check if it exists to be safe, or just try Create.
            // Standardizing on Create.
            
            try 
            {
                await _repository.CreateAsync(activity);
            }
            catch (InvalidOperationException) 
            {
                // Fallback for duplicates if client sends same ID twice unintentionally, 
                // or update availability.
                // But for now, let's assume it's a new activity.
                // If it crashes on PK, we return 409 or 500.
                // To be safe and mimic 'Add' to log, we usually expect new IDs.
                throw;
            }

            return CreatedAtAction(nameof(GetHistory), new { userId }, activity);
        }

    }
}
