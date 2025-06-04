using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Interfaces;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Models;
using ClearSight.Core.Mosels;
using Microsoft.AspNetCore.Identity;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<User> _userManager;

        public FeedbackService(IUnitOfWork unitOfWork, UserManager<User> userManager)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
        }

        public async Task AddFeedbackAsync(Feedback feedback)
        {
            if (await _unitOfWork.Feedbacks.AnyAsync(x => x.UserId == feedback.User.Id))
            {
                var fb = await _unitOfWork.Feedbacks.FindAsync(x => x.UserId == feedback.User.Id);
                fb.SubmittedAt = DateTime.UtcNow;
                fb.Message = feedback.Message;
                await _unitOfWork.Feedbacks.Update(fb);
            }
            else
            {
                await _unitOfWork.Feedbacks.AddAsync(feedback);
            }
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<FeedbackResponseDto>> GetRecentFeedBacks(int count)
        {
            var allFeedbacks = await _unitOfWork.Feedbacks.GetAllWithIncludesAsync(x => x.User);

            var feedbacks = allFeedbacks.TakeLast(count).Select(f => new FeedbackResponseDto
            {
                Content = f.Message,
                SubmittedAt = f.SubmittedAt,
                UserId = f.UserId,
                UserName = f.User.FullName,
                UserImage = f.User.ProfileImagePath,
                UserRole = _userManager.IsInRoleAsync(f.User, "Doctor").Result ? "Doctor" : "Patient"
            }).ToList();

            return feedbacks;
        }

    }
}
