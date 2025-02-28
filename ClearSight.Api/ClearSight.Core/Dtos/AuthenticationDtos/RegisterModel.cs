using ClearSight.Core.CustomValidations;
using ClearSight.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.AuthenticationDtos
{
    public class RegisterModel
    {
        [Required]
        [RegularExpression(@"^[a-zA-Z0-9\s\-\._@+]*$", ErrorMessage = "Invalid characters detected.")]
        public string FullName { get; set; }
        //[Required]
        //[RegularExpression(@"^[a-zA-Z0-9\-\._@+]+$", ErrorMessage = "Invalid characters detected.")]
        //public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Not Valid Email")]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [Compare("Password", ErrorMessage = "Both Passwords are not matched..")]
        [PasswordRequirements]
        public string ConfirmPassword { get; set; }
        [Required]
        [AllowedValues("Patient", "Doctor", ErrorMessage = "Invalid role. Allowed values: Patient or Doctor")]
        public string Role { get; set; }
        //[Required]
        //[AllowedValues("Male", "Female",ErrorMessage = "Invalid Gender")]
        //public string Gender { get; set; }

        

    }

}
