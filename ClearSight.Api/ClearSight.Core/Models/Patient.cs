namespace ClearSight.Core.Mosels
{
    public class Patient
    {
        public string PatientId { get; set; }
        public User User { get; set; }
        public ICollection<PatientHistory> PatientHistories { get; set; } = [];
        public ICollection<PatientDoctorAccess> PatientDoctorAccess { get; set; } = [];

    }
}
