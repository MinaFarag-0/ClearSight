using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class PatientProfileDto
    {
        public string PatientId { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string ProfileImagePath { get; set; }
        public string Email { get; set; }
        public string[]? PhoneNumbers { get; set; } = [];
    }
}
