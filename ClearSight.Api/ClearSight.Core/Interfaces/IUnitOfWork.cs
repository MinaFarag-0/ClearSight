using ClearSight.Core.Interfaces.Repository;
using ClearSight.Core.Models;
using ClearSight.Core.Mosels;

namespace ClearSight.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IPatientReposatory Patients { get; }
        IDoctorReposatory Doctors { get; }
        IBaseRepository<PatientDoctorAccess> PatientDoctorAccess { get; }
        IBaseRepository<PatientHistory> PatientHistories { get; }
        IBaseRepository<Feedback> Feedbacks { get; }

        Task<int> SaveChangesAsync();
    }
}