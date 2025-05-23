using ClearSight.Core.Constant;
using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Dtos.BusnessDtos;
using ClearSight.Core.Interfaces.Services;
using ClearSight.Infrastructure.Implementations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClearSight.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminsController : ControllerBase
    {
        private readonly IAdminService _adminService;
        private readonly MailingService _mailService;

        public AdminsController(IAdminService adminService, MailingService mailService)
        {
            _adminService = adminService;
            _mailService = mailService;
        }

        /// <summary>
        /// Get Doctors Activate List Data.
        /// </summary>
        /// <returns>Returns Doctors Activate Data.</returns>
        /// <response code="200">Doctors Activate Data.</response>
        [ProducesResponseType(typeof(ApiResponse<DoctorActivateProfile>), 200)]
        [HttpGet("ActivateDoctorsList")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateDoctorsListAsync(int pageNumber, int pageSize)
        {
            int totalCount = await _adminService.GetDoctorsActivateProfilesCountAsync();
            var doctors = await _adminService.GetDoctorsActivateProfilesAsync(pageNumber, pageSize);

            var result = new PagedResult<DoctorActivateProfile>
            {
                Items = doctors.ToList(),
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = pageNumber
            };

            return Ok(ApiResponse<PagedResult<DoctorActivateProfile>>.SuccessResponse(result));
        }

        /// <summary>
        /// Get Doctor Activate List Data.
        /// </summary>
        /// <returns>Returns Doctor Activate Data.</returns>
        /// <response code="200">Doctor Activate Data.</response>
        [ProducesResponseType(typeof(ApiResponse<DoctorActivateProfile>), 200)]
        [HttpGet("ActivateDoctorList")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateDoctorsListAsync(string doctorId)
        {
            var doctor = await _adminService.GetDoctorActivateProfileAsync(doctorId);
            return Ok(ApiResponse<DoctorActivateProfile>.SuccessResponse(doctor));
        }
        /// <summary>
        /// Activate Doctor Account
        /// </summary>
        /// <returns>Returns Doctor Activation Result.</returns>
        /// <response code="200">Activated Success.</response>
        /// <response code="400">Activated Error</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("ActivateDoctor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateDoctor(string doctorId)
        {
            var result = await _adminService.ActivateDoctorAsync(doctorId);
            var doctor = await _adminService.GetDoctorAsync(doctorId);

            if (result.IsSuccess)
            {
                var str = new StreamReader(FilePaths.AccountVerified);
                var mailText = str.ReadToEnd();
                str.Close();

                mailText = mailText.Replace("DOCTORNAME", doctor.User.FullName);

                try
                {
                    await _mailService.SendEmailAsync(doctor.User.Email, "Account Activated Success", mailText);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                                        ApiResponse<string>.FailureResponse(ex.Message));
                }
                return Ok(ApiResponse<string>.SuccessResponse(result.Response));
            }
            return BadRequest(ApiResponse<string>.FailureResponse(result.Response));

        }
        /// <summary>
        /// Reject Doctor Account
        /// </summary>
        /// <returns>Returns Doctor Activation Result.</returns>
        /// <response code="200">Rejected Success.</response>
        /// <response code="400">Rejected Error</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("RejectDoctor")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RejectDoctor(string doctorId)
        {
            var result = await _adminService.RejectDoctorAccountAsync(doctorId);
            var doctor = await _adminService.GetDoctorAsync(doctorId);

            if (result.IsSuccess)
            {
                var str = new StreamReader(FilePaths.AccountRejected);
                var mailText = str.ReadToEnd();
                str.Close();

                mailText = mailText.Replace("DOCTORNAME", doctor.User.FullName);

                try
                {
                    await _mailService.SendEmailAsync(doctor.User.Email, "Account Rejected", mailText);
                }
                catch (Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError,
                                        ApiResponse<string>.FailureResponse(ex.Message));
                }
                return Ok(ApiResponse<string>.SuccessResponse(result.Response));
            }
            return BadRequest(ApiResponse<string>.FailureResponse(result.Response));

        }
    }
}
