using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class MLModelResult
    {
        public string Prediction { get; set; }
        public double Confidence { get; set; }
    }
}
