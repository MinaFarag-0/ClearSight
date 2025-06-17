using AutoMapper;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Enums;
using ClearSight.Core.Interfaces;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class PatientService : IPatientService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IConfiguration _configuration;

        public PatientService(IUnitOfWork unitOfWork, IMapper mapper, CloudinaryService cloudinaryService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
        }

        public async Task<Patient> GetPatientByIdAsync(string id)
        {
            var patient = await _unitOfWork.Patients.GetWithIncludesAsync(x => x.PatientId == id, x => x.User.PhoneNumbers);
            return patient;
        }
        public async Task<PatientProfileDto> GetPatientDtoByIdAsync(string patientId)
        {
            var patient = await _unitOfWork.Patients.GetWithIncludesAsync(p => p.PatientId == patientId, x => x.User.PhoneNumbers);
            var patientDto = _mapper.Map<PatientProfileDto>(patient);
            return patientDto;
        }
        public async Task UpdatePatient(Patient patient, PatientEditProfileDto dto)
        {
            if (dto.FullName != null)
            {
                patient.User.FullName = dto.FullName;
            }
            if (dto.ProfileImage != null || dto.ProfileImage?.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(dto.ProfileImage, CloudFolder.UsersProfile);
                patient.User.ProfileImagePath = imageUrl;
            }
            var existingNumbers = patient.User.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
            var newNumbers = dto?.PhoneNumbers;
            foreach (var phoneNumber in newNumbers)
            {
                var regex = @"^01[0125]\d{8}$|^0\d{7,9}$|^1[5679]\d{3}$";
                if (!existingNumbers.Contains(phoneNumber) && Regex.IsMatch(phoneNumber, regex))
                {
                    patient.User.PhoneNumbers.Add(new UserPhoneNumber
                    {
                        PhoneNumber = phoneNumber,
                        User = patient.User
                    });
                }
            }

            var numbersToRemove = patient.User.PhoneNumbers.Where(p => !newNumbers.Contains(p.PhoneNumber)).ToList();

            foreach (var number in numbersToRemove)
            {
                patient.User.PhoneNumbers.Remove(number);
            }

            await _unitOfWork.Patients.Update(patient);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<bool> FindDoctor(string doctorId)
        {
            return await _unitOfWork.Doctors.AnyAsync(x => x.DoctorId == doctorId);
        }
        public async Task<bool> DoctorHasAccess(string patientId, string doctorId)
        {
            return await _unitOfWork.PatientDoctorAccess
                .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorId);
        }
        public async Task GrandDoctorAccess(string patientId, string doctorId)
        {
            await _unitOfWork.PatientDoctorAccess.AddAsync(new PatientDoctorAccess
            {
                PatientId = patientId,
                DoctorId = doctorId
            });
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task RemoveDoctorAccess(string patientId, string doctorId)
        {
            var access = await _unitOfWork.PatientDoctorAccess.FindAsync(x => x.DoctorId == doctorId && x.PatientId == patientId);
            if (access == null)
            {
                return;
            }
            await _unitOfWork.PatientDoctorAccess.Delete(access);
            await _unitOfWork.SaveChangesAsync();
        }
        public async Task<PatientHistoryDto> AddPatientHistory(PatientHistory patientHistory)
        {
            await _unitOfWork.PatientHistories.AddAsync(patientHistory);
            await _unitOfWork.SaveChangesAsync();
            var res = _mapper.Map<PatientHistoryDto>(patientHistory);
            return res;
        }
        public async Task<IEnumerable<DoctorProfileDto>> GetDoctorsDtosAsync(int pageNumber = 1, int pageSize = 5)
        {
            var doctors = await _unitOfWork.Doctors.GetAllWithIncludesAsync(x => x.Status == VerificationStatus.Approved, (pageNumber - 1) * pageSize, pageSize, x => x.User, x => x.User.PhoneNumbers);
            var doctorDto = _mapper.Map<IEnumerable<DoctorProfileDto>>(doctors);
            return doctorDto;
        }
        public async Task<IEnumerable<DoctorProfileDto>> GetDoctorsDtosAsync(Expression<Func<Doctor, bool>> criteria, int pageNumber = 1, int pageSize = 5)
        {
            var doctors = await _unitOfWork.Doctors.GetAllWithIncludesAsync(criteria, (pageNumber - 1) * pageSize, pageSize, x => x.User, x => x.User.PhoneNumbers);
            var doctorDto = _mapper.Map<IEnumerable<DoctorProfileDto>>(doctors);
            return doctorDto;
        }
        public async Task<int> GetDoctorsCountAsync()
        {
            var doctorsCount = await _unitOfWork.Doctors.CountAsync(x => x.Status == VerificationStatus.Approved);
            return doctorsCount;
        }
        public async Task<int> GetDoctorsCountAsync(string patientId)
        {
            var doctorsCount = await _unitOfWork.PatientDoctorAccess.CountAsync(x => x.PatientId == patientId);
            return doctorsCount;
        }
        public async Task<IEnumerable<DoctorProfileDto>> GetDoctorsAccessAsync(string patientId, int pageNumber = 1, int pageSize = 5)
        {
            var doctors = await _unitOfWork.PatientDoctorAccess.GetAllWithIncludesAsync(x => x.PatientId == patientId, (pageNumber - 1) * pageSize, pageSize, x => x.Doctor.User, x => x.Doctor.User.PhoneNumbers);
            var doctorDto = _mapper.Map<IEnumerable<DoctorProfileDto>>(doctors.Select(x => x.Doctor));
            return doctorDto;
        }
        public async Task<int> GetDoctorsCountAsync(Expression<Func<Doctor, bool>> criteria)
        {
            var doctorsCount = await _unitOfWork.Doctors.CountAsync(criteria);
            return doctorsCount;
        }
        public async Task<int> GetPatientHistoriesCountAsync(string patientId)
        {
            var doctorsCount = await _unitOfWork.PatientHistories.CountAsync(x => x.PatientId == patientId);
            return doctorsCount;
        }
        public async Task<IEnumerable<PatientHistoryDto>> GetPatientHistoryAsync(string patientId, int pageNumber, int pageSize)
        {
            var patientHistory = await _unitOfWork.PatientHistories.GetAllWithIncludesAsync(x => x.PatientId == patientId, (pageNumber - 1) * pageSize, pageSize, x => x.Doctor.User.PhoneNumbers);
            var patientHistoryDto = _mapper.Map<IEnumerable<PatientHistoryDto>>(patientHistory);
            foreach (var item in patientHistoryDto)
            {
                item.ArabicName = _configuration[$"DiseasesMSG:{item.FundusCameraResult}:0"];
                item.DiseaseMsg = _configuration[$"DiseasesMSG:{item.FundusCameraResult}:1"];
            }
            return patientHistoryDto;
        }
    }
}
