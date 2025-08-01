﻿namespace ClearSight.Core.Models
{
    public class PatientDoctorAccess
    {
        public int Id { get; set; }
        public string DoctorId { get; set; }
        public string PatientId { get; set; }
        public Doctor Doctor { get; set; }
        public Patient Patient { get; set; }
    }
}
