using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClearSight.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        private readonly UserManager<User> _userManager;

        public FeedbackController(UserManager<User> userManager, IFeedbackService feedbackService)
        {
            _userManager = userManager;
            _feedbackService = feedbackService;
        }

        /// <summary>
        /// Submit New Feedback.
        /// </summary>
        /// <param name="feedback"> number of feedbacks to display </param>
        /// <returns>Returns Submit Feedback Result.</returns>
        /// <response code="200">feedback added successfully.</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        [HttpPost("SubmitFeedback")]
        [Authorize(Policy = "WriteFeedback")]
        public async Task<IActionResult> SubmitFeedback([FromBody] FeedbackDto feedback)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(userId);
            var feedBack = new Feedback
            {
                Message = feedback.Content,
                SubmittedAt = DateTime.UtcNow,
                User = user
            };

            await _feedbackService.AddFeedbackAsync(feedBack);

            return Ok(ApiResponse<string>.SuccessResponse("Added Feedback Successfully"));
        }


        /// <summary>
        /// Get Recent Feedbacks By Count.
        /// </summary>
        /// <returns>Returns Recent Feedbacks Data.</returns>
        /// <response code="200">List Of recent feedbacks.</response>
        [ProducesResponseType(typeof(ApiResponse<List<FeedbackResponseDto>>), 200)]
        [HttpGet("GetRecentFeedBacks")]
        public async Task<IActionResult> GetRecentFeedBacks(int count = 3)
        {
            var feedbacks = await _feedbackService.GetRecentFeedBacks(count);
            return Ok(ApiResponse<List<FeedbackResponseDto>>.SuccessResponse(feedbacks));
        }

    }

}
