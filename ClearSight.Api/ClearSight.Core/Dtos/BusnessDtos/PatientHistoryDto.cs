using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Dtos.BusnessDtos
{
    public class PatientHistoryDto
    {
        public string DoctorName { get; set; }
        public string PatientName { get; set; }
        public string FundusCameraResult { get; set; }
        public string FundusCameraPath { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
    }
}
