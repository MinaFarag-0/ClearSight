using ClearSight.Core.CustomValidations;
using Microsoft.AspNetCore.Http;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class ActivateDoctorAccountDto
    {
        [ValidateImage(5)]
        public IFormFile Document { get; set; }
    }
}
