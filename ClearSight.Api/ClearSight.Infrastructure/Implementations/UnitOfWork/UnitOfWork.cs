using ClearSight.Core.Interfaces;
using ClearSight.Core.Interfaces.Repository;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using ClearSight.Infrastructure.Implementations.Repositories;

namespace ClearSight.Infrastructure.Implementations.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        public IDoctorReposatory Doctors { get; private set; }
        public IPatientReposatory Patients { get; private set; }
        public IBaseRepository<PatientDoctorAccess> PatientDoctorAccess { get; private set; }
        public IBaseRepository<PatientHistory> PatientHistories { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Patients = new PatientReposatory(_context);
            Doctors = new DoctorReposatory(_context);
            PatientDoctorAccess = new BaseRepository<PatientDoctorAccess>(_context);
            PatientHistories = new BaseRepository<PatientHistory>(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}