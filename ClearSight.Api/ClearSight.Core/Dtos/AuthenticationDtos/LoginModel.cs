using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.AuthenticationDtos
{
    public class LoginModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        [EmailAddress(ErrorMessage = "Not Valid Email")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
