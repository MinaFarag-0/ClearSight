using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Models;
using Microsoft.AspNetCore.Http;

namespace ClearSight.Core.Interfaces.Services
{
    public interface IDoctorService
    {
        Task<DoctorProfileDto> GetDoctorDtoByIdAsync(string id);
        Task<Doctor> GetDoctorByIdAsync(string id);
        Task<int> GetPatientsGrantAccessToDoctorCountAsync(string patientId);
        Task<IEnumerable<PatientProfileDto>> GetPatientsDtosAsync(string doctorId, int pageNumber, int pageSize);
        Task<IEnumerable<PatientProfileDto>> GetPatientsDtosAsync(string docotrId, string patientName, int pageNumber, int pageSize);
        Task UpdateDoctor(Doctor doctor, DoctorProfileEditDto dto);
        Task<Patient> GetPatientByIdAsync(string id);
        Task<PatientHistoryDto> AddPatientHistory(PatientHistory patientHistory);
        Task<bool> IsAuthenticated(string doctorId, string patientId);
        Task<int> GetPatientHistoriesCountAsync(string patientId);
        Task<IEnumerable<PatientHistoryDto>> GetPatientHistoryAsync(string id, int skip, int take);
        Task<ServiceResponse<string>> UploadDocumentAsync(IFormFile doc, string doctorId);
    }
}
