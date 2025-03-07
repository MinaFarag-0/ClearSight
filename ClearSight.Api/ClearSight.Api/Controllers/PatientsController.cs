using AutoMapper;
using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Enums;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Context;
using ClearSight.Infrastructure.Implementations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace ClearSight.Api.Controllers
{
    /// <summary>
    /// Patient Controller For Make Prediction And Manage Patient Account
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class PatientsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;
        private readonly CloudinaryService _cloudinaryService;
        private readonly MLModelService _mlModelService;


        public PatientsController(AppDbContext context, IMapper mapper, CloudinaryService cloudinaryService, MLModelService mlModelService)
        {
            _context = context;
            _mapper = mapper;
            _cloudinaryService = cloudinaryService;
            _mlModelService = mlModelService;
        }

        /// <summary>
        /// Get Doctors Data.
        /// </summary>
        /// <returns>Returns Doctors Data.</returns>
        /// <response code="200">List Of Doctors Data.</response>
        [ProducesResponseType(typeof(List<DoctorProfileDto>), 200)]
        [HttpGet("DoctorsList")]
        [Authorize(Roles = "Patient")]
        public IActionResult DoctorsList()
        {
            var doctors = _context.Doctors.Include(x => x.User).ThenInclude(x=>x.PhoneNumbers).ToList();
            var doctorsList = _mapper.Map<List<DoctorProfileDto>>(doctors);
            return Ok(doctorsList);
        }

        /// <summary>
        /// Get Patient Profile Data.
        /// </summary>
        /// <returns>Returns Patient Profile Data.</returns>
        /// <response code="200">Patient Profile Data.</response>
        /// <response code="400">User NotFound error</response>
        [ProducesResponseType(typeof(PatientProfileDto), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [HttpGet("Profile")]
        [Authorize(Roles = "Patient")]
        public IActionResult Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            //var user = _context.Users.Find(userId);
            var patient = _context.Patients.Include(u=>u.User).FirstOrDefault(p=>p.PatientId == userId);

            if (patient == null)
                return BadRequest(new ApiErrorResponse { err_message = "User Not Found" });

            var UserPhoneNumbers = _context.UserPhoneNumbers.Where(u=>u.UserId ==userId).Select(p=>p.PhoneNumber).ToArray();
            
            var patientDto =_mapper.Map<PatientProfileDto>(patient);
            return Ok(patientDto);
        }

        /// <summary>
        /// Edit Patient Profile.
        /// </summary>
        /// <param name="dto">Patient Profile details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">User logined successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="401">UnAuthorized User error</response>
        /// <response code="404">User NotFound error</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ServerErrorResponse), 500)]
        [HttpPost("EditProfile")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> EditProfile([FromForm]PatientEditProfileDto dto)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.Find(id);
            var user = _context.Users.Include(p=>p.PhoneNumbers).FirstOrDefault(u=>u.Id==id);

            if (patient == null || user == null)
                return BadRequest(new ApiErrorResponse { err_message = "Invalid Details" });

            try
            {
                if(dto.FullName != null)
                {
                    user.FullName = dto.FullName;
                }
                if (dto.ProfileImage != null || dto.ProfileImage?.Length > 0)
                {
                    var imageUrl = await _cloudinaryService.UploadImageAsync(dto.ProfileImage,CloudFolder.UsersProfile);
                    user.ProfileImagePath = imageUrl;
                }
                var existingNumbers = user.PhoneNumbers.Select(p => p.PhoneNumber).ToList();
                var newNumbers = dto.PhoneNumbers.ToList();
                foreach (var phoneNumber in newNumbers)
                {
                    var regex = @"^01[0125]\d{8}$|^0\d{7,9}$|^1[5679]\d{3}$";
                    if (!existingNumbers.Contains(phoneNumber) && Regex.IsMatch(phoneNumber, regex))
                    {
                        user.PhoneNumbers.Add(new UserPhoneNumber
                        {
                            PhoneNumber = phoneNumber,
                            User = user
                        });
                    }
                }

                var numbersToRemove = user.PhoneNumbers.Where(p => !newNumbers.Contains(p.PhoneNumber)).ToList();

                foreach (var number in numbersToRemove)
                {
                    user.PhoneNumbers.Remove(number);
                }

                _context.Update(user);
                _context.SaveChanges();
                return Ok(new ApiSuccessResponse{ result = "Updated Successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new ServerErrorResponse { err_message = ex.Message });
            }
        }


        /// <summary>
        /// Get List Of Doctors Have Access To Patient History Data.
        /// </summary>
        /// <returns>Returns List Doctors That Have Access To Patient Data.</returns>
        /// <response code="200">List Of Doctors Data.</response>
        [ProducesResponseType(typeof(List<DoctorProfileDto>), 200)]

        [HttpGet("access-list")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DoctorAccessList()
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var grantDoctorList = _context.PatientDoctorAccess.Include(x=>x.Doctor).ThenInclude(x => x.User).ThenInclude(x => x.PhoneNumbers).Where(x => x.PatientId == patientId)
                .Select(x=>x.Doctor).ToList();

            var doctorsList = _mapper.Map<IEnumerable<DoctorProfileDto>>(grantDoctorList);
            return Ok(doctorsList);
        }


        /// <summary>
        /// Grant Doctor Access To Patient Histroy.
        /// </summary>
        /// <returns>Returns Success Doctor Was Granted Access Successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Error Invalid Doctor Id or Doctor already has access.</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [Authorize(Roles = "Patient")]
        [HttpPost("grant-access")]
        public async Task<IActionResult> GrantDoctorAccess(string doctorId)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doctorExists = _context.Doctors.Find(doctorId);

            if (doctorExists == null)
                return BadRequest(new ApiErrorResponse { err_message = "Invalid doctor ID." });

            // Check if access is already granted
            var exists = await _context.PatientDoctorAccess
                .AnyAsync(a => a.PatientId == patientId && a.DoctorId == doctorId);

            if (exists)
            {
                return BadRequest(new ApiErrorResponse { err_message="Doctor already has access." });
            }

            _context.PatientDoctorAccess.Add(new PatientDoctorAccess
            {
                DoctorId = doctorId,
                PatientId = patientId
            });

            await _context.SaveChangesAsync();
            return Ok(new ApiSuccessResponse{ result = "Doctor access granted." });
        }


        /// <summary>
        /// Revoke Doctor Access To Patient Histroy.
        /// </summary>
        /// <returns>Returns Success Result.</returns>
        /// <response code="200">Success Doctor Was Revoked To Access Patient History.</response>
        /// <response code="400">Error Invalid Doctor Id or Doctor already has access.</response>
        [ProducesResponseType(typeof(ApiSuccessResponse), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [HttpPost("revoke-access")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> RevokeDoctorAccess(string doctorId)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var access = await _context.PatientDoctorAccess
                .FirstOrDefaultAsync(a => a.PatientId == patientId && a.DoctorId == doctorId);
            
            var doctorExists = _context.Doctors.Find(doctorId);
            if (doctorExists == null)
                return BadRequest(new ApiErrorResponse { err_message = "Invalid doctor ID." });

            if (access == null)
            {
                return NotFound(new ApiErrorResponse { err_message = "Access not found." });
            }

            _context.PatientDoctorAccess.Remove(access);
            await _context.SaveChangesAsync();

            return Ok(new ApiSuccessResponse{ result = "Doctor access revoked." });
        }

        /// <summary>
        /// Make New Scan.
        /// </summary>
        /// <param name="scan">Patient Scan Image</param>
        /// <returns>Returns Prediction or validation errors.</returns>
        /// <response code="200">Prediction successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(PatientHistoryDto), 200)]
        [ProducesResponseType(typeof(ApiErrorResponse), 400)]
        [ProducesResponseType(typeof(ServerErrorResponse), 500)]
        [HttpPost("Scan")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Scan([FromForm]ScanDto scan)
        {
            if (scan.ScanImage == null || scan.ScanImage.Length == 0)
                return BadRequest(new ApiErrorResponse { err_message = "No file uploaded." });

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.Include(x => x.User).FirstOrDefault(x => x.PatientId == userId);

            try
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(scan.ScanImage, CloudFolder.PatientsScans);
                //var imageUrl = "path";

                var prediction = await _mlModelService.Predict(scan.ScanImage);

                var Check = new PatientHistory()
                {
                    PatientName = patient.User.UserName,
                    Date = DateTime.UtcNow,
                    PatientId = userId,
                    FundusCameraPath = imageUrl,
                    Patient = patient,
                    FundusCameraResult = prediction
                };

                _context.PatientHistories.Add(Check);
                _context.SaveChanges();
                var res =_mapper.Map<PatientHistoryDto>(Check);
                return Ok(res);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ServerErrorResponse { err_message = ex.Message });
            }

        }
        
        /// <summary>
        /// Get Patient History.
        /// </summary>
        /// <returns>Returns Patient History.</returns>
        /// <response code="200">List Of Patient History</response>
        /// <response code="404">No History</response>
        [ProducesResponseType(typeof(List<PatientHistoryDto>), 200)]
        [HttpGet("GetPatientHistory")]
        [Authorize(Roles = "Patient")]
        public IActionResult GetPatientHistory()
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = _context.Patients.Include(x => x.User).FirstOrDefault(x => x.PatientId == patientId);
            var history = _context.PatientHistories.Include(x => x.Doctor).Where(x => x.PatientId == patientId).OrderByDescending(o => o.Date).ToList();
            
            var patientHistory = _mapper.Map<IEnumerable<PatientHistoryDto>>(history);

            return Ok(patientHistory);
        }


    }
}
