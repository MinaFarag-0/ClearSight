using AutoMapper;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Enums;
using ClearSight.Core.Helpers;
using ClearSight.Core.Interfaces;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Models;
using ClearSight.Core.Mosels;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class DoctorService : IDoctorService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IConfiguration _configuration;


        public DoctorService(IUnitOfWork unitOfWork, IMapper mapper, CloudinaryService cloudinaryService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
        }

        public async Task<Doctor> GetDoctorByIdAsync(string id)
        {
            var doctor = await _unitOfWork.Doctors.GetWithIncludesAsync(x => x.DoctorId == id, x => x.User, x => x.User.PhoneNumbers);
            return doctor;
        }
        public async Task<Patient> GetPatientByIdAsync(string id)
        {
            var patient = await _unitOfWork.Patients.GetWithIncludesAsync(x => x.PatientId == id, x => x.User, x => x.User.PhoneNumbers);
            return patient;
        }
        public async Task<PatientHistoryDto> AddPatientHistory(PatientHistory patientHistory)
        {
            await _unitOfWork.PatientHistories.AddAsync(patientHistory);
            await _unitOfWork.SaveChangesAsync();
            var res = _mapper.Map<PatientHistoryDto>(patientHistory);
            return res;
        }

        public async Task<bool> IsAuthenticated(string doctorId, string patientId)
        {
            return await _unitOfWork.PatientDoctorAccess.AnyAsync(x => x.PatientId == patientId && x.DoctorId == doctorId);
        }

        public async Task<int> GetPatientsGrantAccessToDoctorCountAsync(string doctorId)
        {
            var patientsCount = await _unitOfWork.PatientDoctorAccess.CountAsync(x => x.DoctorId == doctorId);
            return patientsCount;
        }

        public async Task<DoctorProfileDto> GetDoctorDtoByIdAsync(string id)
        {
            var doctor = await _unitOfWork.Doctors.GetWithIncludesAsync(x => x.DoctorId == id, x => x.User.PhoneNumbers);
            var doctorDto = _mapper.Map<DoctorProfileDto>(doctor);
            return doctorDto;
        }

        public async Task<IEnumerable<PatientProfileDto>> GetPatientsDtosAsync(string doctorId, int pageNumber, int pageSize)
        {
            var patientsProfiles = await _unitOfWork.PatientDoctorAccess
                .GetAllWithIncludesAsync(x => x.DoctorId == doctorId, (pageNumber - 1) * pageSize, pageSize, x => x.Patient, x => x.Patient.User, x => x.Patient.User.PhoneNumbers);
            var patientProfilesDto = _mapper.Map<IEnumerable<PatientProfileDto>>(patientsProfiles.Select(x => x.Patient));
            return patientProfilesDto;
        }
        public async Task<IEnumerable<PatientProfileDto>> GetPatientsDtosAsync(string docotrId, string patientName, int pageNumber, int pageSize)
        {
            var patientsProfiles = await _unitOfWork.PatientDoctorAccess
                .GetAllWithIncludesAsync((x => x.DoctorId == docotrId &&
                (x.Patient.User.UserName.Contains(patientName) || x.Patient.User.FullName.Contains(patientName))),
                (pageNumber - 1) * pageSize, pageSize, x => x.Patient, x => x.Patient.User, x => x.Patient.User.PhoneNumbers);
            var patientProfilesDto = _mapper.Map<IEnumerable<PatientProfileDto>>(patientsProfiles.Select(x => x.Patient));
            return patientProfilesDto;
        }
        public async Task<int> GetPatientHistoriesCountAsync(string patientId)
        {
            var doctorsCount = await _unitOfWork.PatientHistories.CountAsync(x => x.PatientId == patientId);
            return doctorsCount;
        }
        public async Task<ServiceResponse<string>> UploadDocumentAsync(IFormFile doc, string doctorId)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
            var url = await _cloudinaryService.UploadImageAsync(doc, CloudFolder.UsersProfile);

            if (doctor == null || url == null)
            {
                return new ServiceResponse<string>
                {
                    IsSuccess = true,
                    Response = "Error While Handle Request"
                };
            }

            doctor.UploadedDocumentPath = url;
            await _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            return new ServiceResponse<string>
            {
                IsSuccess = true,
                Response = "Your Account Is Under Activation If not Activated in 24h Please Call Us"
            };
        }
        public async Task<IEnumerable<PatientHistoryDto>> GetPatientHistoryAsync(string id, int skip, int take)
        {
            var patientHistory = await _unitOfWork.PatientHistories.GetAllWithIncludesAsync(x => x.PatientId == id, (skip - 1) * take, take, x => x.Doctor.User.PhoneNumbers);
            var patientHistoryDto = _mapper.Map<IEnumerable<PatientHistoryDto>>(patientHistory);
            foreach (var item in patientHistoryDto)
            {
                item.ArabicName = _configuration[$"DiseasesMSG:{item.FundusCameraResult}:0"];
                item.DiseaseMsg = _configuration[$"DiseasesMSG:{item.FundusCameraResult}:1"];
            }
            return patientHistoryDto;
        }
        public async Task UpdateDoctor(Doctor doctor, DoctorProfileEditDto dto)
        {
            if (dto.FullName != null)
            {
                doctor.User.FullName = dto.FullName;
            }
            if (dto.ProfileImage != null || dto.ProfileImage?.Length > 0)
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(dto.ProfileImage, CloudFolder.UsersProfile);
                doctor.User.ProfileImagePath = imageUrl;
            }

            var existingNumbers = doctor.User.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
            var newNumbers = dto.PhoneNumbers.ToList();
            foreach (var phoneNumber in newNumbers)
            {
                var regex = @"^01[0125]\d{8}$|^0\d{7,9}$|^1[5679]\d{3}$";
                if (!existingNumbers.Contains(phoneNumber) && Regex.IsMatch(phoneNumber, regex))
                {
                    doctor.User.PhoneNumbers.Add(new UserPhoneNumber
                    {
                        PhoneNumber = phoneNumber,
                        User = doctor.User,
                    });
                }
            }

            var numbersToRemove = doctor.User.PhoneNumbers.Where(p => !newNumbers.Contains(p.PhoneNumber)).ToList();

            foreach (var number in numbersToRemove)
            {
                doctor.User.PhoneNumbers.Remove(number);
            }

            if (dto?.AvailableFrom != null)
            {
                doctor.AvailableFrom = dto.AvailableFrom;
            }
            if (dto?.AvailableTo != null)
            {
                doctor.AvailableTo = dto.AvailableTo;
            }
            if (dto?.Address != null)
            {
                doctor.Address = dto.Address;
            }
            doctor.DaysOff = DaysOffHelper.ConvertToEnum(dto.DaysOff).Item1;
            doctor.AvailableForCureentMonth = dto.AvailableForCureentMonth;
            await _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
