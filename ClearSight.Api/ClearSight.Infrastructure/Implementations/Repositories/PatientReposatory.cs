using ClearSight.Core.Interfaces.Repository;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace ClearSight.Infrastructure.Implementations.Repositories
{
    public class PatientReposatory : BaseRepository<Patient>, IPatientReposatory
    {
        public PatientReposatory(AppDbContext context) : base(context)
        {
        }

        public Patient GetPatientWithUserAndPhoneNumbersAsync(string patientId)
        {
            return _context.Patients.Include(p => p.User).ThenInclude(p => p.PhoneNumbers).FirstOrDefault(x => x.PatientId == patientId);
        }
    }
}
