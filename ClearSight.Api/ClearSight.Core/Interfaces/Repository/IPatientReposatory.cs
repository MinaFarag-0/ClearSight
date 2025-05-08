using ClearSight.Core.Mosels;

namespace ClearSight.Core.Interfaces.Repository
{
    public interface IPatientReposatory : IBaseRepository<Patient>
    {
        Patient GetPatientWithUserAndPhoneNumbersAsync(string patientId);
    }
}
