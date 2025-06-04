using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Models;

namespace ClearSight.Core.Interfaces.Services
{
    public interface IFeedbackService
    {
        Task AddFeedbackAsync(Feedback feedback);
        Task<List<FeedbackResponseDto>> GetRecentFeedBacks(int count);
    }
}