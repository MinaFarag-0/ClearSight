using ClearSight.Core.Enums;
using ClearSight.Core.Mosels;

namespace ClearSight.Core.Models
{
    public class Doctor
    {
        public string DoctorId { get; set; }
        public User User { get; set; }
        public ICollection<PatientHistory> PatientHistories { get; set; } = [];
        public ICollection<PatientDoctorAccess> PatientDoctorAccess { get; set; } = [];
        public VerificationStatus Status { get; set; } = VerificationStatus.Pending;
        public string? UploadedDocumentPath { get; set; }
        public TimeOnly AvailableFrom { get; set; }
        public TimeOnly AvailableTo { get; set; }
        public bool AvailableForCureentMonth { get; set; }
        public DaysOff DaysOff { get; set; }
        public string? Address { get; set; }

    }
}
