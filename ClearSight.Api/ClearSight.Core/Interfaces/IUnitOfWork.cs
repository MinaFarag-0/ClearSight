using ClearSight.Core.Interfaces.Repository;
using ClearSight.Core.Mosels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IPatientReposatory Patients { get; }
        IDoctorReposatory Doctors { get; }
        IBaseRepository<PatientDoctorAccess> PatientDoctorAccess { get; }
        IBaseRepository<PatientHistory> PatientHistories { get; }

        Task<int> SaveChangesAsync();
    }
}