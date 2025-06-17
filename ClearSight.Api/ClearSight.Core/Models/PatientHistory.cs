using System.ComponentModel.DataAnnotations.Schema;

namespace ClearSight.Core.Models
{
    public class PatientHistory
    {
        public int Id { get; set; }
        public string PatientName { get; set; }
        public string? FundusCameraResult { get; set; }
        public double? Confidence { get; set; }
        public string? PatientId { get; set; }
        public Patient? Patient { get; set; }
        public string? DoctorId { get; set; }
        public Doctor? Doctor { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string FundusCameraPath { get; set; }
        [NotMapped]
        public string? ArabicName { get; set; }
        [NotMapped]
        public string? DiseaseMsg { get; set; }
    }
}
