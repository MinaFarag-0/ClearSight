﻿namespace ClearSight.Core.Models
{
    public class Feedback
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Message { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

}
