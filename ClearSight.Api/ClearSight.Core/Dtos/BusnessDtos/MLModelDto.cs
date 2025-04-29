using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class MLModelDto
    {
        public MLModelResult? Result { get; set; }
        public bool IsSuccess { get; set; }
        public string? ArabicName { get; set; }
        public string? DiseaseMsg { get; set; }
    }
}
