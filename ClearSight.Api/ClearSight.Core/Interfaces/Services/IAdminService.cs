using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Models;

namespace ClearSight.Core.Interfaces.Services
{
    public interface IAdminService
    {
        Task<List<DoctorActivateProfile>> GetDoctorsActivateProfilesAsync(int pageNumber = 1, int pageSize = 5);
        Task<DoctorActivateProfile> GetDoctorActivateProfileAsync(string doctorId);
        Task<ServiceResponse<string>> ActivateDoctorAsync(string doctorId);
        Task<int> GetDoctorsActivateProfilesCountAsync();
        Task<Doctor> GetDoctorAsync(string doctorId);
        Task<ServiceResponse<string>> RejectDoctorAccountAsync(string doctorId);
    }
}
