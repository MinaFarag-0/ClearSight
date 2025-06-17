namespace ClearSight.Core.Models
{
    public class UserCode
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        public string Code { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsUsed { get; set; }
    }
}
