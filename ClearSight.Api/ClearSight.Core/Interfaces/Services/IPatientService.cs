using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Mosels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace ClearSight.Core.Interfaces.Services
{
    public interface IPatientService
    {
        Task<PatientProfileDto> GetPatientDtoByIdAsync(string patientId);
        Task<Patient> GetPatientByIdAsync(string id);
        Task<IEnumerable<DoctorProfileDto>> GetDoctorsDtosAsync(int pageNumber = 1, int pageSize = 5);
        Task<int> GetDoctorsCountAsync();
        Task<int> GetDoctorsCountAsync(string patientId);
        Task<int> GetDoctorsCountAsync(Expression<Func<Doctor, bool>> criteria);
        Task<int> GetPatientHistoriesCountAsync(string patientId);
        Task<IEnumerable<PatientHistoryDto>> GetPatientHistoryAsync(string patientId, int pageNumber, int pageSize);
        Task<IEnumerable<DoctorProfileDto>> GetDoctorsDtosAsync(Expression<Func<Doctor, bool>> criteria, int pageNumber = 1, int pageSize = 5);
        Task<IEnumerable<DoctorProfileDto>> GetDoctorsAccessAsync(string patientId, int pageNumber = 1, int pageSize = 5);
        Task UpdatePatient(Patient patient, PatientEditProfileDto dto);
        Task<bool> FindDoctor(string doctorId);
        Task<bool> DoctorHasAccess(string patientId, string doctorId);
        Task GrandDoctorAccess(string patientId, string doctorId);
        Task RemoveDoctorAccess(string patientId, string doctorId);
        Task<PatientHistoryDto> AddPatientHistory(PatientHistory patientHistory);
    }
}
