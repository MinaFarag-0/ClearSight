using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.AuthenticationDtos
{
    public class ResetPasswordDto
    {
        [Required] 
        public string Email { get; set; }
        [Required]
        public string VerificationCode { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
