using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Models;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClearSight.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeedbackController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;

        public FeedbackController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost("SubmitFeedback")]
        [Authorize(Roles = "Doctor,Patient")]
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

            _context.Feedbacks.Add(feedBack);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResponse("Added Feedback Successfully"));
        }

        [HttpGet("GetRecentFeedBacks")]
        public IActionResult GetRecentFeedBacks(int count = 3)
        {
            var feedbacks2 = _context.Feedbacks.Select(f => new FeedbackDto2
            {
                Content = f.Message,
                SubmittedAt = f.SubmittedAt,
                UserId = f.UserId,
                UserName = f.User.FullName,
                UserImage = f.User.ProfileImagePath,
                UserRole = _userManager.IsInRoleAsync(f.User, "Doctor").Result ? "Doctor" : "Patient"
            }).ToList();

            var feedbacks = feedbacks2.TakeLast(count).ToList();
            return Ok(ApiResponse<List<FeedbackDto2>>.SuccessResponse(feedbacks));
        }
    }

}
