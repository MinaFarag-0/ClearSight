using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ClearSight.Core.Models
{
    public class UserPhoneNumber
    {
        public int Id { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
        [Required]
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
    }
}
