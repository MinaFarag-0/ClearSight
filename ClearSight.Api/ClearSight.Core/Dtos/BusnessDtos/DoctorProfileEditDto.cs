using ClearSight.Core.CustomValidations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class DoctorProfileEditDto
    {
        public string? FullName { get; set; }
        public TimeOnly AvailableFrom { get; set; }
        public TimeOnly AvailableTo { get; set; }
        public List<string>? DaysOff { get; set; } = [];
        public string[]? PhoneNumbers { get; set; }
        [ValidateImage(2)]
        public IFormFile? ProfileImage { get; set; }
        public bool AvailableForCureentMonth { get; set; }
        public string? Address { get; set; }
    }
}
