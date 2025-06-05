using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Enums;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Core.Mosels;
using ClearSight.Infrastructure.Implementations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ClearSight.Api.Controllers
{
    /// <summary>
    /// Patient Controller For Make Prediction And Manage Patient Account
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class PatientsController : ControllerBase
    {
        private readonly CloudinaryService _cloudinaryService;
        private readonly MLModelService _mlModelService;
        private readonly IPatientService _patientService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PatientsController> _logger;


        public PatientsController(CloudinaryService cloudinaryService, MLModelService mlModelService, IPatientService patientService, IConfiguration configuration, ILogger<PatientsController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _mlModelService = mlModelService;
            _patientService = patientService;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get Patient Profile Data.
        /// </summary>
        /// <returns>Returns Patient Profile Data.</returns>
        /// <response code="200">Patient Profile Data.</response>
        /// <response code="400">User NotFound error</response>
        /// <response code="401">User UnAutorized error</response>
        [ProducesResponseType(typeof(ApiResponse<PatientProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        [HttpGet("Profile")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Profile()
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patientDto = await _patientService.GetPatientDtoByIdAsync(patientId);
            return Ok(ApiResponse<PatientProfileDto>.SuccessResponse(patientDto));
        }

        /// <summary>
        /// Edit Patient Profile.
        /// </summary>
        /// <param name="dto">Patient Profile details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">User profile data edited successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="401">UnAuthorized User error</response>
        /// <response code="500">Server error</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        [HttpPost("EditProfile")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> EditProfile([FromForm] PatientEditProfileDto dto)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByIdAsync(patientId);

            if (patient == null)
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid Details,Patient Data Not Found"));


            try
            {
                await _patientService.UpdatePatient(patient, dto);
                return Ok(ApiResponse<string>.SuccessResponse("Updated Successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception While Edit Patient Profile");
                return BadRequest(ApiResponse<string>.FailureResponse(ex.Message));
            }
        }

        /// <summary>
        /// Get Doctors Data.
        /// </summary>
        /// <returns>Returns Doctors Data.</returns>
        /// <response code="200">List Of Doctors Data.</response>
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorProfileDto>>), 200)]
        [HttpGet("DoctorsList")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DoctorsList(int pageNumber = 1, int pageSize = 5)
        {
            var totalCount = await _patientService.GetDoctorsCountAsync();
            var items = await _patientService.GetDoctorsDtosAsync(pageNumber, pageSize);

            var pagedResult = new PagedResult<DoctorProfileDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<DoctorProfileDto>>.SuccessResponse(pagedResult));
        }
        /// <summary>
        /// Search Using Doctor Name.
        /// </summary>
        /// <returns>Returns Doctors Data.</returns>
        /// <response code="200">List Of Doctors Data.</response>
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorProfileDto>>), 200)]
        [HttpGet("SearchUsingDoctorName")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> SearchUsingDoctorName(string doctorName, int pageNumber = 1, int pageSize = 5)
        {
            var totalCount = await _patientService.GetDoctorsCountAsync(x => x.User.FullName.Contains(doctorName));
            var items = await _patientService.GetDoctorsDtosAsync(x => x.User.FullName.Contains(doctorName), pageNumber, pageSize);

            var pagedResult = new PagedResult<DoctorProfileDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<DoctorProfileDto>>.SuccessResponse(pagedResult));
        }

        /// <summary>
        /// Get List Of Doctors Have Access To Patient History Data.
        /// </summary>
        /// <returns>Returns List Doctors That Have Access To Patient Data.</returns>
        /// <response code="200">List Of Doctors Data.</response>
        [ProducesResponseType(typeof(ApiResponse<PagedResult<DoctorProfileDto>>), 200)]

        [HttpGet("access-list")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> DoctorAccessList(int pageNumber = 1, int pageSize = 5)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var grantDoctorList = await _patientService.GetDoctorsCountAsync(patientId);

            var items = await _patientService.GetDoctorsAccessAsync(patientId, pageNumber, pageSize);

            var pagedResult = new PagedResult<DoctorProfileDto>
            {
                Items = items.ToList(),
                TotalCount = grantDoctorList,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<DoctorProfileDto>>.SuccessResponse(pagedResult));
        }


        /// <summary>
        /// Grant Doctor Access To Patient Histroy.
        /// </summary>
        /// <returns>Returns Success Doctor Was Granted Access Successfully.</returns>
        /// <response code="200">Success.</response>
        /// <response code="400">Error Invalid Doctor Id or Doctor already has access.</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [Authorize(Roles = "Patient")]
        [HttpPost("grant-access")]
        public async Task<IActionResult> GrantDoctorAccess(string doctorId)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var isDoctorExists = await _patientService.FindDoctor(doctorId);

            if (!isDoctorExists)
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid doctor ID."));

            var exists = await _patientService.DoctorHasAccess(patientId, doctorId);

            if (exists)
            {
                return BadRequest(ApiResponse<string>.SuccessResponse("Doctor already has access."));
            }
            await _patientService.GrandDoctorAccess(patientId, doctorId);

            return Ok(ApiResponse<string>.SuccessResponse("Doctor access granted."));
        }


        /// <summary>
        /// Revoke Doctor Access To Patient Histroy.
        /// </summary>
        /// <returns>Returns Success Result.</returns>
        /// <response code="200">Success Doctor Was Revoked To Access Patient History.</response>
        /// <response code="400">Error Invalid Doctor Id or Doctor already has access.</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("revoke-access")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> RevokeDoctorAccess(string doctorId)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var access = await _patientService.DoctorHasAccess(patientId, doctorId);
            if (!access)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("Access not found."));
            }

            var doctorExists = await _patientService.FindDoctor(doctorId);
            if (!doctorExists)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid doctor ID."));
            }

            await _patientService.RemoveDoctorAccess(patientId, doctorId);
            return Ok(ApiResponse<string>.SuccessResponse("Doctor access revoked."));
        }

        /// <summary>
        /// Make New Scan.
        /// </summary>
        /// <param name="scan">Patient Scan Image</param>
        /// <returns>Returns Prediction or validation errors.</returns>
        /// <response code="200">Prediction successfully</response>
        /// <response code="400">No file uploaded error</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(ApiResponse<PatientHistoryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        [HttpPost("Scan")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> Scan([FromForm] ScanDto scan)
        {
            if (scan.ScanImage == null || scan.ScanImage.Length == 0)
                return BadRequest(ApiResponse<string>.FailureResponse("No file uploaded."));

            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _patientService.GetPatientByIdAsync(patientId);

            try
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(scan.ScanImage, CloudFolder.PatientsScans);

                var prediction = await _mlModelService.Predict(scan.ScanImage);

                if (!prediction.IsSuccess)
                    return StatusCode(500, ApiResponse<string>.FailureResponse(prediction?.Result?.Prediction ?? "Please Try Again Later",
                        System.Net.HttpStatusCode.InternalServerError));

                var check = new PatientHistory()
                {
                    PatientName = patient.User.FullName,
                    Date = DateTime.UtcNow,
                    PatientId = patientId,
                    FundusCameraPath = imageUrl,
                    Patient = patient,
                    FundusCameraResult = prediction.Result.Prediction,
                    Confidence = prediction.Result.Confidence,
                    ArabicName = prediction.ArabicName,
                    DiseaseMsg = prediction.DiseaseMsg,
                };

                var res = await _patientService.AddPatientHistory(check);
                return Ok(ApiResponse<PatientHistoryDto>.SuccessResponse(res));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while Make new scan");
                return StatusCode(500, ApiResponse<string>.FailureResponse(ex.Message, System.Net.HttpStatusCode.InternalServerError));
            }

        }

        /// <summary>
        /// Get Patient History.
        /// </summary>
        /// <returns>Returns Patient History.</returns>
        /// <response code="200">List Of Patient History</response>
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientHistoryDto>>), 200)]
        [HttpGet("GetPatientHistory")]
        [Authorize(Roles = "Patient")]
        public async Task<IActionResult> GetPatientHistory(int pageNumber = 1, int pageSize = 5)
        {
            var patientId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var totalCount = await _patientService.GetPatientHistoriesCountAsync(patientId);
            var items = await _patientService.GetPatientHistoryAsync(patientId, pageNumber, pageSize);

            var pagedResult = new PagedResult<PatientHistoryDto>
            {
                Items = items.ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<PatientHistoryDto>>.SuccessResponse(pagedResult));
        }

    }
}
