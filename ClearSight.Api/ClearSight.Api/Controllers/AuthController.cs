using ClearSight.Core.Constant;
using ClearSight.Core.Dtos.ApiResponse;
using ClearSight.Core.Dtos.AuthenticationDtos;
using ClearSight.Core.Models;
using ClearSight.Infrastructure.Context;
using ClearSight.Infrastructure.Implementations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClearSight.Api.Controllers
{
    /// <summary>
    /// Authentication Controller For Login , Register ,Reset Password and more
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly AuthenticationService _authenticationService;
        private readonly MailingService _emailSender;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly AppDbContext _context;
        private readonly ActivateUserAccountsServices _activateUserAccounts;
        private readonly GenerateCodeServices _generateCodeServices;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AuthenticationService AuthenticationService, MailingService emailSender, UserManager<User> userManager, SignInManager<User> signInManager, AppDbContext appDbContext, ActivateUserAccountsServices activateUserAccounts, GenerateCodeServices generateCodeServices, IConfiguration configuration, ILogger<AuthController> logger)
        {
            _authenticationService = AuthenticationService;
            _emailSender = emailSender;
            _userManager = userManager;
            _signInManager = signInManager;
            _context = appDbContext;
            _activateUserAccounts = activateUserAccounts;
            _generateCodeServices = generateCodeServices;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Generate a new userName.
        /// </summary>
        /// <param name="userName">UserName.</param>
        /// <returns>Returns unused UserName.</returns>
        /// <response code="200">UserName</response>
        [ProducesResponseType(200)]
        [HttpPost("generateUserName")]
        public IActionResult GenerateUserName(string userName)
        {
            string name = userName.Trim().Replace(" ", "");
            bool IsUsedName = _userManager.Users.Any(u => u.UserName == name);
            while (IsUsedName)
            {
                var randNum = new Random().Next(100_000, 999_999);
                name = name + "_" + randNum;
                IsUsedName = _userManager.Users.Any(x => x.UserName == name);
            }
            return Ok(new { UserName = name });
        }

        /// <summary>
        /// Registers a new user.
        /// </summary>
        /// <param name="model">User registration details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="500">Internal server error</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 500)]
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] RegisterModel model)
        {
            var registrationModel = await _authenticationService.RegisterUserAsync(model);

            if (registrationModel.Message != null)
                return BadRequest(ApiResponse<string>.FailureResponse(registrationModel.Message));

            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest(ApiResponse<string>.FailureResponse("Email Not Found"));

            var token = await _authenticationService.GenerateEmailConfirmationTokenAsync(user.Id);

            var confirmationLink = Url.Action(nameof(ConfirmEmail), "Auth", new { userId = user.Id, token }, Request.Scheme);

            var str = new StreamReader(FilePaths.EmailConfirmation);
            var mailText = str.ReadToEnd();
            str.Close();

            mailText = mailText.Replace("RESET_LINK", confirmationLink);
            mailText = mailText.Replace("PATIENT_NAME", user.FullName);

            try
            {
                await _emailSender.SendEmailAsync(model.Email, "Confirm your email", mailText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login Error");
                await _userManager.DeleteAsync(user);
                return StatusCode(StatusCodes.Status500InternalServerError,
                                    ApiResponse<string>.FailureResponse(ex.Message, System.Net.HttpStatusCode.InternalServerError));
            }
            return Ok(ApiResponse<string>.SuccessResponse("Registration Email has send successful Check Your Mail To Confirm Account."));
        }

        /// <summary>
        /// Login to system.
        /// </summary>
        /// <param name="model">User Login details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">User logined successfully</response>
        /// <response code="400">Validation error</response>
        /// <response code="401">UnAuthorized User error</response>
        [ProducesResponseType(typeof(ApiResponse<AuthenticationModel>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<string>), 401)]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromForm] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);

            if (user == null)
                return BadRequest(ApiResponse<string>.FailureResponse("Email Not Registerd"));

            if (await _userManager.IsLockedOutAsync(user))
                return BadRequest(ApiResponse<string>.FailureResponse("Your account is locked. Please try again later."));


            if (!user.EmailConfirmed)
                return BadRequest(ApiResponse<string>.FailureResponse("PLZ Confirm Your Mail First"));

            await _activateUserAccounts.ActivateUserAccount(user);

            var result = await _authenticationService.LoginUserAsync(model);

            if (!result.IsAuthenticated)
                return BadRequest(ApiResponse<string>.FailureResponse(result.ErrorMessage));

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                var cookieOptions = _authenticationService.SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
                Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);
            }

            return Ok(ApiResponse<AuthenticationModel>.SuccessResponse(result));
        }

        /// <summary>
        /// Change User Password.
        /// </summary>
        /// <param name="model">change password details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">Change Password successfully</response>
        /// <response code="400">Validation error Note(return errors as String comma Separator)</response>
        /// <response code="400">User not found.</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [ProducesResponseType(typeof(ApiResponse<List<IdentityError>>), 400)]
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest(ApiResponse<string>.FailureResponse("User not found."));
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {

                user.RefreshTokens = null;
                await _userManager.UpdateAsync(user);

                return Ok(ApiResponse<string>.SuccessResponse("Password has been Change successfully."));
            }

            return BadRequest(ApiResponse<List<IdentityError>>.FailureResponse(string.Join(',', result.Errors)));
        }


        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
                return BadRequest("Invalid email confirmation request.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return BadRequest("No User Found");

            if (user.EmailConfirmed)
            {
                var str = new StreamReader(FilePaths.EmailConfirmatedBefore);
                var mailText = str.ReadToEnd();
                str.Close();
                return Content(mailText, "text/html");
            }

            var result = await _authenticationService.ConfirmEmailAsync(userId, token);

            if (result)
            {
                var str = new StreamReader(FilePaths.EmailConfirmationSuccess);
                var mailText = str.ReadToEnd();
                str.Close();
                mailText = mailText.Replace("FRONTENDLOGIN_URL", _configuration["FRONTENDLOGIN_URL"]);
                return Content(mailText, "text/html");

            }
            return BadRequest("Error confirming your email.");
        }

        /// <summary>
        /// Get Change Password Code.
        /// </summary>
        /// <param name="email">User Mail To Get Verivication Code.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">Code Send To Your Mail successfully</response>
        /// <response code="400">Email not found error</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpGet("GetCode")]
        public async Task<IActionResult> GetCode(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
                return NotFound(ApiResponse<string>.FailureResponse("Email not found."));

            var code = Guid.NewGuid().ToString()[..5].ToUpper();
            string hexCode = _generateCodeServices.GenerateCode(code);

            var userCode = new UserCode
            {
                Code = hexCode,
                ExpirationTime = DateTime.UtcNow.AddMinutes(15),
                User = user,
            };
            _context.Add(userCode);
            _context.SaveChanges();

            var str = new StreamReader(FilePaths.ResetPasswordCode);
            var mailText = str.ReadToEnd();
            str.Close();

            mailText = mailText.Replace("USER_NAME", user.FullName);
            mailText = mailText.Replace("{CODE}", code);

            await _emailSender.SendEmailAsync(user.Email, "Code", mailText);
            return Ok(ApiResponse<string>.SuccessResponse("Verification code sent to your email."));
        }

        /// <summary>
        /// Reset User Password.
        /// </summary>
        /// <param name="resetPasswordDto">Reset password details.</param>
        /// <returns>Returns success message or validation errors.</returns>
        /// <response code="200">Reset Password successfully</response>
        /// <response code="400">Invalid or expired verification code.</response>
        /// <response code="400">User not found.</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPasswordDto)
        {
            string hashedCode = _generateCodeServices.GenerateCode(resetPasswordDto.VerificationCode);

            var resetRequest = await _context.UserCodes
                .FirstOrDefaultAsync(r =>
                    r.User.Email == resetPasswordDto.Email &&
                    r.Code == hashedCode &&
                    r.ExpirationTime >= DateTime.UtcNow &&
                   !r.IsUsed);

            if (resetRequest == null)
                return BadRequest(ApiResponse<string>.FailureResponse("Invalid or expired verification code."));

            // Update the user's password
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetPasswordDto.Email);
            if (user == null)
                return BadRequest(ApiResponse<string>.FailureResponse("User not found."));


            user.PasswordHash = _userManager.PasswordHasher.HashPassword(user, resetPasswordDto.NewPassword);
            resetRequest.IsUsed = true;

            await _userManager.UpdateSecurityStampAsync(user);
            _context.Users.Update(user);
            _context.UserCodes.Update(resetRequest);
            await _context.SaveChangesAsync();

            return Ok(ApiResponse<string>.SuccessResponse("Password reset successful."));
        }

        /// <summary>
        /// Call this api to get new token if your refresh token still valid
        /// </summary>
        /// <returns>Returns  refresh token or validation errors.</returns>
        /// <response code="200">Returns refresh token successfully</response>
        /// <response code="400">Invalid refresh token, login again</response>
        [ProducesResponseType(typeof(ApiResponse<AuthenticationModel>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized(ApiResponse<string>.FailureResponse("UnAuthorized", System.Net.HttpStatusCode.Unauthorized));

            await _userManager.UpdateSecurityStampAsync(user);

            var refreshToken = Request.Cookies["refreshToken"];

            var result = await _authenticationService.RefreshTokenAsync(refreshToken);

            if (!result.IsAuthenticated)
                return BadRequest(ApiResponse<string>.FailureResponse("Refresh Token InValid"));

            var cookieOptions = _authenticationService.SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);
            Response.Cookies.Append("refreshToken", result.RefreshToken, cookieOptions);



            return Ok(ApiResponse<AuthenticationModel>.SuccessResponse(result));
        }

        /// <summary>
        /// Call this api to revoke your refresh token 
        /// </summary>
        /// <param name="model"></param>
        /// <returns>Returns  success or validation errors.</returns>
        /// <response code="200">Token Revoked Successfully</response>
        /// <response code="400">Invalid refresh token login again</response>
        [ProducesResponseType(typeof(ApiResponse<string>), 200)]
        [ProducesResponseType(typeof(ApiResponse<string>), 400)]
        [HttpDelete("revokeToken")]
        public async Task<IActionResult> RevokeToken()
        {
            var token = Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(ApiResponse<string>.FailureResponse("Token is required!"));

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return Unauthorized(ApiResponse<string>.FailureResponse("UnAuthorized", System.Net.HttpStatusCode.Unauthorized));

            await _userManager.UpdateSecurityStampAsync(user);

            var result = await _authenticationService.RevokeTokenAsync(token);

            if (!result)
                return BadRequest(ApiResponse<string>.FailureResponse("Token is invalid!"));

            return Ok(ApiResponse<string>.SuccessResponse("Token Revoked Successfully"));
        }
    }
}
