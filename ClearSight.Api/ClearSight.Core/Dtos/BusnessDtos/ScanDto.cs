using ClearSight.Core.CustomValidations;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class ScanDto
    {
        [ValidateImage(2)]
        public IFormFile ScanImage { get; set; }
    }
}
