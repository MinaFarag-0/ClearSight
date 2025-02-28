namespace ClearSight.Core.Mosels
{
    public class Doctor
    {
        public string DoctorId { get; set; }
        public User User { get; set; }
        public ICollection<PatientHistory> PatientHistories { get; set; } = [];
        public ICollection<PatientDoctorAccess> PatientDoctorAccess { get; set; } = [];

        public TimeOnly AvailableFrom { get; set; }
        public TimeOnly AvailableTo { get; set; }
        public bool AvailableForCureentMonth { get; set; }
        public string[]? DaysOff { get; set; } = [];
        public string? Address { get; set; } 

    }
}
