using AutoMapper;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Enums;
using ClearSight.Core.Interfaces;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Models;
using Microsoft.Extensions.Configuration;

namespace ClearSight.Infrastructure.Implementations.Services
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;
        private readonly IConfiguration _configuration;

        public AdminService(IUnitOfWork unitOfWork, IMapper mapper, CloudinaryService cloudinaryService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _configuration = configuration;
        }
        public async Task<DoctorActivateProfile> GetDoctorActivateProfileAsync(string doctorId)
        {
            var doctor = await _unitOfWork.Doctors.GetWithIncludesAsync(x => x.Status == VerificationStatus.Pending, x => x.User.PhoneNumbers);
            var doctorDto = _mapper.Map<DoctorActivateProfile>(doctor);
            return doctorDto;
        }


        public async Task<List<DoctorActivateProfile>> GetDoctorsActivateProfilesAsync(int pageNumber = 1, int pageSize = 5)
        {
            var doctors = await _unitOfWork.Doctors.GetAllWithIncludesAsync(x => x.Status == VerificationStatus.Pending, pageNumber, pageSize, x => x.User.PhoneNumbers);
            var doctorsDtos = _mapper.Map<List<DoctorActivateProfile>>(doctors);
            return doctorsDtos;
        }

        public async Task<int> GetDoctorsActivateProfilesCountAsync()
        {
            var doctorsCount = await _unitOfWork.Doctors.CountAsync(x => x.Status == VerificationStatus.Pending);
            return doctorsCount;
        }
        public async Task<Doctor> GetDoctorAsync(string doctorId)
        {
            var doctorName = await _unitOfWork.Doctors.GetWithIncludesAsync(x => x.DoctorId == doctorId, x => x.User);
            return doctorName;
        }
        public async Task<ServiceResponse<string>> ActivateDoctorAsync(string doctorId)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
            if (doctor == null)
            {
                return new ServiceResponse<string>()
                {
                    IsSuccess = false,
                    Response = "Error While Activate Doctor It May Be Doctor Account Is Already Activated"
                };
            }
            doctor.Status = VerificationStatus.Approved;
            await _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            return new ServiceResponse<string>
            {
                IsSuccess = true,
                Response = "Doctor Account Activated Success"
            };
        }
        public async Task<ServiceResponse<string>> RejectDoctorAccountAsync(string doctorId)
        {
            var doctor = await _unitOfWork.Doctors.GetByIdAsync(doctorId);
            if (doctor == null)
            {
                return new ServiceResponse<string>()
                {
                    IsSuccess = false,
                    Response = "Error While Reject Doctor It May Be Doctor Account Is Already Rejected"
                };
            }
            doctor.Status = VerificationStatus.Rejected;
            await _unitOfWork.Doctors.Update(doctor);
            await _unitOfWork.SaveChangesAsync();
            return new ServiceResponse<string>
            {
                IsSuccess = true,
                Response = "Doctor Account Rejected Successfully"
            };
        }
    }
}
