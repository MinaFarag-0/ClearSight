using ClearSight.Core.CustomValidations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class PatientEditProfileDto
    {
        [RegularExpression(@"^[a-zA-Z0-9\s\-\._@+]*$", ErrorMessage = "Invalid characters detected.")]
        public string? FullName { get; set; }
        [ValidateImage(2)]
        public IFormFile? ProfileImage { get; set; }
        public string[]? PhoneNumbers { get; set; } = [];
    }
}
