using System.ComponentModel.DataAnnotations;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class FeedbackDto
    {
        [Required(ErrorMessage = "Message is required.")]
        [StringLength(1000, ErrorMessage = "Message must be less than 1000 characters.")]
        [RegularExpression(@"^[a-zA-Z0-9\s,.!?]+$", ErrorMessage = "Only alphanumeric and punctuation allowed.")]
        public string Content { get; set; }
    }
    public class FeedbackDto2
    {
        public string Content { get; set; }
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string UserImage { get; set; }
        public string UserRole { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

}
