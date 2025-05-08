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
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _docotrServices;
        private readonly CloudinaryService _cloudinaryService;
        private readonly MLModelService _mlModelService;
        private readonly ILogger<DoctorsController> _logger;


        public DoctorsController(CloudinaryService cloudinaryService, MLModelService mlModelService, IDoctorService docotrServices, ILogger<DoctorsController> logger)
        {
            _cloudinaryService = cloudinaryService;
            _mlModelService = mlModelService;
            _docotrServices = docotrServices;
            _logger = logger;
        }

        /// <summary>
        /// Get Patients Allowed Doctor To Access Data.
        /// </summary>
        /// <returns>Returns Patients Data.</returns>
        /// <response code="200">List Of Paginated Patients Data.</response>
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientProfileDto>>), 200)]
        [HttpGet("PatientsList")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> PatientsListAsync(int pageNumber = 1, int pageSize = 10)
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int totalCount = await _docotrServices.GetPatientsGrantAccessToDoctorCountAsync(doctorId);
            var patientsList = await _docotrServices.GetPatientsDtosAsync(doctorId, pageNumber, pageSize);

            var result = new PagedResult<PatientProfileDto>
            {
                Items = patientsList.ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<PatientProfileDto>>.SuccessResponse(result));
        }

        /// <summary>
        /// Get Patients Allowed Doctor To Access Data.
        /// </summary>
        /// <param name="patientName"> Patient FullName or Patient UserName </param>
        /// <returns>Returns Patients Data.</returns>
        /// <response code="200">List Of Paginated Patients Data.</response>
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientProfileDto>>), 200)]
        [HttpGet("PatientsListSearch")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> PatientsListSearchAsync(string patientName, int pageNumber = 1, int pageSize = 10)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int totalCount = await _docotrServices.GetPatientsGrantAccessToDoctorCountAsync(userId);
            var patientsList = await _docotrServices.GetPatientsDtosAsync(userId, patientName, pageNumber, pageSize);

            var result = new PagedResult<PatientProfileDto>
            {
                Items = patientsList.ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<PatientProfileDto>>.SuccessResponse(result));

        }


        /// <summary>
        /// Get Doctor Profile Data.
        /// </summary>
        /// <returns>Returns Doctor Profile Data.</returns>
        /// <response code="200">Doctor Profile Data.</response>
        /// <response code="400">User NotFound error</response>
        [ProducesResponseType(typeof(ApiResponse<DoctorProfileDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpGet("Profile")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Profile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var doctor = await _docotrServices.GetDoctorDtoByIdAsync(userId);

            if (doctor == null)
                return BadRequest(ApiResponse<string>.FailureResponse("User Not Found"));


            return Ok(ApiResponse<DoctorProfileDto>.SuccessResponse(doctor));
        }

        /// <summary>
        /// Edit Doctor Profile.
        /// </summary>
        /// <param name="dto">Doctor Profile details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">profile data edited successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="401">UnAuthorized User error</response>
        /// <response code="404">User NotFound error</response>
        /// <response code="500">Internal Server error</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 404)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        [HttpPost("EditProfile")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> EditProfile([FromForm] DoctorProfileEditDto dto)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await _docotrServices.GetDoctorByIdAsync(id);

            if (doctor == null)
                return BadRequest(ApiResponse<string>.FailureResponse("User Not Found"));

            try
            {
                await _docotrServices.UpdateDoctor(doctor, dto);
                return Ok(ApiResponse<string>.SuccessResponse("Updated Successfully"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "error while edit doctor profile data");
                return BadRequest(ApiResponse<string>.FailureResponse(ex.Message));
            }
        }



        /// <summary>
        /// Make New Scan.
        /// </summary>
        /// <param name="dto">Patient Scan Image</param>
        /// <param name="patientId">Patient Id</param>
        /// <returns>Returns Prediction or validation errors.</returns>
        /// <response code="200">Prediction successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(ApiResponse<PatientHistoryDto>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        [HttpPost("Scan/{patientId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> Scan(string patientId, ScanDto dto)
        {
            if (dto.ScanImage == null || dto.ScanImage.Length == 0)
                return BadRequest(ApiResponse<string>.FailureResponse("No file uploaded."));

            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var doctor = await _docotrServices.GetDoctorByIdAsync(doctorId);
            var patient = await _docotrServices.GetPatientByIdAsync(patientId);

            if (patient == null)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("Patient Not Found"));
            }

            try
            {
                var imageUrl = await _cloudinaryService.UploadImageAsync(dto.ScanImage, CloudFolder.DoctorsScans);

                var prediction = await _mlModelService.Predict(dto.ScanImage);

                if (!prediction.IsSuccess)
                    return StatusCode(500, ApiResponse<string>.FailureResponse(prediction?.Result?.Prediction ?? "Error"));


                var Check = new PatientHistory()
                {
                    PatientName = patient.User.UserName,
                    Doctor = doctor,
                    Date = DateTime.UtcNow,
                    PatientId = patientId,
                    FundusCameraPath = imageUrl,
                    Patient = patient,
                    FundusCameraResult = prediction.Result.Prediction,
                    Confidence = prediction.Result.Confidence,
                    ArabicName = prediction.ArabicName,
                    DiseaseMsg = prediction.DiseaseMsg,
                };

                var res = await _docotrServices.AddPatientHistory(Check);

                return Ok(ApiResponse<PatientHistoryDto>.SuccessResponse(res));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An Error Occured While Handle New Scan For Doctor");
                return StatusCode(500, ApiResponse<string>.FailureResponse(ex.Message));
            }

        }

        /// <summary>
        /// Get Patient History.
        /// </summary>
        /// <param name="patientId"> Patient Id </param>
        /// <returns>Returns Patient History.</returns>
        /// <response code="200">List Of Patient History</response>
        /// <response code="400">Patient Not Found</response>
        /// <response code="403">Docotr Doesn't Have Access To This Patient History</response>
        [ProducesResponseType(typeof(ApiResponse<PagedResult<PatientHistoryDto>>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 403)]
        [HttpGet("GetPatientHistory/{patientId:guid}")]
        [Authorize(Roles = "Doctor")]
        public async Task<IActionResult> GetPatientHistory(string patientId, int pageNumber = 1, int pageSize = 5)
        {
            var doctorId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var patient = await _docotrServices.GetPatientByIdAsync(patientId);

            if (patient == null)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("Patient Not Found"));

            }
            var IsAuthenticatedDoctor = await _docotrServices.IsAuthenticated(doctorId, patientId);
            if (!IsAuthenticatedDoctor)
            {
                return StatusCode(StatusCodes.Status403Forbidden,
                    ApiResponse<string>.FailureResponse("You Don't Have Access To This Patient History",
                    System.Net.HttpStatusCode.Forbidden));
            }
            var totalCount = await _docotrServices.GetPatientHistoriesCountAsync(patientId);
            var patientHistory = await _docotrServices.GetPatientHistoryAsync(patientId, pageNumber, pageSize);

            var pagedResult = new PagedResult<PatientHistoryDto>
            {
                Items = patientHistory.ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<PatientHistoryDto>>.SuccessResponse(pagedResult));
        }
    }
}
